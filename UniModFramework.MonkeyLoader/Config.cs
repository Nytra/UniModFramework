using System.Reflection;
using HarmonyLib;
using MonkeyLoader.Configuration;

namespace UniModFramework;

public class Config : ConfigSection
{
    public override string Id => "General";

    public override Version Version => new Version(1, 0, 0);
    protected override IEnumerable<IDefiningConfigKey> GetConfigKeys()
    {
        var set = new HashSet<IDefiningConfigKey>();
        foreach (var cfgKeyField in AccessTools.GetDeclaredFields(GetType()).Where(f => f.GetCustomAttribute<ConfigKeyAttribute>() is not null))
        {
            var cfgKey = cfgKeyField.GetValue(this);
            var initMethod = AccessTools.Method(cfgKey!.GetType(), "Init");
            initMethod.Invoke(cfgKey, []);
            var keyField = AccessTools.Field(cfgKey.GetType(), "_configKey");
            set.Add((IDefiningConfigKey)keyField.GetValue(cfgKey)!);
        }
        return set;
    }
}

public partial class ConfigurationKey<T> : IConfigurationKey<T> where T : unmanaged
{
    public string Id => _configKey!.Id;
    string IConfigurationKey.Id => Id;
    public T Value => _configKey!.GetValue();
    T IConfigurationKey<T>.Value => Value;
    private DefiningConfigKey<T>? _configKey;
    private string _id;
    private T _defaultValue;
    public ConfigurationKey(string id, T defaultValue)
    {
        _id = id;
        _defaultValue = defaultValue;
    }
    public void SetValue(T val)
    {
        _configKey!.SetValue(val);
    }
    public T GetValue()
    {
        return _configKey!.GetValue();
    }
    internal void Init()
    {
        _configKey = new(_id, computeDefault: () => _defaultValue);
    }
    public static implicit operator T(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}

public class ConfigKeyAttribute : Attribute
{
    
}