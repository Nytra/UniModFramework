using System.Reflection;

namespace UniModFramework;

public enum Feature
{
    PrePatching
}

public enum RequirementType
{
    /// <summary>
    /// The mod isn't loaded without the feature, and an error is logged.
    /// </summary>
    Required,
    /// <summary>
    /// The mod is loaded without the feature, and a message is logged saying some of its features are off.
    /// The mod checks <c>HasFeature</c> to work without it.
    /// </summary>
    Optional
}

/// <summary>
/// Declares a feature the mod or pre-patcher uses.
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

    /// <returns>A message for each optional feature the mod uses that this loader doesn't have.</returns>
    internal static IEnumerable<string> MissingOptionalFeatures(Type modType)
    {
        return modType.GetCustomAttributes<FeatureRequirementAttribute>()
                      .Where(requirement => requirement.RequirementType == RequirementType.Optional && !HasFeature(requirement.Feature))
                      .Select(requirement => $"{modType.Name} can use {requirement.Feature}, which this mod loader doesn't have, so some of its features are off");
    }

    /// <summary>
    /// The checks before a mod or pre-patcher starts, which also log the warnings for invalid config values.
    /// </summary>
    /// <returns>Whether it can start.</returns>
    internal static bool CanStart(Type modType, Config? config, Action<LogLevel, string> log)
    {
        if ((CheckVersion(modType) ?? CheckFeatures(modType)) is string error)
        {
            log(LogLevel.Error, error);
            return false;
        }

        foreach (var message in MissingOptionalFeatures(modType))
            log(LogLevel.Info, message);

        // Messages the framework logs itself name the mod, since RML only labels log lines with the mod when the mod's own code logs them
        foreach (var warning in config?.ResetInvalidValues() ?? [])
            log(LogLevel.Warn, $"{modType.Name}: {warning}");

        return true;
    }

    /// <summary>
    /// Logs an error if a step of the mod or pre-patcher failed.
    /// </summary>
    internal static bool Report(bool success, Type modType, string step, Action<LogLevel, string> log)
    {
        if (!success)
            log(LogLevel.Error, $"{modType.Name}: {step} failed");
        return success;
    }
}
