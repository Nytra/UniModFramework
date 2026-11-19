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
        Config.Bind(base.Config);
        if (RunLoad(HarmonyInstance))
            ResoniteHooks.OnEngineReady += () => RunReady();
    }
    public UniMod()
    {
        _logSource = Log;
    }
    private static partial void WriteLog(LogLevel level, string message) => Backend.Log(_logSource, level, message);
}
