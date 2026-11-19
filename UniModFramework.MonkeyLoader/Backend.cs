using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;

namespace UniModFramework;

internal static partial class Backend
{
    internal static partial bool HasFeature(Feature feature) => feature switch
    {
        Feature.PrePatching => true,
        _ => false
    };

    internal static void Log(Logger? logger, LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Debug: logger?.Debug(() => message); break;
            case LogLevel.Info: logger?.Info(() => message); break;
            case LogLevel.Warn: logger?.Warn(() => message); break;
            case LogLevel.Error: logger?.Error(() => message); break;
        }
    }

    /// <summary>
    /// Keys are initialized before the saved values are read, so if reading them fails the section is still usable with defaults.
    /// </summary>
    internal static TSection LoadSection<TSection>(MonkeyLoader.Configuration.Config config, Action<LogLevel, string> log) where TSection : Config, new()
    {
        var section = new TSection();
        try
        {
            config.LoadSection(section);
        }
        catch (ConfigLoadException ex)
        {
            log(LogLevel.Warn, $"Failed to load config, using defaults: {ex.Message}");
        }
        return section;
    }
}
