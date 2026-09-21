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
        foreach (var cfgKeyField in AccessTools.GetDeclaredFields(GetType()).Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ConfigurationKey<>)))
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

public class ConfigurationKey<T> : IConfigurationKey<T>
{
    public string Id => _configKey!.Id;
    public T? Value => _configKey!.GetValue();
    public T? DefaultValue
    {
        get
        {
            _configKey!.TryComputeDefault(out T? defaultValue);
            return defaultValue;
        }
    }
    public string? Description => _configKey!.Description;
    public bool InternalAccessOnly => _configKey!.InternalAccessOnly;
    public event Action<T?>? OnChanged;
    private DefiningConfigKey<T>? _configKey;
    private string _id;
    private T? _defaultValue;
    private string? _description;
    private Predicate<T?>? _valueValidator;
    private bool _internalAccessOnly;
    public ConfigurationKey(string id, string? description = null, T? defaultValue = default, bool? internalAccessOnly = null, Predicate<T?>? valueValidator = null)
    {
        _id = id;
        _defaultValue = defaultValue ?? default;
        _description = description;
        _valueValidator = valueValidator;
        _internalAccessOnly = internalAccessOnly ?? false;
    }
    public void SetValue(T? val)
    {
        _configKey!.SetValue(val!);
    }
    public T? GetValue()
    {
        return _configKey!.GetValue();
    }
    internal void Init()
    {
        _configKey = new(_id, _description, () => _defaultValue!, _internalAccessOnly, _valueValidator);
        _configKey.Changed += (sender, args) => OnChanged?.Invoke(_configKey.GetValue());
    }
    public static implicit operator T?(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}