using BepInEx.Logging;
using BepInEx.NET.Common;
using BepisResoniteWrapper;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> : BasePlugin where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    private static ManualLogSource? _logSource;
    public override void Load()
    {
        Config = new();
        foreach (var key in Config.ConfigurationKeys.OfType<IBindableKey>())
            key.Bind(base.Config);
        if (RunLoad(HarmonyInstance))
            ResoniteHooks.OnEngineReady += () => RunReady();
    }
    public UniMod()
    {
        _logSource = Log;
    }
    private static partial void WriteLog(LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Debug: _logSource?.LogDebug(message); break;
            case LogLevel.Info: _logSource?.LogInfo(message); break;
            case LogLevel.Warn: _logSource?.LogWarning(message); break;
            case LogLevel.Error: _logSource?.LogError(message); break;
        }
    }
}
