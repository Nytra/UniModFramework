using System.Reflection;

namespace UniModFramework;

public class MetadataAttribute : Attribute
{
    public string GUID;
    public string Name;
    public string Version;
    public string Author;
    public string Link;
    public MetadataAttribute(string GUID, string Name, string Version, string Author, string Link)
    {
        this.GUID = GUID;
        this.Name = Name;
        this.Version = Version;
        this.Author = Author;
        this.Link = Link;
    }
}

public class PrePatcherMetadataAttribute : Attribute
{
    public string GUID;
    public string Name;
    public string Version;
    public PrePatcherMetadataAttribute(string GUID, string Name, string Version)
    {
        this.GUID = GUID;
        this.Name = Name;
        this.Version = Version;
    }
}

public class TargetAssemblyAttribute : Attribute
{
    public AssemblyName TargetAssembly;

    public TargetAssemblyAttribute(string assemblyName)
    {
        TargetAssembly = AssemblyName.GetAssemblyName(assemblyName);
    }
}

public class TargetAllAssembliesAttribute : Attribute
{

    public TargetAllAssembliesAttribute()
    {
    }
}

public class TargetTypeAttribute : Attribute
{
    public AssemblyName TargetAssembly;
    public string TargetType;

    public TargetTypeAttribute(string assemblyName, string typeName)
    {
        TargetAssembly = AssemblyName.GetAssemblyName(assemblyName);
        TargetType = typeName;
    }
}