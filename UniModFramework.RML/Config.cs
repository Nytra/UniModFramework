using ResoniteModLoader;

namespace UniModFramework;

public class Config
{
}

public partial class ConfigurationKey<T> : ModConfigurationKey<T>, IConfigurationKey<T> where T : unmanaged
{
    public string Id => Name;
    string IConfigurationKey.Id => Id;
    //public T Value;
    T IConfigurationKey<T>.Value => Value;
    public ConfigurationKey(string id, T defaultValue) : base(id, computeDefault: () => defaultValue)
    {
    }
    public void SetValue(T val)
    {
        Value = val;
    }
    public T GetValue()
    {
        return Value;
    }
    public static implicit operator T(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}

public class ConfigKeyAttribute : Attribute
{
    
}