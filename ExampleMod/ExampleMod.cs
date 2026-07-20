using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using UniModFramework;

namespace ExampleMod;

public class MyConfig : Config
{
    [ConfigKey]
    public ConfigurationKey<float> MyValue = new("MyValue", 4.5f);
}

[Metadata("Nytra.ExampleMod", "ExampleMod", "1.0.0", "Nytra", "https://github.com/Nytra/UniModFramework")]
public class ExampleMod : UniMod<ExampleMod, MyConfig>
{
    // called as early as possible, sometimes before the game is loaded
    protected override bool OnLoad(Harmony harmony)
    {
        // setup mod here
        LogInfo("OnLoad");
        LogInfo($"Key val: {Config!.MyValue}");
        harmony.PatchAll();
        return true;
    }

    [Hook("OnEngineReady")]
    private void EngineHook()
    {
        // do stuff here
        UniLog.Log("hello!");
    }

    [HarmonyPatch(typeof(Engine), "Initialize")]
    class MyPatch
    {
        static void Postfix()
        {
            UniLog.Log("In postfix");
            if (HasFeature(Feature.PrePatching))
            {
                // run patch code
                UniLog.Log("Has pre patching!");
            }
        }
    }
}