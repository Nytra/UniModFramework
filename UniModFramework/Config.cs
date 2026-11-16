namespace UniModFramework;

public class Config
{
    
}

public class ConfigurationKey<T> : IConfigurationKey<T>
{
    public string Id {get; private set;}
    public T? Value {get; private set;}
    public T? DefaultValue {get; private set;}
    public string? Description {get; private set;}
    public string[] Tags {get; private set;}
    public IValueValidator<T>? Validator {get; private set;}
    public event Action<T?>? OnChanged;
    public ConfigurationKey(string id, string? description = default, T? defaultValue = default, IValueValidator<T>? valueValidator = default, params string[] tags)
    {
        Id = id;
        DefaultValue = defaultValue;
        Description = description;
        Tags = tags;
        Validator = valueValidator;
    }
    public void SetValue(T? val)
    {
        bool valid = false;
        if (Validator is not null)
        {
            if (Validator.Validate(val))
            {
                valid = true;
            }
            else
            {
                throw new InvalidOperationException($"Value not valid: {val?.ToString() ?? "NULL"}");
            }
        }
        else
        {
            valid = true;
        }
        if (valid)
        {
            Value = val;
            OnChanged?.Invoke(Value);
        }
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