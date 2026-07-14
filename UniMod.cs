namespace UniModFramework;

public abstract class UniMod : UniModBase
{
    public abstract void Init();
    public static bool HasFeature(Feature feature)
    {
        return true;
    }
}