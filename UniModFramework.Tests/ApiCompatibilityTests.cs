using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace UniModFramework.Tests;

/// <summary>
/// Mods compile against the reference UniModFramework.dll and run against a backend's UniModFramework.dll,
/// so every type and member a mod can see in the reference must also exist in each backend (declared or inherited),
/// otherwise mods fail at runtime with MissingMethodException/TypeLoadException.
/// The backends must be built first (building the solution does this).
/// </summary>
public class ApiCompatibilityTests
{
    public static TheoryData<string> Backends => new()
    {
        "UniModFramework.RML",
        "UniModFramework.MonkeyLoader",
        "UniModFramework.BepisLoader",
    };

    [Theory]
    [MemberData(nameof(Backends))]
    public void Backend_exposes_reference_api(string backend)
    {
        using var referenceContext = CreateContext(BuildOutput("UniModFramework"), out var reference);
        using var backendContext = CreateContext(BuildOutput(backend), out var implementation);

        var missing = new List<string>();
        foreach (var referenceType in reference.GetTypes().Where(IsVisible))
        {
            var backendType = implementation.GetType(referenceType.FullName!);
            if (backendType is null || !IsVisible(backendType))
            {
                missing.Add($"type {referenceType.FullName}");
                continue;
            }

            var available = AvailableSignatures(backendType);
            foreach (var member in referenceType.GetMembers(AllDeclared))
            {
                if (IsVisible(member) && Signature(member) is string signature && !available.Contains(signature))
                    missing.Add($"{referenceType.FullName}: {signature}");
            }
        }

        Assert.True(missing.Count == 0, $"{backend} is missing:\n{string.Join("\n", missing)}");
    }

    private const BindingFlags AllDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static HashSet<string> AvailableSignatures(Type type)
    {
        var signatures = new HashSet<string>(type.GetConstructors(AllDeclared).Where(IsVisible).Select(Signature).OfType<string>());
        // The runtime resolves member references through base classes, so inherited members count
        for (var current = type; current is not null; current = current.BaseType)
            signatures.UnionWith(current.GetMembers(AllDeclared).Where(m => m is not ConstructorInfo && IsVisible(m)).Select(Signature).OfType<string>());
        return signatures;
    }

    private static bool IsVisible(Type type) => type.IsPublic || type.IsNestedPublic || type.IsNestedFamily || type.IsNestedFamORAssem;

    private static bool IsVisible(MemberInfo member) => member switch
    {
        MethodBase method => method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly,
        FieldInfo field => field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly,
        _ => false, // properties and events are covered by their accessor methods
    };

    private static string? Signature(MemberInfo member) => member switch
    {
        MethodInfo method => $"{(method.IsStatic ? "static " : "")}{method.ReturnType} {method.Name}({Parameters(method)})",
        ConstructorInfo ctor when !ctor.IsStatic => $".ctor({Parameters(ctor)})",
        FieldInfo field => $"{(field.IsStatic ? "static " : "")}{field.FieldType} {field.Name}",
        _ => null,
    };

    private static string Parameters(MethodBase method) => string.Join(", ", method.GetParameters().Select(p => p.ParameterType.ToString()));

    private static string Setting(string key) =>
        typeof(ApiCompatibilityTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == key).Value ?? "";

    private static string BuildOutput(string project)
    {
        var path = Path.Combine(Setting("RepoRoot"), project, "bin", Setting("Configuration"), "net10.0", "UniModFramework.dll");
        Assert.True(File.Exists(path), $"{path} not found, build {project} first");
        return path;
    }

    private static MetadataLoadContext CreateContext(string assemblyPath, out Assembly assembly)
    {
        var context = new MetadataLoadContext(new SearchResolver(assemblyPath), "System.Private.CoreLib");
        assembly = context.LoadFromAssemblyPath(assemblyPath);
        return context;
    }

    /// <summary>
    /// Finds referenced assemblies in the runtime, the game folder, then the NuGet package cache.
    /// </summary>
    private sealed class SearchResolver(string targetPath) : MetadataAssemblyResolver
    {
        private static readonly Lazy<ILookup<string, string>> NuGetIndex = new(() =>
        {
            var root = Setting("NuGetPackageRoot");
            if (!Directory.Exists(root))
                return Array.Empty<string>().ToLookup(p => p);
            return Directory.EnumerateFiles(root, "*.dll", SearchOption.AllDirectories)
                .OrderByDescending(p => p, StringComparer.Ordinal)
                .ToLookup(p => Path.GetFileNameWithoutExtension(p), StringComparer.OrdinalIgnoreCase);
        });

        public override Assembly? Resolve(MetadataLoadContext context, AssemblyName assemblyName)
        {
            var name = assemblyName.Name!;
            if (name == "UniModFramework")
                return context.LoadFromAssemblyPath(targetPath);

            var gamePath = Setting("GamePath");
            string[] directories = [RuntimeEnvironment.GetRuntimeDirectory(), Path.GetDirectoryName(targetPath)!,
                                    gamePath, Path.Combine(gamePath, "Libraries"), Path.Combine(gamePath, "rml_libs")];
            var path = directories.Where(d => d != "").Select(d => Path.Combine(d, name + ".dll")).FirstOrDefault(File.Exists)
                       ?? NuGetIndex.Value[name].FirstOrDefault();
            return path is null ? null : context.LoadFromAssemblyPath(path);
        }
    }
}
