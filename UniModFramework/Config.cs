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
    public ConfigurationKey(string id, T defaultValue)
    {
        Id = id;
        Value = defaultValue;
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

// public class ConfigKeyAttribute : Attribute
// {
    
// }