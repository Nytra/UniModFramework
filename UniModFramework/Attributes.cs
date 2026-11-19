using System.Reflection;

namespace UniModFramework;

// The values are internal, mods only use the constructors and each loader exposes them differently.

[AttributeUsage(AttributeTargets.Class)]
public class MetadataAttribute : Attribute
{
    internal string GUID;
    internal string Name;
    internal string Version;
    internal string Author;
    internal string Link;
    public MetadataAttribute(string GUID, string Name, string Version, string Author, string Link)
    {
        this.GUID = GUID;
        this.Name = Name;
        this.Version = Version;
        this.Author = Author;
        this.Link = Link;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PrePatcherMetadataAttribute : Attribute
{
    internal string GUID;
    internal string Name;
    internal string Version;
    public PrePatcherMetadataAttribute(string GUID, string Name, string Version)
    {
        this.GUID = GUID;
        this.Name = Name;
        this.Version = Version;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TargetAssemblyAttribute : Attribute
{
    internal AssemblyName TargetAssembly;

    public TargetAssemblyAttribute(string assemblyName)
    {
        if (assemblyName.EndsWith(".dll"))
            TargetAssembly = new(Path.GetFileNameWithoutExtension(assemblyName));
        else
            TargetAssembly = new(assemblyName);
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class TargetAllAssembliesAttribute : Attribute
{

    public TargetAllAssembliesAttribute()
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TargetTypeAttribute : Attribute
{
    internal AssemblyName TargetAssembly;
    internal string TargetType;

    public TargetTypeAttribute(string assemblyName, string typeName)
    {
        if (assemblyName.EndsWith(".dll"))
            TargetAssembly = new(Path.GetFileNameWithoutExtension(assemblyName));
        else
            TargetAssembly = new(assemblyName);
        TargetType = typeName;
    }
}
