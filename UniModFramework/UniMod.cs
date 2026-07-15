using HarmonyLib;

namespace UniModFramework;

public abstract class UniMod<T> where T : UniMod<T>, new()
{
    protected abstract bool OnLoad(Harmony harmony);
    public static bool HasFeature(Feature feature)
    {
        return false;
    }
    protected void LogInfo(string msg)
    {
    }
}