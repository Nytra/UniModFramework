using Xunit;

namespace UniModFramework.Tests;

/// <summary>
/// The tests use the reference assembly's backend, which has no features.
/// </summary>
public class FeatureRequirementTests
{
    [FeatureRequirement(Feature.PrePatching)]
    private class RequiresPrePatching;

    [FeatureRequirement(Feature.PrePatching, RequirementType.Optional)]
    private class OptionallyUsesPrePatching;

    private class NoRequirements;

    [Fact]
    public void Missing_required_feature_is_an_error()
    {
        Assert.Contains(nameof(Feature.PrePatching), Backend.CheckFeatures(typeof(RequiresPrePatching)));
    }

    [Fact]
    public void Missing_optional_feature_is_fine()
    {
        Assert.Null(Backend.CheckFeatures(typeof(OptionallyUsesPrePatching)));
    }

    [Fact]
    public void No_requirements_is_fine()
    {
        Assert.Null(Backend.CheckFeatures(typeof(NoRequirements)));
    }

    [Fact]
    public void Missing_optional_feature_gets_a_message()
    {
        Assert.Contains(nameof(Feature.PrePatching), Assert.Single(Backend.MissingOptionalFeatures(typeof(OptionallyUsesPrePatching))));
    }

    [Fact]
    public void Required_features_get_no_optional_message()
    {
        Assert.Empty(Backend.MissingOptionalFeatures(typeof(RequiresPrePatching)));
        Assert.Empty(Backend.MissingOptionalFeatures(typeof(NoRequirements)));
    }
}
