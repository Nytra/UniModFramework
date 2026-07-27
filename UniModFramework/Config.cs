namespace UniModFramework;

public class Config
{
    
}

public class ConfigurationKey<T> : IConfigurationKey<T>// where T : unmanaged
{
    public string Id;
    string IConfigurationKey.Id => Id;
    public T? Value;
    T? IConfigurationKey<T>.Value => Value;
    public string? Description;
    public event Action<T?>? OnChanged;
    public ConfigurationKey(string id, string? description, T? defaultValue)
    {
        Id = id;
        Value = defaultValue ?? default;
        Description = description;
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

// public class ConfigKeyAttribute : Attribute
// {
    
// }