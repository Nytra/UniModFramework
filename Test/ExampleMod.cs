using Elements.Core;
using FrooxEngine;
using UniModFramework;

namespace ExampleMod;

[FeatureRequirement(Feature.PrePatching, RequirementType.Optional)]
public class ExampleMod : UniMod<ExampleMod>
{

    // typically called as early as possible
    protected override bool OnLoad()
    {
        // setup mod here
        Log("OnLoad");
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
            if (HasFeature(Feature.PrePatching))
            {
                // run patch code
            }
        }
    }
}