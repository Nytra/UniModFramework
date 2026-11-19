using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> : ConfiguredResoniteMonkey<T, TConfig> where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    protected override bool OnLoaded()
    {
        // Keys are initialized before the saved values are read, so if reading them fails the section is still usable with defaults.
        Config = new();
        try
        {
            ConfiguredResoniteMonkey<T, TConfig>.Config.LoadSection(Config);
        }
        catch (ConfigLoadException ex)
        {
            LogWarn($"Failed to load config, using defaults: {ex.Message}");
        }
        return RunLoad(Harmony);
    }
    protected override bool OnEngineReady() => RunReady();
    private static partial void WriteLog(LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Debug: Logger?.Debug(() => message); break;
            case LogLevel.Info: Logger?.Info(() => message); break;
            case LogLevel.Warn: Logger?.Warn(() => message); break;
            case LogLevel.Error: Logger?.Error(() => message); break;
        }
    }
}
