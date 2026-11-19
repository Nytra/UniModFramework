using ResoniteModLoader;

namespace UniModFramework;

public partial class ConfigurationKey<T> : ModConfigurationKey<T>
{
    public string Id => Name;
    public T? DefaultValue
    {
        get
        {
            TryComputeDefaultTyped(out T? defaultValue);
            return defaultValue;
        }
    }
    public new event Action<T?>? OnChanged;
    public ConfigurationKey(string id, string? description = default, T? defaultValue = default, IValueValidator<T>? valueValidator = default, params string[] tags) : base(ConfigKeyRules.CheckDefinition(id, typeof(T)), description, () => defaultValue ?? default!, tags.Contains(ConfigTags.Hidden), valueValidator is null ? null : (val) => valueValidator.Validate(val))
    {
        base.OnChanged += (val) => OnChanged?.Invoke(Value);
        Tags = tags;
        Validator = valueValidator;
    }
    // RML doesn't validate values set in code or loaded from the config file
    private partial void StoreValue(T? val) => Value = val;
    // RML resets keys by removing their value through ModConfiguration.Unset, which keys can't reach.
    // Setting the default gives the same value, but the config file stores it.
    private partial void ResetValue() => Value = DefaultValue;
}
