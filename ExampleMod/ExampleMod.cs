using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using UniModFramework;

namespace ExampleMod;

public class MyConfig : Config
{
    public ConfigurationKey<float> MyValue = new("MyValue", "This is my value", 4.5f, valueValidator: new RangeValueValidator<float>(3f, 9f));
    public ConfigurationKey<string> MyString = new("MyString", "This is my string", "Hello");
    public ConfigurationKey<int> MyInternalKey = new("MyInternalKey", "This is my internal key", tags: ["Hidden"]);
}

[Metadata("Nytra.ExampleMod", "ExampleMod", "1.0.0", "Nytra", "https://github.com/Nytra/UniModFramework")]
public class ExampleMod : UniMod<ExampleMod, MyConfig>
{
    protected override bool OnLoad(Harmony harmony)
    {
        // setup mod here
        LogInfo($"OnLoad. Key val: {Config!.MyValue}");
        harmony.PatchAll();
        Config.MyValue.OnChanged += (val) => LogInfo($"Value changed: {val}");
        return true;
    }

    protected override bool OnReady()
    {
        UniLog.Log("hello!");
        Config!.MyValue.SetValue(6.7f);
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