namespace UniModFramework;

public abstract class UniMod<T> where T : UniMod<T>, new()
{
    protected abstract bool OnLoad();
    public static bool HasFeature(Feature feature)
    {
        return false;
    }
    protected void Log(string msg)
    {
    }
    protected void PatchAll()
    {
    }
}