using ResoniteModLoader;

namespace UniModFramework;

public class Config
{
}

public class ConfigurationKey<T> : ModConfigurationKey<T>, IConfigurationKey<T>
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
    public ConfigurationKey(string id, string? description = null, T? defaultValue = default, bool? internalAccessOnly = null, Predicate<T?>? valueValidator = null) : base(id, description, () => defaultValue ?? default!, internalAccessOnly ?? false, valueValidator)
    {
        base.OnChanged += (val) => OnChanged?.Invoke(Value);
    }
    public void SetValue(T? val)
    {
        Value = val;
    }
    public T? GetValue()
    {
        return Value;
    }
    public static implicit operator T?(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}