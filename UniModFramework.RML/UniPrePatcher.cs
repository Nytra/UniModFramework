namespace UniModFramework;

// RML has no pre-patching, so nothing runs UniPrePatchers
public abstract partial class UniPrePatcher<T, TConfig> where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    private static partial void WriteLog(LogLevel level, string message) => Backend.Log(level, message);
}
