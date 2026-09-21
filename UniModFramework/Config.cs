namespace UniModFramework;

public class Config
{
    
}

public class ConfigurationKey<T> : IConfigurationKey<T>
{
    public string Id {get; private set;}
    public T? Value {get; private set;}
    public T? DefaultValue {get; private set;}
    public string? Description {get; private set;}
    public bool InternalAccessOnly {get; private set;}
    public event Action<T?>? OnChanged;
    public ConfigurationKey(string id, string? description = null, T? defaultValue = default, bool? internalAccessOnly = null, Predicate<T?>? valueValidator = null)
    {
        Id = id;
        Value = defaultValue ?? default;
        DefaultValue = defaultValue;
        Description = description;
        InternalAccessOnly = internalAccessOnly ?? false;
    }
    public void SetValue(T? val)
    {
        Value = val;
        OnChanged?.Invoke(Value);
    }
    public T? GetValue()
    {
        return Value;
    }
    public static implicit operator T?(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}