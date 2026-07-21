using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using UniModFramework;

namespace ExampleMod;

public class MyConfig : Config
{
    public ConfigurationKey<float> MyValue = new("MyValue", 4.5f);
    public ConfigurationKey<string> MyString = new("MyString", "Hello");
}

[Metadata("Nytra.ExampleMod", "ExampleMod", "1.0.0", "Nytra", "https://github.com/Nytra/UniModFramework")]
public class ExampleMod : UniMod<ExampleMod, MyConfig>
{
    protected override bool OnLoad(Harmony harmony)
    {
        // setup mod here
        LogInfo($"OnLoad. Key val: {Config!.MyValue}");
        harmony.PatchAll();
        return true;
    }

    protected override bool OnReady()
    {
        UniLog.Log("hello!");
        return true;
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