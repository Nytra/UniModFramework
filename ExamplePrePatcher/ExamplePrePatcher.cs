using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using UniModFramework;

namespace ExamplePrePatcher;

public class MyPrePatcherConfig : Config
{
    
}

[PrePatcherMetadata("Nytra.ExampleMod", "ExampleMod", "1.0.0")]
public class ExamplePrePatcher : UniPrePatcher<ExamplePrePatcher, MyPrePatcherConfig>
{
    protected override bool OnInitialize()
    {
        return true;
    }

    protected override bool OnFinalize()
    {
        return true;
    }

    [TargetAssembly("Elements.Core.dll")]
    public bool PatchFrooxEngine(ref AssemblyDefinition assembly)
    {
        LogInfo($"Patching: {assembly.Name.Name}");
        return true;
    }

    [TargetType("Elements.Core.dll", "Elements.Core.CollectionsExtensions")]
    public bool PatchFrooxEngine(TypeDefinition type)
    {
        LogInfo($"Patching: {type.Name}");
        return true;
    }
}