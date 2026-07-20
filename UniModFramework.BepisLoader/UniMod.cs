using System.Reflection;
using BepInEx.NET.Common;
using BepisResoniteWrapper;
using HarmonyLib;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> : BasePlugin where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    public override void Load()
    {
        foreach (var cfgKeyField in AccessTools.GetDeclaredFields(typeof(TConfig)).Where(f => f.GetCustomAttribute<ConfigKeyAttribute>() is not null))
        {
            var cfgKey = cfgKeyField.GetValue(Config);
            var initMethod = AccessTools.Method(cfgKey!.GetType(), "Init");
            initMethod.Invoke(cfgKey, [base.Config]);
        }
        OnLoad(HarmonyInstance);
        var engineReadyHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineReady");
        ResoniteHooks.OnEngineReady += () => engineReadyHook?.Invoke(this, []);
    }
    public UniMod()
    {
        _infoLogger = (string str) => Log.LogInfo(str);
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