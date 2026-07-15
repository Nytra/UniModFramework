using BepInExResoniteShim;

namespace UniModFramework;

public class MetadataAttribute : ResonitePlugin
{
    public MetadataAttribute(string GUID, string Name, string Version, string Author, string Link) : base(GUID, Name, Version, Author, Link)
    {
    }
}