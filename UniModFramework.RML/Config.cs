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
    public ConfigurationKey(string id, string? description = default, T? defaultValue = default, IValueValidator<T>? valueValidator = default, params string[] tags) : base(id, description, () => defaultValue ?? default!, tags.Contains(ConfigTags.Hidden), valueValidator is null ? null : (val) => valueValidator.Validate(val))
    {
        base.OnChanged += (val) => OnChanged?.Invoke(Value);
        Tags = tags;
        Validator = valueValidator;
    }
    // RML doesn't validate values set in code or loaded from the config file
    private partial void StoreValue(T? val) => Value = val;
}
