namespace UniModFramework;

public abstract partial class UniPrePatcher<T, TConfig> where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    private static TConfig? _config;

    /// <summary>
    /// The config for this UniPrePatcher. Available from <see cref="OnInitialize"/> onwards.
    /// </summary>
#pragma warning disable CS0109 // Only some loaders have a Config member to hide
    protected static new TConfig Config
#pragma warning restore CS0109
    {
        get => _config ?? throw new InvalidOperationException($"The config of {typeof(T).Name} was used before the patcher was initialized");
        private set => _config = value;
    }

    /// <summary>
    /// Called when the patcher initializes.
    /// </summary>
    /// <returns>Whether the patching was successful.</returns>
    protected abstract bool OnInitialize();

    /// <summary>
    /// Called when all patchers have been applied.
    /// </summary>
    /// <returns>Whether the patching was successful.</returns>
    protected virtual bool OnFinalize() => true;

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
    /// <returns>Whether the patcher initialized and should patch.</returns>
    private bool RunInitialize()
    {
        if ((Backend.CheckVersion(typeof(T)) ?? Backend.CheckFeatures(typeof(T))) is string error)
        {
            LogError(error);
            return false;
        }

        foreach (var warning in _config?.ResetInvalidValues() ?? [])
            LogWarn(warning);

        return Report(OnInitialize(), nameof(OnInitialize));
    }

    private bool RunFinalize() => Report(OnFinalize(), nameof(OnFinalize));

    private static bool Report(bool success, string step)
    {
        if (!success)
            LogError($"{step} failed");
        return success;
    }
}
