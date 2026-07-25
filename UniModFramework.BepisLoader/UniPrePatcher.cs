using BepInEx.Preloader.Core.Patching;
using HarmonyLib;

namespace UniModFramework;

public abstract partial class UniPrePatcher<T, TConfig> : BasePatcher where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    public override void Initialize()
    {
        Config = new();
        foreach (var cfgKeyField in AccessTools.GetDeclaredFields(typeof(TConfig)).Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ConfigurationKey<>)))
        {
            var cfgKey = cfgKeyField.GetValue(Config);
            var initMethod = AccessTools.Method(cfgKey!.GetType(), "Init");
            initMethod.Invoke(cfgKey, [base.Config]);
        }
        OnInitialize();
    }
    public override void Finalizer()
    {
        OnFinalize();
    }
    public UniPrePatcher()
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