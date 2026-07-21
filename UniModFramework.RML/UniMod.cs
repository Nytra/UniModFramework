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
        Engine.Current.OnReady += () => OnReady();
    }
    public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
    {
        Config = new();
        foreach (var cfgKeyField in AccessTools.GetDeclaredFields(typeof(TConfig)).Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ConfigurationKey<>)))
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