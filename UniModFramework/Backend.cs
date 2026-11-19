namespace UniModFramework;

// The reference assembly mods compile against has no loader, the loader backends implement these.

internal static partial class Backend
{
    internal static partial bool HasFeature(Feature feature) => false;
}

public abstract partial class UniMod<T, TConfig>
{
    private static partial void WriteLog(LogLevel level, string message)
    {
    }
}

public abstract partial class UniPrePatcher<T, TConfig>
{
    private static partial void WriteLog(LogLevel level, string message)
    {
    }
}
