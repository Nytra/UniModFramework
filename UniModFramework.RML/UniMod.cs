using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;

namespace UniModFramework;

public abstract class UniMod<T> : ResoniteMod where T : UniMod<T>, new()
{
    public override string Name => typeof(T).GetCustomAttribute<MetadataAttribute>()!.Name;
    public override string Version => typeof(T).GetCustomAttribute<MetadataAttribute>()!.Version;
    public override string Author => typeof(T).GetCustomAttribute<MetadataAttribute>()!.Author;
    public override string Link => typeof(T).GetCustomAttribute<MetadataAttribute>()!.Link;
    protected abstract bool OnLoad(Harmony harmony);
    public override void OnEngineInit()
    {
        var harmony = new Harmony(typeof(T).GetCustomAttribute<MetadataAttribute>()!.GUID);
        OnLoad(harmony);
        var engineInitHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineInit");
        engineInitHook?.Invoke(this, []);
        var engineReadyHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineReady");
        Engine.Current.OnReady += () => engineReadyHook?.Invoke(this, []);
    }
    public static bool HasFeature(Feature feature)
    {
        switch (feature)
        {
            case Feature.PrePatching:
                return false;
        }
        return false;
    }
    protected void LogInfo(string msg)
    {
        Msg(msg);
    }
}