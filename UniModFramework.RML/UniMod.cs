using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> : ResoniteMod where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    public override string Name => typeof(T).GetCustomAttribute<MetadataAttribute>()!.Name;
    public override string Version => typeof(T).GetCustomAttribute<MetadataAttribute>()!.Version;
    public override string Author => typeof(T).GetCustomAttribute<MetadataAttribute>()!.Author;
    public override string Link => typeof(T).GetCustomAttribute<MetadataAttribute>()!.Link;
    public override void OnEngineInit()
    {
        var harmony = new Harmony(typeof(T).GetCustomAttribute<MetadataAttribute>()!.GUID);
        OnLoad(harmony);
        var engineInitHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineInit");
        engineInitHook?.Invoke(this, []);
        var engineReadyHook = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<HookAttribute>()?.HookName == "OnEngineReady");
        Engine.Current.OnReady += () => engineReadyHook?.Invoke(this, []);
    }
    public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
    {
        Config = new();
        foreach (var cfgKeyField in AccessTools.GetDeclaredFields(typeof(TConfig)).Where(f => f.GetCustomAttribute<ConfigKeyAttribute>() is not null))
        {
            var cfgKey = cfgKeyField.GetValue(Config);
            builder.Key((ModConfigurationKey)cfgKey!);
        }
    }
    public UniMod()
    {
        _infoLogger = (string str) => Msg(str);
        _featureChecker = (Feature feature) =>
        {
            switch (feature)
            {
                case Feature.PrePatching:
                    return false;
            }
            return false;
        };
    }
}