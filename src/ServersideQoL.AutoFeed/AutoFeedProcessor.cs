using ServersideQoL.Processors;
using ServersideQoL.Utilities;

namespace ServersideQoL.AutoFeed;

[Processor(Id)]
[RunAfter<TameableRegistryProcessor>]
[RunAfter<ContainerRegistryProcessor>]
public sealed class AutoFeedProcessor : Processor<TameableRegistryProcessor.PrefabInfo>
{
    public const string Id = "18fb793d-319a-4792-a924-157f4dc3ebb7";

    const float NoFoodRetrySeconds = 10f;

    SectorDictionary<SharedItemDataKey, HashSet<ServersideQoLZDO>>? _containersByItemName;
    List<ServersideQoLZDO>? _staleContainers;

    protected override void Initialize()
    {
        _containersByItemName = Instance<ContainerRegistryProcessor>()
            .GetContainersByItemName(Math.Max(Config.Instance.ContainerRange.Value, 1f));
    }

    protected override ProcessResult Process(ServersideQoLZDO zdo, IReadOnlyList<Peer> peers, TameableRegistryProcessor.PrefabInfo prefabInfo)
    {
        if (_containersByItemName is null)
            return ProcessResult.UnregisterProcessor;

        // Tamed creatures only; wild/taming ones are reprocessed whenever their ZDO changes,
        // so a fresh tame is picked up as soon as its 'tamed' var flips.
        if (Instance<TameableRegistryProcessor>().GetState(zdo) is not { State: TameableState.States.Tamed })
            return default;

        /// <see cref="Tameable.IsHungry()"/>
        var fedDuration = zdo.Fields<Tameable>().GetFloat(static () => x => x.m_fedDuration);
        var untilHungry = FeedingLogic.SecondsUntilHungry(ZNet.instance.GetTime(), zdo.Vars.GetTameLastFeeding(), fedDuration);
        if (untilHungry > 0)
            return ScheduleReprocessing((float)untilHungry + 1f);

        var result = ProcessResult.Default;
        var pos = zdo.ZDO.GetPosition();
        var rangeSqr = Config.Instance.ContainerRange.Value * Config.Instance.ContainerRange.Value;

        foreach (var consumeItem in prefabInfo.MonsterAI.m_consumeItems)
        {
            var consumeName = consumeItem.m_itemData.m_shared.m_name;
            foreach (var containers in _containersByItemName.EnumerateAdjacent((pos, (SharedItemDataKey)consumeItem.m_itemData)))
            {
                var fed = false;
                foreach (var containerZdo in containers)
                {
                    if (Instance<ContainerRegistryProcessor>().GetState(containerZdo) is not { } containerState)
                    {
                        (_staleContainers ??= []).Add(containerZdo);
                        continue;
                    }

                    if (containerZdo.Vars.GetInUse())
                        continue; // a player has the chest open

                    if (Utils.DistanceSqr(pos, containerZdo.ZDO.GetPosition()) > rangeSqr)
                        continue;

                    var inventory = containerState.GetInventory();
                    var slotIdx = FeedingLogic.SelectSlot(inventory.Items,
                        static x => x.m_shared.m_name, static x => x.m_stack, consumeName);
                    if (slotIdx < 0)
                    {
                        // no (more) matching food in this container: drop it from this item's index
                        (_staleContainers ??= []).Add(containerZdo);
                        continue;
                    }

                    if (!containerZdo.IsOwnerOrUnassigned())
                    {
                        // chest is owned by a client; request ownership and retry shortly
                        result |= ScheduleReprocessing(
                            Instance<ContainerRegistryProcessor>().RequestOwnership(containerZdo, default, containerState));
                        continue;
                    }

                    var slot = inventory.Items[slotIdx];
                    slot.m_stack -= 1;
                    if (slot.m_stack is 0)
                        inventory.Items.Remove(slot);
                    inventory.Save();

                    /// <see cref="Tameable.OnConsumedItem"/> — reset the hunger timer.
                    // The creature is a moving ZDO owned by a nearby client: release ownership
                    // and get ahead of the owner's data revisions so the change sticks (same
                    // pattern the core uses when saving inventories of moving ZDOs).
                    zdo.ReleaseOwnership();
                    zdo.Vars.SetTameLastFeeding(ZNet.instance.GetTime());
                    zdo.ZDO.DataRevision += 120;
                    ZDOMan.instance.ForceSendZDO(zdo.ZDO.m_uid);

                    Logger.LogInfo($"AutoFeed: fed {prefabInfo.PrefabInfo.PrefabName} at {pos} with {consumeName} from container at {containerZdo.ZDO.GetPosition()}");
                    fed = true;
                    break;
                }

                RemoveStale(containers);
                if (fed)
                    return result | ScheduleReprocessing(Math.Max(fedDuration, 1f));
            }
        }

        // hungry, but nothing edible in range — retry soon
        return result | ScheduleReprocessing(NoFoodRetrySeconds);
    }

    void RemoveStale(HashSet<ServersideQoLZDO> containers)
    {
        if (_staleContainers is not { Count: > 0 })
            return;
        foreach (var containerZdo in _staleContainers)
            containers.Remove(containerZdo);
        _staleContainers.Clear();
    }
}
