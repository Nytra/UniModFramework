namespace UniModFramework;

public abstract partial class UniPrePatcher<T, TConfig> where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    protected static TConfig? Config;
    private static Func<Feature, bool>? _featureChecker;
    private Action<string>? _infoLogger;

    /// <summary>
    /// Called when the patcher initializes.
    /// </summary>
    /// <returns>Whether the patching was successful.</returns>
    protected abstract bool OnInitialize();

    /// <summary>
    /// Checks if the modding environment has certain features.
    /// </summary>
    /// <param name="feature">The feature to check for.</param>
    /// <returns>Whether the feature is available.</returns>
    protected static bool HasFeature(Feature feature) => _featureChecker?.Invoke(feature) ?? false;

    /// <summary>
    /// Attempts to send an info message to the log (if available).
    /// </summary>
    /// <param name="msg">The message to send.</param>
    protected void LogInfo(string msg) => _infoLogger?.Invoke(msg);

    /// <summary>
    /// Called when all patchers have been applied.
    /// </summary>
    /// <returns>Whether the patching was successful.</returns>
    protected virtual bool OnFinalize() => true;
}