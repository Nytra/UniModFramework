using BepInEx.Logging;

namespace UniModFramework;

internal static partial class Backend
{
    internal static partial bool HasFeature(Feature feature) => feature switch
    {
        Feature.PrePatching => true,
        _ => false
    };

    internal static void Log(ManualLogSource? logSource, LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Debug: logSource?.LogDebug(message); break;
            case LogLevel.Info: logSource?.LogInfo(message); break;
            case LogLevel.Warn: logSource?.LogWarning(message); break;
            case LogLevel.Error: logSource?.LogError(message); break;
        }
    }
}
