using HarmonyLib;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    protected new static TConfig Config;
    private static Func<Feature, bool>? _featureChecker;
    private Action<string>? _infoLogger;
    protected abstract bool OnLoad(Harmony harmony);
    public static bool HasFeature(Feature feature) => _featureChecker?.Invoke(feature) ?? false;
    protected void LogInfo(string msg) => _infoLogger?.Invoke(msg);
    static UniMod() 
    {
        Config = new();
    }
}