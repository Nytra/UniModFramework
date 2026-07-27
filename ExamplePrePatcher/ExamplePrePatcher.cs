using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using UniModFramework;

namespace ExamplePrePatcher;

public class MyPrePatcherConfig : Config
{
    public ConfigurationKey<bool> MyToggle = new("MyToggle", "This is my toggle", false);
}

[PrePatcherMetadata("Nytra.ExamplePrePatcher", "ExamplePrePatcher", "1.0.0")]
public class ExamplePrePatcher : UniPrePatcher<ExamplePrePatcher, MyPrePatcherConfig>
{
    protected override bool OnInitialize()
    {
        LogInfo("Initialize");
        return true;
    }

    protected override bool OnFinalize()
    {
        LogInfo("Finalize");
        return true;
    }

    // [TargetAssembly("Elements.Core.dll")]
    // public bool PatchAsm(ref AssemblyDefinition assembly)
    // {
    //     LogInfo($"Patching assembly: {assembly.Name.Name}");
    //     return true;
    // }

    [TargetType("Elements.Core.dll", "Elements.Core.CollectionsExtensions")]
    public bool PatchType(TypeDefinition type)
    {
        LogInfo($"Patching type: {type.Name}");
        return true;
    }

    // [TargetAllAssemblies]
    // public bool PatchAllAsms(ref AssemblyDefinition assembly)
    // {
    //     LogInfo($"Patching all assembly: {assembly.Name.Name}");
    //     return true;
    // }
}