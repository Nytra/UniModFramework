using Xunit;

namespace UniModFramework.Tests;

public class AttributeTests
{
    [Theory]
    [InlineData("Elements.Core")]
    [InlineData("Elements.Core.dll")]
    [InlineData("Elements.Core.DLL")]
    public void Target_assembly_names_work_with_or_without_dll(string name)
    {
        Assert.Equal("Elements.Core", new TargetAssemblyAttribute(name).TargetAssembly.Name);
        Assert.Equal("Elements.Core", new TargetTypeAttribute(name, "Elements.Core.Type").TargetAssembly.Name);
    }
}
