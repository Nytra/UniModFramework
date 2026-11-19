using HarmonyLib;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    private static TConfig? _config;

    /// <summary>
    /// The config for this UniMod. Available from <see cref="OnLoad"/> onwards.
    /// </summary>
#pragma warning disable CS0109 // Only some loaders have a Config member to hide
    protected static new TConfig Config
#pragma warning restore CS0109
    {
        get => _config ?? throw new InvalidOperationException($"The config of {typeof(T).Name} was used before the mod was loaded");
        private set => _config = value;
    }

    /// <summary>
    /// Called when the mod is first loaded. This may happen before the game itself is loaded.
    /// </summary>
    /// <param name="harmony">The Harmony instance to be used for patching.</param>
    /// <returns>Whether the patching was successful.</returns>
    protected abstract bool OnLoad(Harmony harmony);

    /// <summary>
    /// Called when the game is ready.
    /// </summary>
    /// <returns>Whether the patching was successful.</returns>
    protected virtual bool OnReady() => true;

    /// <summary>
    /// Checks if the modding environment has certain features.
    /// </summary>
    /// <param name="feature">The feature to check for.</param>
    /// <returns>Whether the feature is available.</returns>
    protected static bool HasFeature(Feature feature) => Backend.HasFeature(feature);

    /// <summary>
    /// Sends a message to the mod loader's log.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    protected static void LogDebug(string msg) => WriteLog(LogLevel.Debug, msg);

    /// <inheritdoc cref="LogDebug"/>
    protected static void LogInfo(string msg) => WriteLog(LogLevel.Info, msg);

    /// <inheritdoc cref="LogDebug"/>
    protected static void LogWarn(string msg) => WriteLog(LogLevel.Warn, msg);

    /// <inheritdoc cref="LogDebug"/>
    protected static void LogError(string msg) => WriteLog(LogLevel.Error, msg);

    private static partial void WriteLog(LogLevel level, string message);

    /// <summary>
    /// Called by the backend once the config is loaded.
    /// </summary>
    /// <returns>Whether the mod loaded and should get <see cref="OnReady"/>.</returns>
    private bool RunLoad(Harmony harmony) =>
        Backend.CanStart(typeof(T), _config, WriteLog) && Backend.Report(OnLoad(harmony), typeof(T), nameof(OnLoad), WriteLog);

    private bool RunReady() => Backend.Report(OnReady(), typeof(T), nameof(OnReady), WriteLog);
}
