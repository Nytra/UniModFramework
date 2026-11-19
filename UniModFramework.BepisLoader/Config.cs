using BepInEx.Configuration;

namespace UniModFramework;

internal interface IBindableKey
{
    void Bind(ConfigFile file);
}

public partial class ConfigurationKey<T> : IBindableKey
{
    public string Id => _id;
    public T? Value => _configEntry is null ? _defaultValue : _configEntry.Value;
    public T? DefaultValue => _defaultValue;
    public string? Description => _description;
    public event Action<T?>? OnChanged;
    private ConfigEntry<T>? _configEntry;
    private readonly string _id;
    private readonly T? _defaultValue;
    private readonly string? _description;
    public ConfigurationKey(string id, string? description = default, T? defaultValue = default, IValueValidator<T>? valueValidator = default, params string[] tags)
    {
        _id = id;
        _defaultValue = defaultValue;
        _description = description;
        Validator = valueValidator;
        Tags = tags;
    }
    private partial void StoreValue(T? val)
    {
        if (_configEntry is null)
            throw new InvalidOperationException($"Config key [{_id}] can't be set before the config is loaded");
        _configEntry.Value = val!;
    }
    void IBindableKey.Bind(ConfigFile file)
    {
        _configEntry = file.Bind("General", _id, _defaultValue!, new ConfigDescription(_description, CreateAcceptableValues(), Tags));
        _configEntry.SettingChanged += (sender, args) => OnChanged?.Invoke(_configEntry.Value);
    }
    // BepisModSettings only shows sliders and dropdowns for BepInEx's own range and list types.
    // BepInEx clamps out of range values and replaces values missing from the list with the first one.
    private AcceptableValueBase? CreateAcceptableValues() => Validator switch
    {
        null => null,
        IRangeValueValidator range => (AcceptableValueBase)Activator.CreateInstance(typeof(AcceptableValueRange<>).MakeGenericType(typeof(T)), range.Min, range.Max)!,
        IListValueValidator list => (AcceptableValueBase)Activator.CreateInstance(typeof(AcceptableValueList<>).MakeGenericType(typeof(T)), [list.Values])!,
        _ => new ValidatorAdapter<T>(Validator, _defaultValue),
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
