using HarmonyLib;
//using MonkeyLoader.Components;
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
    public string[] Tags => _tags;
    public IValueValidator<T>? Validator
    {
        get
        {
            if (_configKey!.Components.TryGet<IConfigKeyValidator<T>>(out var comp))
                return (IValueValidator<T>)comp;
            else
                return null;
        }
    }
    public event Action<T?>? OnChanged;
    private DefiningConfigKey<T>? _configKey;
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
        _configKey!.SetValue(val!);
    }
    public T? GetValue()
    {
        return _configKey!.GetValue();
    }
    internal void Init()
    {
        _configKey = new(_id, _description, () => _defaultValue!, _tags.Contains("Hidden"));
        if (_valueValidator is not null)
            _configKey.Add((IConfigKeyValidator<T>)_valueValidator);
        _configKey.Changed += (sender, args) => OnChanged?.Invoke(_configKey.GetValue());
    }
    public static implicit operator T?(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}

public class FuncValueValidator<T> : IConfigKeyValidator<T>, IValueValidator<T>
{
    private Predicate<T?> _func;
    public FuncValueValidator(Predicate<T?> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        _func = func;
    }

    public void Initialize(IDefiningConfigKey<T> entity)
    {
        if (entity.HasValue && entity.TryGetValue(out var value) && !IsValid(value))
            throw new InvalidOperationException($"Value for key [{entity.Id}] did not pass validation!");
    }

    public bool IsValid(T? value)
    {
        return _func(value);
    }

    public bool Validate(T? val) => IsValid(val);
}

public class RangeValueValidator<T> : IConfigKeyValidator<T>, IValueValidator<T> where T : unmanaged, IComparable
{
    private T _min;
    private T _max;
    public RangeValueValidator(T min, T max)
    {
        _min = min;
        _max = max;
    }

    public void Initialize(IDefiningConfigKey<T> entity)
    {
        if (entity.HasValue && entity.TryGetValue(out var value) && !IsValid(value))
            throw new InvalidOperationException($"Value for key [{entity.Id}] did not pass validation!");
    }

    public bool IsValid(T value)
    {
        return value.CompareTo(_min) >= 0 && value.CompareTo(_max) <= 0;
    }

    public bool Validate(T val) => IsValid(val);
}

public class ListValueValidator<T> : IConfigKeyValidator<T>, IValueValidator<T> where T : IEquatable<T?>
{
    private T?[] _acceptableValues;
    public ListValueValidator(params T?[] values)
    {
        _acceptableValues = values;
    }

    public void Initialize(IDefiningConfigKey<T> entity)
    {
        if (entity.HasValue && entity.TryGetValue(out var value) && !IsValid(value))
            throw new InvalidOperationException($"Value for key [{entity.Id}] did not pass validation!");
    }

    public bool IsValid(T? value)
    {
        return _acceptableValues.Contains(value);
    }

    public bool Validate(T? val) => IsValid(val);
}