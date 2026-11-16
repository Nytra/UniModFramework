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
    public string[] Tags => _configEntry!.Description.Tags.OfType<string>().ToArray();
    public IValueValidator<T>? Validator => (IValueValidator<T>?)_configEntry!.Description.AcceptableValues;
    public event Action<T?>? OnChanged;
    private ConfigEntry<T>? _configEntry;
    private string _id;
    private T? _defaultValue;
    private string? _description;
    private IValueValidator<T>? _valueValidator;
    private string[] _tags;
    public ConfigurationKey(string id, string? description = default, T? defaultValue = default, IValueValidator<T>? valueValidator = default, params string[] tags)
    {
        _id = id;
        _defaultValue = defaultValue ?? default;
        _description = description;
        _valueValidator = valueValidator;
        _tags = tags;
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
                                                                                       (AcceptableValueBase?)_valueValidator,
                                                                                       _tags)
        );
        _configEntry.SettingChanged += (sender, args) => OnChanged?.Invoke(_configEntry.Value);
    }
    public static implicit operator T?(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}

public class FuncValueValidator<T> : AcceptableValueBase, IValueValidator<T>
{
    private Predicate<T?> _func;
    private T? _defaultValue;
    public FuncValueValidator(Predicate<T?> func) : base(typeof(T))
    {
        ArgumentNullException.ThrowIfNull(func);
        _func = func;
    }
    internal void SetDefault(T? def)
    {
        _defaultValue = def;
    }
    public bool Validate(T? val)
    {
        return IsValid(val!);
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
        return _func((T?)value);
    }
    public override string ToDescriptionString()
    {
        return "Custom value validator";
    }
}

public class RangeValueValidator<T> : AcceptableValueRange<T>, IValueValidator<T> where T : unmanaged, IComparable
{
    public RangeValueValidator(T min, T max) : base(min, max)
    {
    }
    public bool Validate(T val)
    {
        return IsValid(val);
    }
}

public class ListValueValidator<T> : AcceptableValueList<T>, IValueValidator<T> where T : IEquatable<T>
{
    public ListValueValidator(params T?[] values) : base(values!)
    {
    }
    public bool Validate(T? val)
    {
        return IsValid(val);
    }
}