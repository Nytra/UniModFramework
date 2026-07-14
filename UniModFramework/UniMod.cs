namespace UniModFramework;

public abstract class UniMod
{
    public abstract void Init();
    public static bool HasFeature(Feature feature)
    {
        return false;
    }
}