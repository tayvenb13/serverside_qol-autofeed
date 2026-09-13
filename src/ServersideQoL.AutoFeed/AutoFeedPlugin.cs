using BepInEx;
using BepInEx.Configuration;

namespace ServersideQoL.AutoFeed;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(ServersideQoLPlugin.PluginGuid, ServersideQoLPlugin.PluginVersion)]
public sealed partial class AutoFeedPlugin : ServersideQoLPluginBase<AutoFeedPlugin, Config>
{
    public const string Author = "tayvenb13";
    public const string PluginName = "ServersideQoL.AutoFeed";
    public const string PluginGuid = $"{Author}.{PluginName}";

    protected override Config CreateConfigSingleton(ConfigFile configFile, Logger logger) => new(configFile, logger);

    protected override void RegisterProcessors(IProcessorCollection processors) => processors
        .Add<AutoFeedProcessor>();
}
