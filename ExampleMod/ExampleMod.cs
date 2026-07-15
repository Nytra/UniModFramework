using Elements.Core;
using FrooxEngine;
using UniModFramework;

namespace ExampleMod;

[Metadata("Nytra.ExampleMod", "ExampleMod", "1.0.0", "Nytra", "https://github.com/Nytra/UniModFramework")]
[FeatureRequirement(Feature.PrePatching, RequirementType.Optional)]
public class ExampleMod : UniMod<ExampleMod>
{
    // called as early as possible, sometimes before the game is loaded
    protected override bool OnLoad()
    {
        // setup mod here
        Log("OnLoad");
        PatchAll();
        return true;
    }

    [Hook("OnEngineReady")]
    private void EngineHook()
    {
        // do stuff here
        UniLog.Log("hello!");
    }

    [Patch(typeof(Engine), "Initialize")]
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