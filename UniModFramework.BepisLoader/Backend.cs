namespace UniModFramework;

internal static partial class Backend
{
    internal static partial bool HasFeature(Feature feature) => feature switch
    {
        Feature.PrePatching => true,
        _ => false
    };
}
