using ResoniteModLoader;

namespace UniModFramework;

internal static partial class Backend
{
    internal static partial bool HasFeature(Feature feature) => feature switch
    {
        Feature.PrePatching => false,
        _ => false
    };

    // RML works out the calling mod from the stack trace, skipping this framework's frames
    internal static void Log(LogLevel level, string message)
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
