namespace ServersideQoL.AutoFeed;

public static class FeedingLogic
{
    /// <summary>Seconds until the creature becomes hungry; &lt;= 0 means hungry now.</summary>
    public static double SecondsUntilHungry(DateTime now, DateTime lastFeeding, float fedDuration)
        => fedDuration - (now - lastFeeding).TotalSeconds;

    /// <summary>
    /// Index of the slot to consume from: the slot matching <paramref name="consumeName"/>
    /// with the smallest positive stack (frees container slots fastest), or -1 if none.
    /// </summary>
    public static int SelectSlot<T>(IReadOnlyList<T> slots, Func<T, string> name, Func<T, int> stack, string consumeName)
    {
        var best = -1;
        for (var i = 0; i < slots.Count; i++)
        {
            var s = stack(slots[i]);
            if (s <= 0 || name(slots[i]) != consumeName)
                continue;
            if (best < 0 || s < stack(slots[best]))
                best = i;
        }
        return best;
    }
}
