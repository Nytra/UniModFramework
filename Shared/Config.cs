namespace UniModFramework;

public interface IConfigurationKey
{
    string Id {get;}
    string? Description {get;}
    object? BoxedValue {get;}
    object? BoxedDefaultValue {get;}
    object[] Tags {get;}
    Type ValueType {get;}
}

public interface IConfigurationKey<T> : IConfigurationKey
{
    T? Value {get;}
    T? DefaultValue {get;}
    object? IConfigurationKey.BoxedValue => Value;
    object? IConfigurationKey.BoxedDefaultValue => DefaultValue;
    Type IConfigurationKey.ValueType => typeof(T);
    void SetValue(T? val);
    T? GetValue();
    event Action<T?>? OnChanged;
}

// public interface IValueValidator
// {
//     bool Validate(object? val);
// }

// public interface IValueValidator<T> : IValueValidator
// {
//     bool Validate(T? val);
//     bool IValueValidator.Validate(object? val) => Validate((T?)val);
// }