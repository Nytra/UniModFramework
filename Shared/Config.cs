namespace UniModFramework;

public interface IConfigurationKey
{
    string Id {get;}
    string? Description {get;}
    object? BoxedValue {get;}
    object? BoxedDefaultValue {get;}
    bool InternalAccessOnly {get;}
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
}