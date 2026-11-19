using System.Reflection;

namespace UniModFramework;

public enum Feature
{
    PrePatching
}

public enum RequirementType
{
    Required,
    Optional
}

/// <summary>
/// Declares a feature the mod or pre-patcher uses. It isn't loaded if a required feature is missing.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FeatureRequirementAttribute : Attribute
{
    public Feature Feature;
    public RequirementType RequirementType;
    public FeatureRequirementAttribute(Feature feature, RequirementType type = RequirementType.Required)
    {
        Feature = feature;
        RequirementType = type;
    }
}

internal enum LogLevel
{
    Debug,
    Info,
    Warn,
    Error
}

/// <summary>
/// What each loader backend has to provide, besides the loader-specific parts of the public classes.
/// </summary>
internal static partial class Backend
{
    internal static partial bool HasFeature(Feature feature);

    /// <summary>
    /// Mods built against a newer UniModFramework may use members the installed one doesn't have.
    /// </summary>
    /// <returns>An error if the mod needs a newer UniModFramework than <paramref name="framework"/>.</returns>
    internal static string? CheckVersion(Assembly modAssembly, AssemblyName framework)
    {
        var required = modAssembly.GetReferencedAssemblies().FirstOrDefault(reference => reference.Name == framework.Name)?.Version;
        if (required is null || framework.Version is null || required <= framework.Version)
            return null;
        return $"{modAssembly.GetName().Name} needs {framework.Name} {required} or newer, but {framework.Version} is installed";
    }

    internal static string? CheckVersion(Type modType) => CheckVersion(modType.Assembly, typeof(Backend).Assembly.GetName());

    /// <returns>An error if the mod requires features this loader doesn't have.</returns>
    internal static string? CheckFeatures(Type modType)
    {
        var missing = modType.GetCustomAttributes<FeatureRequirementAttribute>()
                             .Where(requirement => requirement.RequirementType == RequirementType.Required && !HasFeature(requirement.Feature))
                             .Select(requirement => requirement.Feature)
                             .ToArray();
        if (missing.Length == 0)
            return null;
        return $"{modType.Name} requires {string.Join(", ", missing)}, which this mod loader doesn't have";
    }
}
