using ResoniteModLoader;

namespace UniModFramework;

public class Config
{
}

public class ConfigurationKey<T> : ModConfigurationKey<T>, IConfigurationKey<T>// where T : unmanaged
{
    public string Id => Name;
    string IConfigurationKey.Id => Id;
    //public T Value;
    T? IConfigurationKey<T>.Value => Value;
    public new event Action<T?>? OnChanged;
    public ConfigurationKey(string id, string? description, T? defaultValue) : base(id, description, computeDefault: () => defaultValue ?? default!)
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

// public class ConfigKeyAttribute : Attribute
// {
    
// }