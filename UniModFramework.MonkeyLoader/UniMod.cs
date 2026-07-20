using System.Reflection;
using HarmonyLib;
using MonkeyLoader.Resonite;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> : ConfiguredResoniteMonkey<T, TConfig> where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    protected override bool OnLoaded()
    {
        Config = ConfiguredResoniteMonkey<T, TConfig>.Config.LoadSection<TConfig>();
        return OnLoad(Harmony);
    }
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
        _infoLogger = (string str) => Logger.Info(() => str);
        _featureChecker = (Feature feature) =>
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