using System.Reflection;
using HarmonyLib;
using MonkeyLoader.Patching;
using NuGet.Packaging;

namespace UniModFramework;

public abstract partial class UniPrePatcher<T, TConfig> : ConfiguredEarlyMonkey<T, TConfig> where T : UniPrePatcher<T, TConfig>, new() where TConfig : Config, new()
{
    protected override bool Prepare()
    {
        Config = ConfiguredEarlyMonkey<T, TConfig>.Config.LoadSection<TConfig>();
        return OnInitialize();
    }
    protected override IEnumerable<PrePatchTarget> GetPrePatchTargets()
    {
        var set = new HashSet<PrePatchTarget>();

        foreach (var targetType in AccessTools.GetDeclaredMethods(typeof(T)).Where(m => m.GetCustomAttribute<TargetTypeAttribute>() is not null))
        {
            var attr = targetType.GetCustomAttribute<TargetTypeAttribute>();
            set.Add(new PrePatchTarget(new MonkeyLoader.AssemblyName(attr!.TargetAssembly.Name!), attr.TargetType));
        }

        foreach (var targetAsm in AccessTools.GetDeclaredMethods(typeof(T)).Where(m => m.GetCustomAttribute<TargetAssemblyAttribute>() is not null))
        {
            var attr = targetAsm.GetCustomAttribute<TargetAssemblyAttribute>();
            set.Add(new PrePatchTarget(new MonkeyLoader.AssemblyName(attr!.TargetAssembly.Name!)));
        }

        return set;
    }
    protected override bool Patch(PatchJob patchJob)
    {
        if (patchJob.Target.Types.Count() == 0)
        {
            var method = AccessTools.GetDeclaredMethods(typeof(T)).FirstOrDefault(m => m.GetCustomAttribute<TargetAssemblyAttribute>() is TargetAssemblyAttribute attr && attr.TargetAssembly.Name == patchJob.Target.Assembly.Name);
            method?.Invoke(this, [patchJob.Assembly]);
        }
        else
        {
            foreach (var type in patchJob.Target.Types)
            {
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