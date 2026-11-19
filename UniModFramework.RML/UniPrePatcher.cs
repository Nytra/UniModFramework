using ResoniteModLoader;

namespace UniModFramework;

// RML has no pre-patching, so nothing runs UniPrePatchers
public abstract partial class UniPrePatcher<T, TConfig> where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    private static partial void WriteLog(LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Debug: ResoniteMod.Debug(message); break;
            case LogLevel.Info: ResoniteMod.Msg(message); break;
            case LogLevel.Warn: ResoniteMod.Warn(message); break;
            case LogLevel.Error: ResoniteMod.Error(message); break;
        }
    }
}
