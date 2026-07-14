namespace UniModFramework;

public abstract partial class UniModBase
{
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