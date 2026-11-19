using System.Reflection;
using HarmonyLib;
using MonkeyLoader.Patching;
using Mono.Cecil;

namespace UniModFramework;

public abstract partial class UniPrePatcher<T, TConfig> : ConfiguredEarlyMonkey<T, TConfig> where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    protected override bool Prepare()
    {
        Config = Backend.LoadSection<TConfig>(ConfiguredEarlyMonkey<T, TConfig>.Config, WriteLog);
        return RunInitialize();
    }
    // The base skips all patching if any target assembly is missing, BepInEx patches the ones that exist
    protected override bool Prepare(IEnumerable<PatchJob> patchJobs) => true;
    protected override IEnumerable<PrePatchTarget> GetPrePatchTargets()
    {
        var methods = AccessTools.GetDeclaredMethods(typeof(T));
        var targetTypes = methods.SelectMany(m => m.GetCustomAttributes<TargetTypeAttribute>()).ToArray();
        var assemblyNames = methods.SelectMany(m => m.GetCustomAttributes<TargetAssemblyAttribute>())
                                   .Select(attr => attr.TargetAssembly.Name)
                                   .Concat(targetTypes.Select(t => t.TargetAssembly.Name));

        // MonkeyLoader doesn't implement PrePatchTarget.AllAvailable (it gives no patch jobs), so target every game assembly like MonkeyLoader loads them
        if (methods.Any(m => m.GetCustomAttribute<TargetAllAssembliesAttribute>() is not null))
            assemblyNames = assemblyNames.Concat(Directory.EnumerateFiles(MonkeyLoader.MonkeyLoader.GameAssemblyPath, "*.dll").Select(Path.GetFileNameWithoutExtension));

        // Only one target per assembly, MonkeyLoader waits forever for an assembly that is already part of another patch job
        return assemblyNames.OfType<string>()
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Select(assemblyName => new PrePatchTarget(new MonkeyLoader.AssemblyName(assemblyName), targetTypes.Where(t => IsAssembly(t.TargetAssembly, assemblyName)).Select(t => t.TargetType)))
                            .ToArray();
    }
    protected override bool Patch(PatchJob patchJob)
    {
        var assemblyName = patchJob.Target.Assembly.Name;
        foreach (var method in AccessTools.GetDeclaredMethods(typeof(T)))
        {
            if (method.GetCustomAttribute<TargetAllAssembliesAttribute>() is not null
                || method.GetCustomAttributes<TargetAssemblyAttribute>().Any(attr => IsAssembly(attr.TargetAssembly, assemblyName)))
                RunPatchMethod(method, patchJob, patchJob.Assembly);

            foreach (var attr in method.GetCustomAttributes<TargetTypeAttribute>())
            {
                if (IsAssembly(attr.TargetAssembly, assemblyName) && patchJob.TryGetTypeDefinition(attr.TargetType, out var type))
                    RunPatchMethod(method, patchJob, type);
            }
        }
        return true;
    }
    /// <summary>
    /// MonkeyLoader compares assembly names ignoring case.
    /// </summary>
    private static bool IsAssembly(System.Reflection.AssemblyName target, string assemblyName) => string.Equals(target.Name, assemblyName, StringComparison.OrdinalIgnoreCase);
    /// <summary>
    /// Matches BepInEx: an optional second parameter gets the target file name,
    /// and returning void or true reports changes (including replacing an assembly taken by ref).
    /// </summary>
    private void RunPatchMethod(MethodInfo method, PatchJob patchJob, object target)
    {
        var args = new object?[method.GetParameters().Length];
        args[0] = target;
        if (args.Length > 1)
            args[1] = $"{patchJob.Target.Assembly.Name}.dll";

        if (method.Invoke(method.IsStatic ? null : this, args) is false)
            return;

        patchJob.Changes = true;
        if (args[0] is AssemblyDefinition assembly)
            patchJob.Assembly = assembly;
    }
    protected override bool Validate(IEnumerable<PatchJob> patchJobs) => RunFinalize();
    private static partial void WriteLog(LogLevel level, string message) => Backend.Log(Logger, level, message);
}
