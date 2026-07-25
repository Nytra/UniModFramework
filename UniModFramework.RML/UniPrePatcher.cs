using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;

namespace UniModFramework;

public abstract partial class UniPrePatcher<T, TConfig> where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    public UniPrePatcher()
    {
        _infoLogger = (string str) => ResoniteMod.Msg(str);
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