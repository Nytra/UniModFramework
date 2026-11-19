using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> : ResoniteMod where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    private static MetadataAttribute Metadata => typeof(T).GetCustomAttribute<MetadataAttribute>()!;
    public override string Name => Metadata.Name;
    public override string Version => Metadata.Version;
    public override string Author => Metadata.Author;
    public override string Link => Metadata.Link;
    public override void OnEngineInit()
    {
        if (RunLoad(new Harmony(Metadata.GUID)))
            Engine.Current.OnReady += () => RunReady();
    }
    public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
    {
        Config = new();
        foreach (var key in Config.ConfigurationKeys.OfType<ModConfigurationKey>())
            builder.Key(key);
    }
    private static partial void WriteLog(LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Debug: Debug(message); break;
            case LogLevel.Info: Msg(message); break;
            case LogLevel.Warn: Warn(message); break;
            case LogLevel.Error: Error(message); break;
        }
    }
}
