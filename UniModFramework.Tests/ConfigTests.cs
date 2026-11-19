using Xunit;

namespace UniModFramework.Tests;

/// <summary>
/// Tests the config code shared by every loader, using the reference assembly's in-memory key storage.
/// </summary>
public class ConfigTests
{
    private class BaseConfig : Config
    {
        public ConfigurationKey<int> BaseKey = new("BaseKey");
    }

    private class DerivedConfig : BaseConfig
    {
        private bool _allowLarge = true;
        public ConfigurationKey<string> PublicKey = new("PublicKey", defaultValue: "a");
        private readonly ConfigurationKey<bool> _privateKey = new("PrivateKey");
        public ConfigurationKey<int> PropertyKey {get;}
        public string NotAKey = "";

        public DerivedConfig()
        {
            PropertyKey = new("PropertyKey", defaultValue: 1, valueValidator: new FuncValueValidator<int>(val => _allowLarge || val < 10));
        }

        public void DisallowLarge() => _allowLarge = false;
    }

    [Fact]
    public void Finds_keys_in_fields_properties_and_base_classes()
    {
        var ids = new DerivedConfig().ConfigurationKeys.Select(key => key.Id);
        Assert.Equal(["BaseKey", "PublicKey", "PrivateKey", "PropertyKey"], ids);
    }

    [Fact]
    public void Set_value_rejects_invalid_values_and_keeps_the_old_value()
    {
        var key = new ConfigurationKey<int>("Key", defaultValue: 1, valueValidator: new RangeValueValidator<int>(0, 10));
        key.SetValue(5);
        Assert.Throws<ArgumentException>(() => key.SetValue(11));
        Assert.Equal(5, key.GetValue());
    }

    [Fact]
    public void Try_set_value_reports_invalid_values()
    {
        var key = new ConfigurationKey<int>("Key", defaultValue: 1, valueValidator: new RangeValueValidator<int>(0, 10));
        var changes = new List<int>();
        key.OnChanged += changes.Add;

        Assert.True(key.TrySetValue(5));
        Assert.False(key.TrySetValue(11));
        Assert.Equal(5, key.GetValue());
        Assert.Equal([5], changes);
    }

    [Fact]
    public void Setting_the_same_value_does_not_fire_on_changed()
    {
        var key = new ConfigurationKey<int>("Key", defaultValue: 1);
        var changes = new List<int>();
        key.OnChanged += changes.Add;

        key.SetValue(1);
        key.SetValue(2);
        key.SetValue(2);

        Assert.Equal([2], changes);
    }

    [Theory]
    [InlineData("Has=Equals")]
    [InlineData("Has[Bracket]")]
    [InlineData("Has\"Quote")]
    [InlineData("Has\\Backslash")]
    [InlineData(" Padded ")]
    public void Ids_that_BepInEx_rejects_are_rejected_everywhere(string id)
    {
        Assert.Throws<ArgumentException>(() => new ConfigurationKey<int>(id));
    }

    private enum SomeEnum { A, B }

    [Fact]
    public void Types_BepInEx_supports_are_allowed()
    {
        _ = new ConfigurationKey<string>("String");
        _ = new ConfigurationKey<decimal>("Decimal");
        _ = new ConfigurationKey<SomeEnum>("Enum");
    }

    [Fact]
    public void Types_BepInEx_does_not_support_are_rejected_everywhere()
    {
        Assert.Throws<ArgumentException>(() => new ConfigurationKey<List<string>>("List"));
        Assert.Throws<ArgumentException>(() => new ConfigurationKey<int?>("Nullable"));
    }

    [Fact]
    public void Reset_to_default_sets_the_default()
    {
        IConfigurationKey key = new ConfigurationKey<int>("Key", defaultValue: 1);
        ((ConfigurationKey<int>)key).SetValue(5);
        key.ResetToDefault();
        Assert.Equal(1, key.Value);
    }

    [Fact]
    public void Reset_invalid_values_resets_to_default_with_a_warning()
    {
        var config = new DerivedConfig();
        config.PropertyKey.SetValue(20);
        config.DisallowLarge();

        var warnings = config.ResetInvalidValues();

        Assert.Equal(1, config.PropertyKey.GetValue());
        Assert.Contains("PropertyKey", Assert.Single(warnings));
        Assert.Empty(config.ResetInvalidValues());
    }
}
