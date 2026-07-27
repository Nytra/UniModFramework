using System.Reflection;
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> : ConfiguredResoniteMonkey<T, TConfig> where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    protected override bool OnLoaded()
    {
        // Maybe update the _infoLogger here so it logs to the game log?
        try
        {
            Config = ConfiguredResoniteMonkey<T, TConfig>.Config.LoadSection<TConfig>();
        }
        catch (ConfigLoadException)
        {
            // Empty config, this is probably fine
        }
        return OnLoad(Harmony);
    }
    protected override bool OnEngineReady() => OnReady();
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