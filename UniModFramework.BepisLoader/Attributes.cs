using BepInEx.Preloader.Core.Patching;
using BepInExResoniteShim;

namespace UniModFramework;

public class MetadataAttribute : ResonitePlugin
{
    public MetadataAttribute(string GUID, string Name, string Version, string Author, string Link) : base(GUID, Name, Version, Author, Link)
    {
    }
}

public class PrePatcherMetadataAttribute : PatcherPluginInfoAttribute
{
    public PrePatcherMetadataAttribute(string GUID, string Name, string Version) : base(GUID, Name, Version)
    {
    }
}

public class TargetAssemblyAttribute : BepInEx.Preloader.Core.Patching.TargetAssemblyAttribute
{
    public TargetAssemblyAttribute(string assemblyName) : base(assemblyName)
    {
    }
}

public class TargetTypeAttribute : BepInEx.Preloader.Core.Patching.TargetTypeAttribute
{
    public TargetTypeAttribute(string assemblyName, string typeName) : base(assemblyName, typeName)
    {
    }
}