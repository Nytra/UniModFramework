using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MonkeyLoader.Resonite;

namespace UniModFramework;

public abstract class UniMod<T> : ResoniteMonkey<T> where T : UniMod<T>, new()
{
    protected abstract bool OnLoad();
    protected override bool OnLoaded() => OnLoad();
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
    public static bool HasFeature(Feature feature)
    {
        switch (feature)
        {
            case Feature.PrePatching:
                return true;
        }
        return false;
    }
    protected void Log(string msg)
    {
        Logger.Info(() => msg);
    }
    protected void PatchAll()
    {
        Harmony.PatchAll(typeof(T).Assembly);
    }
}