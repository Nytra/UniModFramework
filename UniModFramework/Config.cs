namespace UniModFramework;

// The reference assembly mods compile against. The loader backends store the value in the loader's config instead.
public partial class ConfigurationKey<T>
{
    public string Id {get;}
    public string? Description {get;}
    public T? Value {get; private set;}
    public T? DefaultValue {get;}
    public event Action<T?>? OnChanged;
    public ConfigurationKey(string id, string? description = default, T? defaultValue = default, IValueValidator<T>? valueValidator = default, params string[] tags)
    {
        Id = ConfigKeyRules.CheckDefinition(id, typeof(T));
        Description = description;
        Value = DefaultValue = defaultValue;
        Validator = valueValidator;
        Tags = tags;
    }
    private partial void StoreValue(T? val)
    {
        Value = val;
        OnChanged?.Invoke(Value);
    }
    public partial void ResetToDefault() => StoreValue(DefaultValue);
}
