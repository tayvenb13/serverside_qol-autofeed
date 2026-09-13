using BepInEx.Configuration;

namespace ServersideQoL.AutoFeed;

public sealed class Config(ConfigFile cfg, Logger logger) : ConfigBase<Config>(cfg, logger)
{
    const string Section = "AutoFeed";

    public override ConfigEntry<bool> Enabled { get; } = BindEx(cfg, Section, false,
        "Enables/disables the entire mod");
    public ConfigEntry<float> ContainerRange { get; } = BindEx(cfg, Section, 10f,
        "Radius in meters around a hungry tamed creature in which containers are searched for its food");
}
