using System.Reflection;
using HarmonyLib;
using MonkeyLoader.Resonite;

namespace UniModFramework;

public abstract partial class UniMod<T> : ResoniteMonkey<T> where T : UniMod<T>, new()
{
    protected override bool OnLoaded() => OnLoad(Harmony);
    protected override bool OnEngineInit()
    {
        var engineInitHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineInit");
        engineInitHook?.Invoke(this, []);
        return true;
    }
    protected override bool OnEngineReady()
    {
        var engineReadyHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineReady");
        engineReadyHook?.Invoke(this, []);
        return true;
    }
    public UniMod()
    {
        InfoLogger = (string str) => Logger.Info(() => str);
    }
    static UniMod()
    {
        FeatureChecker = (Feature feature) =>
        {
            switch (feature)
            {
                case Feature.PrePatching:
                    return true;
            }
            return false;
        };
    }
}