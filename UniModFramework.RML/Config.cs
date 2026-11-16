using ResoniteModLoader;

namespace UniModFramework;

public class Config
{
}

public class ConfigurationKey<T> : ModConfigurationKey<T>, IConfigurationKey<T>
{
    public string Id => Name;
    public T? DefaultValue
    {
        get
        {
            TryComputeDefaultTyped(out T? defaultValue);
            return defaultValue;
        }
    }
    public string[] Tags => _tags;
    public IValueValidator<T>? Validator {get; private set;}
    public new event Action<T?>? OnChanged;
    private string[] _tags;
    public ConfigurationKey(string id, string? description = default, T? defaultValue = default, IValueValidator<T>? valueValidator = default, params string[] tags) : base(id, description, () => defaultValue ?? default!, tags.Contains("Hidden"), valueValidator is null ? null : (val) => valueValidator.Validate(val))
    {
        base.OnChanged += (val) => OnChanged?.Invoke(Value);
        _tags = tags;
        Validator = valueValidator;
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

public class FuncValueValidator<T> : IValueValidator<T>
{
    private Predicate<T?> _func;
    public FuncValueValidator(Predicate<T?> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        _func = func;
    }
    public bool Validate(T? val)
    {
        return _func(val);
    }
}

public class RangeValueValidator<T> : IValueValidator<T> where T : unmanaged, IComparable
{
    private T _min;
    private T _max;
    public RangeValueValidator(T min, T max)
    {
        _min = min;
        _max = max;
    }
    public bool Validate(T val)
    {
        return val.CompareTo(_min) >= 0 && val.CompareTo(_max) <= 0;
    }
}

public class ListValueValidator<T> : IValueValidator<T> where T : IEquatable<T?>
{
    private T?[] _acceptableValues;
    public ListValueValidator(params T?[] values)
    {
        _acceptableValues = values;
    }
    public bool Validate(T? val)
    {
        return _acceptableValues.Contains(val);
    }
}