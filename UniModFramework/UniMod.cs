using HarmonyLib;

namespace UniModFramework;

public abstract partial class UniMod<T> where T : UniMod<T>, new()
{
    public static Func<Feature, bool>? FeatureChecker;
    public Action<string>? InfoLogger;
    protected abstract bool OnLoad(Harmony harmony);
    public static bool HasFeature(Feature feature) => FeatureChecker?.Invoke(feature) ?? false;
    protected void LogInfo(string msg) => InfoLogger?.Invoke(msg);
}