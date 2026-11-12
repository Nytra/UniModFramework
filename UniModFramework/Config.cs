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
    public object[] Tags {get; private set;}
    public event Action<T?>? OnChanged;
    private Predicate<T?>? _valueValidator;
    public ConfigurationKey(string id, string? description = null, T? defaultValue = default, Predicate<T?>? valueValidator = null, params object[] tags)
    {
        Id = id;
        DefaultValue = defaultValue;
        Description = description;
        Tags = tags;
        _valueValidator = valueValidator;
    }
    public void SetValue(T? val)
    {
        bool valid = false;
        if (_valueValidator is not null)
        {
            if (_valueValidator(val))
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