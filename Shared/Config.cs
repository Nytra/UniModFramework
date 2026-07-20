namespace UniModFramework;

public interface IConfigurationKey
{
    string Id {get;}
    object UntypedValue {get;}
}

public interface IConfigurationKey<T> : IConfigurationKey where T : unmanaged
{
    T Value {get;}
    object IConfigurationKey.UntypedValue => Value;
    void SetValue(T val);
    T GetValue();
}