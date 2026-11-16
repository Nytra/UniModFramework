namespace UniModFramework;

public interface IConfigurationKey
{
    string Id {get;}
    string? Description {get;}
    object? Value {get;}
    object? DefaultValue {get;}
    string[] Tags {get;}
    IValueValidator? Validator {get;}
    Type ValueType {get;}
}

public interface IConfigurationKey<T> : IConfigurationKey
{
    new T? Value {get;}
    object? IConfigurationKey.Value => Value;
    new T? DefaultValue {get;}
    object? IConfigurationKey.DefaultValue => DefaultValue;
    Type IConfigurationKey.ValueType => typeof(T);
    new IValueValidator<T>? Validator {get;}
    IValueValidator? IConfigurationKey.Validator => Validator;
    void SetValue(T? val);
    T? GetValue();
    event Action<T?>? OnChanged;
}

public interface IValueValidator
{
    bool Validate(object? val);
}

public interface IValueValidator<T> : IValueValidator
{
    bool Validate(T? val);
    bool IValueValidator.Validate(object? val) => Validate((T?)val);
}