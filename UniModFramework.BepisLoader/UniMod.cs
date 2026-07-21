using System.Reflection;
using BepInEx.NET.Common;
using BepisResoniteWrapper;
using HarmonyLib;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> : BasePlugin where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    public override void Load()
    {
        Config = new();
        foreach (var cfgKeyField in AccessTools.GetDeclaredFields(typeof(TConfig)).Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ConfigurationKey<>)))
        {
            var cfgKey = cfgKeyField.GetValue(Config);
            var initMethod = AccessTools.Method(cfgKey!.GetType(), "Init");
            initMethod.Invoke(cfgKey, [base.Config]);
        }
        OnLoad(HarmonyInstance);
        ResoniteHooks.OnEngineReady += () => OnReady();
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