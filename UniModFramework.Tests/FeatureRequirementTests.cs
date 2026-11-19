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

    private class ToggledConfig : Config
    {
        public bool AllowLarge = true;
        public ConfigurationKey<int> Key;
        public ToggledConfig() => Key = new("Key", valueValidator: new FuncValueValidator<int>(val => AllowLarge || val < 10));
    }

    private static (bool started, List<(LogLevel level, string message)> log) Start(Type modType, Config? config = null)
    {
        var log = new List<(LogLevel, string)>();
        return (Backend.CanStart(modType, config, (level, message) => log.Add((level, message))), log);
    }

    [Fact]
    public void Missing_required_feature_stops_the_start_with_an_error()
    {
        var (started, log) = Start(typeof(RequiresPrePatching));
        Assert.False(started);
        var (level, message) = Assert.Single(log);
        Assert.Equal(LogLevel.Error, level);
        Assert.Contains(nameof(RequiresPrePatching), message);
    }

    [Fact]
    public void Missing_optional_feature_starts_with_a_message()
    {
        var (started, log) = Start(typeof(OptionallyUsesPrePatching));
        Assert.True(started);
        var (level, message) = Assert.Single(log);
        Assert.Equal(LogLevel.Info, level);
        Assert.Contains(nameof(OptionallyUsesPrePatching), message);
    }

    [Fact]
    public void Invalid_config_values_start_with_a_warning()
    {
        // Stands in for a value loaded from a config file: set while valid, then the validator rejects it
        var config = new ToggledConfig();
        config.Key.SetValue(20);
        config.AllowLarge = false;

        var (started, log) = Start(typeof(NoRequirements), config);

        Assert.True(started);
        var (level, message) = Assert.Single(log);
        Assert.Equal(LogLevel.Warn, level);
        Assert.StartsWith($"{nameof(NoRequirements)}: ", message);
        Assert.Equal(0, config.Key.GetValue());
    }

    [Fact]
    public void Failed_steps_are_logged_with_the_mod_name()
    {
        var log = new List<(LogLevel level, string message)>();
        Assert.False(Backend.Report(false, typeof(NoRequirements), "OnLoad", (level, message) => log.Add((level, message))));
        Assert.True(Backend.Report(true, typeof(NoRequirements), "OnReady", (level, message) => log.Add((level, message))));
        Assert.Equal((LogLevel.Error, $"{nameof(NoRequirements)}: OnLoad failed"), Assert.Single(log));
    }
}
