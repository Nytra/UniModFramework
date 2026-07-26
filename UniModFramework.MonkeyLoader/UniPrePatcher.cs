using System.Reflection;
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Patching;

namespace UniModFramework;

public abstract partial class UniPrePatcher<T, TConfig> : ConfiguredEarlyMonkey<T, TConfig> where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    // protected override IEnumerable<IFeaturePatch> GetFeaturePatches()
    // {
    //     return [FeaturePatches.]
    // }
    protected override bool Prepare()
    {
        try
        {
            Config = ConfiguredEarlyMonkey<T, TConfig>.Config.LoadSection<TConfig>();
        }
        catch (ConfigLoadException)
        {
            // Empty config, this is probably fine
        }
        return OnInitialize();
    }
    protected override bool Prepare(IEnumerable<PatchJob> patchJobs)
    {
        LogInfo($"In prepare with patch jobs");
        foreach (var patchJob in patchJobs)
        {
            LogInfo($"Prepare patch job: {patchJob.Target.Assembly.Name}:{string.Join(".", patchJob.Target.Types)}");
        }
        return true;
    }
    protected override IEnumerable<PrePatchTarget> GetPrePatchTargets()
    {
        var set = new HashSet<PrePatchTarget>();

        if (AccessTools.GetDeclaredMethods(typeof(T)).Any(m => m.GetCustomAttribute<TargetAllAssembliesAttribute>() is not null))
        {
            LogInfo($"Targets all assemblies");
            return PrePatchTarget.AllAvailable;
        }

        foreach (var targetType in AccessTools.GetDeclaredMethods(typeof(T)).Where(m => m.GetCustomAttribute<TargetTypeAttribute>() is not null))
        {
            var attr = targetType.GetCustomAttribute<TargetTypeAttribute>();
            LogInfo($"attr: {attr!.TargetAssembly.Name}:{attr.TargetType}");
            var ppt = new PrePatchTarget(new MonkeyLoader.AssemblyName(attr!.TargetAssembly.Name!), [attr.TargetType]);
            LogInfo($"Adding prepatch target type: {ppt.Assembly.Name}:{string.Join(".", ppt.Types)}");
            set.Add(ppt);
            //yield return ppt;
        }

        foreach (var targetAsm in AccessTools.GetDeclaredMethods(typeof(T)).Where(m => m.GetCustomAttribute<TargetAssemblyAttribute>() is not null))
        {
            var attr = targetAsm.GetCustomAttribute<TargetAssemblyAttribute>();
            LogInfo($"attr: {attr!.TargetAssembly.Name}");
            var ppt = new PrePatchTarget(new MonkeyLoader.AssemblyName(attr!.TargetAssembly.Name!));
            LogInfo($"Adding prepatch target assembly: {ppt.Assembly.Name}");
            set.Add(ppt);
            //yield return ppt;
        }

        return set;
    }
    protected override bool Patch(PatchJob patchJob)
    {
        LogInfo($"Patch job: {patchJob.Target.Assembly.Name}.{string.Join(".", patchJob.Target.Types)}");
        if (patchJob.Target.Types.Count() == 0)
        {
            LogInfo($"target assembly");
            var method = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<TargetAssemblyAttribute>() is TargetAssemblyAttribute attr && attr.TargetAssembly.Name == patchJob.Target.Assembly.Name);
            method?.Invoke(this, [patchJob.Assembly]);
        }
        else
        {
            LogInfo($"target type");
            foreach (var type in patchJob.Target.Types)
            {
                LogInfo($"type: {type}");
                var method = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<TargetTypeAttribute>() is TargetTypeAttribute attr && attr.TargetAssembly.Name == patchJob.Target.Assembly.Name && attr.TargetType == type);
                method?.Invoke(this, [patchJob[type]]);
            }
        }
        return true;
    }
    protected override bool Validate(IEnumerable<PatchJob> patchJobs)
    {
        return OnFinalize();
        //return base.Validate(patchJobs);
    }
    public UniPrePatcher()
    {
        _infoLogger = (string str) => Logger.Info(() => str);
        _featureChecker = (Feature feature) =>
        {
            switch (feature)
            {
                case Feature.PrePatching:
                    return true;
            }
            return false;
        };
    }
}