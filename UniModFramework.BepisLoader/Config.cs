using BepInEx.Configuration;

namespace UniModFramework;

internal interface IBindableKey
{
    void Bind(ConfigFile file);
}

public partial class Config
{
    internal void Bind(ConfigFile file)
    {
        foreach (var key in ConfigurationKeys.OfType<IBindableKey>())
            key.Bind(file);
    }
}

public partial class ConfigurationKey<T> : IBindableKey
{
    public string Id {get;}
    public T? Value => _configEntry is null ? DefaultValue : _configEntry.Value;
    public T? DefaultValue {get;}
    public string? Description {get;}
    public event Action<T?>? OnChanged;
    private ConfigEntry<T>? _configEntry;
    public ConfigurationKey(string id, string? description = default, T? defaultValue = default, IValueValidator<T>? valueValidator = default, params string[] tags)
    {
        Id = ConfigKeyRules.CheckDefinition(id, typeof(T));
        DefaultValue = defaultValue;
        Description = description;
        Validator = valueValidator;
        Tags = tags;
    }
    private partial void StoreValue(T? val)
    {
        if (_configEntry is null)
            throw new InvalidOperationException($"Config key [{Id}] can't be set before the config is loaded");
        _configEntry.Value = val!;
    }
    // BepInEx has no reset, setting the default is how it's done. Before the config is loaded the value is already the default.
    private partial void ResetValue()
    {
        if (_configEntry is not null)
            _configEntry.Value = DefaultValue!;
    }
    void IBindableKey.Bind(ConfigFile file)
    {
        // ConfigDescription doesn't allow a null description
        _configEntry = file.Bind("General", Id, DefaultValue!, new ConfigDescription(Description ?? "", CreateAcceptableValues(), Tags));
        _configEntry.SettingChanged += (sender, args) => OnChanged?.Invoke(_configEntry.Value);
    }
    // BepisModSettings only shows sliders and dropdowns for BepInEx's own range and list types.
    // BepInEx clamps out of range values and replaces values missing from the list with the first one.
    private AcceptableValueBase? CreateAcceptableValues() => Validator switch
    {
        null => null,
        IRangeValueValidator range => (AcceptableValueBase)Activator.CreateInstance(typeof(AcceptableValueRange<>).MakeGenericType(typeof(T)), range.Min, range.Max)!,
        IListValueValidator list => (AcceptableValueBase)Activator.CreateInstance(typeof(AcceptableValueList<>).MakeGenericType(typeof(T)), [list.Values])!,
        _ => new ValidatorAdapter<T>(Validator, DefaultValue),
    };
}

/// <summary>
/// Lets BepInEx use any <see cref="IValueValidator{T}"/>. Invalid values are replaced with the default.
/// </summary>
internal class ValidatorAdapter<T>(IValueValidator<T> validator, T? defaultValue) : AcceptableValueBase(typeof(T))
{
    public override object Clamp(object value) => IsValid(value) ? value : defaultValue!;
    public override bool IsValid(object value) => ((IValueValidator)validator).Validate(value);
    public override string ToDescriptionString() => "# Acceptable values: checked by the mod";
}
