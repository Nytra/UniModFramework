using ResoniteModLoader;

namespace UniModFramework;

public abstract class UniMod : ResoniteMod
{
    public abstract void Init();
    public override void OnEngineInit() => Init();
    public static bool HasFeature(Feature feature)
    {
        switch (feature)
        {
            case Feature.PrePatching:
                return false;
        }
        return false;
    }
}