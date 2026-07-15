using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;

namespace UniModFramework;

public abstract partial class UniMod<T> : ResoniteMod where T : UniMod<T>, new()
{
    protected abstract bool OnLoad();
    public override string Name => typeof(T).Assembly.GetName().Name;
    public override string Version => typeof(T).Assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "0.0.0";
    public override string Author => typeof(T).Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Unknown";
    public override string Link => typeof(T).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(attr => attr.Key == "RepositoryUrl")?.Value ?? "Unknown";
    public override void OnEngineInit()
    {
        OnLoad();
        var engineInitHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineInit");
        engineInitHook?.Invoke(this, []);
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
}