using FrooxEngine;
using UniModFramework;

[FeatureRequirement(Feature.PrePatching, FeatureRequirementType.Optional)]
public class ExampleMod : UniModFramework.UniMod
{
    public override void Init()
    {
        // setup mod here
    }

    [Patch(typeof(Engine), "Initialize")]
    class MyPatch
    {
        void Postfix()
        {
            if (HasFeature(Feature.PrePatching))
            {
                // run patch code
            }
        }
    }
}