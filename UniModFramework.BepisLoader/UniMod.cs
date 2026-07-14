using BepInEx.NET.Common;

namespace UniModFramework;

public abstract class UniMod : BasePlugin
{
    public abstract void Init();
    public override void Load() => Init();
    public static bool HasFeature(Feature feature)
    {
        switch (feature)
        {
            case Feature.PrePatching:
                return true;
        }
        return false;
    }

    
}