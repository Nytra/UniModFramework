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
        TargetAssembly = ParseAssemblyName(assemblyName);
    }

    /// <summary>
    /// Accepts the assembly name with or without <c>.dll</c>, like BepInEx.
    /// </summary>
    internal static AssemblyName ParseAssemblyName(string assemblyName) =>
        new(assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? Path.GetFileNameWithoutExtension(assemblyName) : assemblyName);
}

[AttributeUsage(AttributeTargets.Method)]
public class TargetAllAssembliesAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TargetTypeAttribute : Attribute
{
    internal AssemblyName TargetAssembly;
    internal string TargetType;

    public TargetTypeAttribute(string assemblyName, string typeName)
    {
        TargetAssembly = TargetAssemblyAttribute.ParseAssemblyName(assemblyName);
        TargetType = typeName;
    }
}
