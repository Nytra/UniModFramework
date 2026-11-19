using System.Reflection;
using Xunit;

namespace UniModFramework.Tests;

public class VersionCheckTests
{
    // Stands in for a mod: the test assembly references xunit.core like a mod references UniModFramework
    private static readonly Assembly Mod = typeof(VersionCheckTests).Assembly;
    private static readonly Version Referenced = Mod.GetReferencedAssemblies().Single(reference => reference.Name == "xunit.core").Version!;

    [Fact]
    public void Older_framework_is_an_error()
    {
        var framework = new AssemblyName("xunit.core") { Version = new Version(0, 1) };
        Assert.Contains($"{Referenced} or newer", Backend.CheckVersion(Mod, framework));
    }

    [Fact]
    public void Same_or_newer_framework_is_fine()
    {
        Assert.Null(Backend.CheckVersion(Mod, new AssemblyName("xunit.core") { Version = Referenced }));
        Assert.Null(Backend.CheckVersion(Mod, new AssemblyName("xunit.core") { Version = new Version(Referenced.Major + 1, 0) }));
    }

    [Fact]
    public void Unreferenced_framework_is_fine()
    {
        Assert.Null(Backend.CheckVersion(Mod, new AssemblyName("NotReferenced") { Version = new Version(1, 0) }));
    }
}
