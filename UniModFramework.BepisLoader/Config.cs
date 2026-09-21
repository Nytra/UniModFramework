using BepInEx.Configuration;

namespace UniModFramework;

public class Config
{
    
}

public class ConfigurationKey<T> : IConfigurationKey<T>
{
    public string Id => _configEntry!.Definition.Key;
    public T? Value => _configEntry!.Value;
    public T? DefaultValue => (T?)_configEntry!.DefaultValue;
    public string? Description => _configEntry!.Description.Description;
    public bool InternalAccessOnly => _configEntry!.Description.Tags?.Contains("Hidden") ?? false;
    public event Action<T?>? OnChanged;
    private ConfigEntry<T>? _configEntry;
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
        _configEntry!.Value = val!;
    }
    public T? GetValue()
    {
        return _configEntry!.Value;
    }
    internal void Init(ConfigFile file)
    {
        _configEntry = file.Bind("General", _id, _defaultValue!, new ConfigDescription(_description, 
                                                                                       _valueValidator is not null ? new ValueValidator<T>(_valueValidator, _defaultValue) : null,
                                                                                       _internalAccessOnly ? ["Hidden"] : [])
        );
        _configEntry.SettingChanged += (sender, args) => OnChanged?.Invoke(_configEntry.Value);
    }
    public static implicit operator T?(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}

internal class ValueValidator<T> : AcceptableValueBase
{
    private Predicate<T?> _valueValidator;
    private T? _defaultValue;
    public ValueValidator(Predicate<T?> validatorFunc, T? defaultValue) : base(typeof(T))
    {
        _valueValidator = validatorFunc;
        _defaultValue = defaultValue;
    }
    public override object Clamp(object value)
    {
        if (IsValid(value))
            return value;
        else
            return _defaultValue!;
    }

    public override bool IsValid(object value)
    {
        return _valueValidator((T?)value);
    }

    public override string ToDescriptionString()
    {
        return "Custom value validator";
    }
}