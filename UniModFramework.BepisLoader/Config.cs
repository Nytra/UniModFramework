using BepInEx.Configuration;

namespace UniModFramework;

public class Config
{
    
}

public class ConfigurationKey<T> : IConfigurationKey<T> where T : unmanaged
{
    public string Id => _configEntry!.Definition.Key;
    string IConfigurationKey.Id => Id;
    public T Value => _configEntry!.Value;
    T IConfigurationKey<T>.Value => Value;
    private ConfigEntry<T>? _configEntry;
    private string _id;
    private T _defaultValue;
    public ConfigurationKey(string id, T defaultValue)
    {
        _id = id;
        _defaultValue = defaultValue;
    }
    public void SetValue(T val)
    {
        _configEntry!.Value = val;
    }
    public T GetValue()
    {
        return _configEntry!.Value;
    }
    internal void Init(ConfigFile file)
    {
        _configEntry = file.Bind("General", _id, _defaultValue, "");
    }
    public static implicit operator T(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}

public class ConfigKeyAttribute : Attribute
{
    
}