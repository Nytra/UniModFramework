using MonkeyLoader.Resonite;

namespace UniModFramework;

public abstract partial class UniMod<T, TConfig> : ConfiguredResoniteMonkey<T, TConfig> where T : UniMod<T, TConfig>, new() where TConfig : Config, new()
{
    protected override bool OnLoaded()
    {
        Config = Backend.LoadSection<TConfig>(ConfiguredResoniteMonkey<T, TConfig>.Config, WriteLog);
        return RunLoad(Harmony);
    }
    protected override bool OnEngineReady() => RunReady();
    private static partial void WriteLog(LogLevel level, string message) => Backend.Log(Logger, level, message);
}
