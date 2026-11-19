using System.Reflection;

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
    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <exception cref="ArgumentException">The value didn't pass the validator.</exception>
    void SetValue(T? val);
    /// <summary>
    /// Sets the value if it passes the validator.
    /// </summary>
    /// <returns>Whether the value was set.</returns>
    bool TrySetValue(T? val);
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
    bool IValueValidator.Validate(object? val) => val switch
    {
        T typed => Validate(typed),
        null when default(T) is null => Validate(default),
        _ => false // null for a value type, or the wrong type
    };
}

/// <summary>
/// Tags with a meaning for config keys.
/// </summary>
public static class ConfigTags
{
    /// <summary>
    /// Hides the key from config UIs.
    /// </summary>
    public const string Hidden = "Hidden";
}

public partial class Config
{
    private IConfigurationKey[]? _configurationKeys;

    /// <summary>
    /// The config keys declared as fields of this config, including in base classes. Base class keys come first.
    /// </summary>
    public IEnumerable<IConfigurationKey> ConfigurationKeys => _configurationKeys ??= FindConfigurationKeys();

    private IConfigurationKey[] FindConfigurationKeys()
    {
        var types = new Stack<Type>();
        for (var type = GetType(); type != typeof(Config); type = type.BaseType!)
            types.Push(type);

        return types.SelectMany(type => type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    .Select(field => field.GetValue(this))
                    .OfType<IConfigurationKey>()
                    .ToArray();
    }

    /// <summary>
    /// Resets keys whose loaded value is invalid (e.g. from an edited config file) to their default.
    /// </summary>
    /// <returns>A warning for each key that had an invalid value.</returns>
    internal List<string> ResetInvalidValues()
    {
        var warnings = new List<string>();
        AddLoaderWarnings(warnings);
        foreach (var key in ConfigurationKeys.OfType<IResettableKey>())
        {
            if (key.Validator?.Validate(key.Value) != false)
                continue;
            warnings.Add($"Config key [{key.Id}] had invalid value {key.Value ?? "NULL"}, using the default");
            key.ResetToDefault();
        }
        return warnings;
    }

    /// <summary>
    /// Lets a loader report invalid values it rejected itself while loading.
    /// </summary>
    partial void AddLoaderWarnings(List<string> warnings);
}

internal interface IResettableKey : IConfigurationKey
{
    void ResetToDefault();
}

/// <summary>
/// The parts of a config key that are the same for every loader. Each loader stores the value in its own config.
/// </summary>
public partial class ConfigurationKey<T> : IConfigurationKey<T>, IResettableKey
{
    public string[] Tags {get;}
    public IValueValidator<T>? Validator {get;}
    public T? GetValue() => Value;
    public void SetValue(T? val)
    {
        if (!TrySetValue(val))
            throw new ArgumentException($"Value not valid for config key [{Id}]: {val?.ToString() ?? "NULL"}", nameof(val));
    }
    public bool TrySetValue(T? val)
    {
        if (Validator?.Validate(val) == false)
            return false;
        StoreValue(val);
        return true;
    }
    void IResettableKey.ResetToDefault() => StoreValue(DefaultValue);
    /// <summary>
    /// Stores an already validated value in the loader's config.
    /// </summary>
    private partial void StoreValue(T? val);
    public static implicit operator T?(ConfigurationKey<T> cfg) => cfg.GetValue();
    public override string ToString() => $"{GetValue()}";
}

public class FuncValueValidator<T> : IValueValidator<T>
{
    private readonly Predicate<T?> _func;
    public FuncValueValidator(Predicate<T?> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        _func = func;
    }
    public bool Validate(T? val) => _func(val);
}

/// <summary>
/// Lets loaders show range validators in their own way, e.g. as sliders.
/// </summary>
internal interface IRangeValueValidator
{
    object Min {get;}
    object Max {get;}
}

public class RangeValueValidator<T> : IValueValidator<T>, IRangeValueValidator where T : unmanaged, IComparable
{
    private readonly T _min;
    private readonly T _max;
    public RangeValueValidator(T min, T max)
    {
        _min = min;
        _max = max;
    }
    object IRangeValueValidator.Min => _min;
    object IRangeValueValidator.Max => _max;
    public bool Validate(T val) => val.CompareTo(_min) >= 0 && val.CompareTo(_max) <= 0;
}

/// <summary>
/// Lets loaders show list validators in their own way, e.g. as dropdowns.
/// </summary>
internal interface IListValueValidator
{
    Array Values {get;}
}

public class ListValueValidator<T> : IValueValidator<T>, IListValueValidator where T : IEquatable<T?>
{
    private readonly T?[] _acceptableValues;
    public ListValueValidator(params T?[] values)
    {
        _acceptableValues = values;
    }
    Array IListValueValidator.Values => _acceptableValues;
    public bool Validate(T? val) => _acceptableValues.Contains(val);
}
