using FrooxEngine;
using UniModFramework;

[FeatureRequirement(Feature.PrePatching, RequirementType.Optional)]
public class ExampleMod : UniMod
{
    public override void Init()
    {
        // setup mod here
    }

    [UniPatch(typeof(Engine), "Initialize")]
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