using BepInEx.Logging;
using BepInEx.Preloader.Core.Patching;

namespace UniModFramework;

public abstract partial class UniPrePatcher<T, TConfig> : BasePatcher where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    private static ManualLogSource? _logSource;
    // BepInEx runs the patch methods even if this fails
    public override void Initialize()
    {
        Config = new();
        Config.Bind(base.Config);
        RunInitialize();
    }
    public override void Finalizer()
    {
        RunFinalize();
    }
    public UniPrePatcher()
    {
        _logSource = Log;
    }
    private static partial void WriteLog(LogLevel level, string message) => Backend.Log(_logSource, level, message);
}
