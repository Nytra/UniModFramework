using FrooxEngine;
using UniModFramework;

[FeatureRequirement(Feature.PrePatching, RequirementType.Optional)]
public class ExampleMod : UniMod<ExampleMod>
{
    private bool _hooked;

    // typically called as early as possible
    protected override bool OnLoad()
    {
        // setup mod here
        return true;
    }

    [Hook("OnEngineInit")] // Used by RML and MonkeyLoader
    [Hook("OnEngineReady")] // Used by BepisLoader and MonkeyLoader
    private void EngineHook()
    {
        if (!_hooked)
        {
            // because this method could be called by either OnEngineInit or OnEngineReady, we use a flag to make sure it only runs the code once
            _hooked = true;
        }
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