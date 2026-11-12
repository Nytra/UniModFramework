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
    public object[] Tags => _tags;
    public new event Action<T?>? OnChanged;
    private object[] _tags;
    public ConfigurationKey(string id, string? description = null, T? defaultValue = default, Predicate<T?>? valueValidator = null, params object[] tags) : base(id, description, () => defaultValue ?? default!, tags.Contains("Hidden"), valueValidator)
    {
        base.OnChanged += (val) => OnChanged?.Invoke(Value);
        _tags = tags;
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