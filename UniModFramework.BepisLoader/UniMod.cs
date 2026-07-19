using System.Reflection;
using BepInEx.NET.Common;
using BepisResoniteWrapper;
using HarmonyLib;

namespace UniModFramework;

public abstract partial class UniMod<T> : BasePlugin where T : UniMod<T>, new()
{
    public override void Load()
    {
        OnLoad(HarmonyInstance);
        var engineReadyHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineReady");
        ResoniteHooks.OnEngineReady += () => engineReadyHook?.Invoke(this, []);
    }
    public UniMod()
    {
        InfoLogger = (string str) => Log.LogInfo(str);
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