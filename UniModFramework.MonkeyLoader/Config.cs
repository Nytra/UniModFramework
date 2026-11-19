using MonkeyLoader.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniModFramework;

public partial class Config : ConfigSection
{
    private readonly List<string> _rejectedValues = [];

    public override string Id => "General";

    public override Version Version => new Version(1, 0, 0);

    protected override IEnumerable<IDefiningConfigKey> GetConfigKeys()
    {
        return ConfigurationKeys.OfType<IDefiningKeyFactory>().Select(key => key.CreateDefiningKey()).ToArray();
    }

    // MonkeyLoader stops loading at the first value that fails validation,
    // so invalid values are dropped first and those keys keep their default.
    protected override void OnLoad(JObject source, JsonSerializer jsonSerializer)
    {
        foreach (var key in ConfigurationKeys)
        {
            if (key.Validator is null || source[key.Id] is not JToken token)
                continue;

            object? value;
            try
            {
                value = token.ToObject(key.ValueType, jsonSerializer);
            }
            catch (Exception)
            {
                continue; // MonkeyLoader reports values it can't read
            }

            if (key.Validator.Validate(value))
                continue;

            source.Remove(key.Id);
            _rejectedValues.Add($"Config key [{key.Id}] had invalid value {token}, using the default");
        }

        base.OnLoad(source, jsonSerializer);
    }

    partial void AddLoaderWarnings(List<string> warnings)
    {
        warnings.AddRange(_rejectedValues);
        _rejectedValues.Clear();
    }
}

internal interface IDefiningKeyFactory
{
    IDefiningConfigKey CreateDefiningKey();
}

public partial class ConfigurationKey<T> : IDefiningKeyFactory
{
    public string Id => _id;
    public T? Value => _configKey is null ? _defaultValue : _configKey.GetValue();
    public T? DefaultValue => _defaultValue;
    public string? Description => _description;
    public event Action<T?>? OnChanged;
    private DefiningConfigKey<T>? _configKey;
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
        if (_configKey is null)
            throw new InvalidOperationException($"Config key [{_id}] can't be set before the config is loaded");
        _configKey.SetValue(val!);
    }
    IDefiningConfigKey IDefiningKeyFactory.CreateDefiningKey()
    {
        _configKey = new(_id, _description, () => _defaultValue!, Tags.Contains(ConfigTags.Hidden));
        if (Validator is not null)
            _configKey.Add(new ValidatorComponent<T>(Validator));
        _configKey.Changed += (sender, args) => OnChanged?.Invoke(_configKey.GetValue());
        return _configKey;
    }
}

/// <summary>
/// Lets MonkeyLoader use any <see cref="IValueValidator{T}"/>.
/// </summary>
internal class ValidatorComponent<T>(IValueValidator<T> validator) : IConfigKeyValidator<T>
{
    // Invalid values from the config file are handled by Config.OnLoad
    public void Initialize(IDefiningConfigKey<T> entity)
    {
    }

    public bool IsValid(T? value) => validator.Validate(value);
}
