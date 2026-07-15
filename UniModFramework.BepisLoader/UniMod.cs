using System.Reflection;
using BepInEx.NET.Common;
using BepisResoniteWrapper;
using HarmonyLib;

namespace UniModFramework;

public abstract class UniMod<T> : BasePlugin where T : UniMod<T>, new()
{
    protected abstract bool OnLoad(Harmony harmony);
    public override void Load()
    {
        OnLoad(HarmonyInstance);
        var engineReadyHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineReady");
        ResoniteHooks.OnEngineReady += () => engineReadyHook?.Invoke(this, []);
    }
    public static bool HasFeature(Feature feature)
    {
        switch (feature)
        {
            case Feature.PrePatching:
                return true;
        }
        return false;
    }
    protected void LogInfo(string msg)
    {
        Log.LogInfo(msg);
    }
}