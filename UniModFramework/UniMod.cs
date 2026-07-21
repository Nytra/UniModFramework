using HarmonyLib;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    protected static TConfig? Config;
    private static Func<Feature, bool>? _featureChecker;
    private Action<string>? _infoLogger;

    /// <summary>
    /// Called when the mod is first loaded. This may happen before the game itself is loaded.
    /// </summary>
    /// <param name="harmony">The Harmony instance to be used for patching.</param>
    /// <returns>Whether the patching was successful.</returns>
    protected abstract bool OnLoad(Harmony harmony);

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
    /// Called when the game is ready.
    /// </summary>
    /// <returns>Whether the patching was successful.</returns>
    protected virtual bool OnReady() => true;
}