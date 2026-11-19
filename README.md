# UniModFramework

Write a Resonite mod once and run it on ResoniteModLoader, MonkeyLoader or BepisLoader.

> Still in development, expect breaking changes.

Mods compile against the `Nytra.UniModFramework` package. Players install the UniModFramework build for their loader.

## Writing a mod

```csharp
public class MyConfig : Config
{
    public ConfigurationKey<float> Speed = new("Speed", "How fast", 4.5f, new RangeValueValidator<float>(1f, 10f));
}

[Metadata("You.MyMod", "MyMod", "1.0.0", "You", "https://github.com/you/MyMod")]
public class MyMod : UniMod<MyMod, MyConfig>
{
    protected override bool OnLoad(Harmony harmony)
    {
        harmony.PatchAll();
        Config.Speed.OnChanged += speed => LogInfo($"Speed is now {speed}");
        return true;
    }
}
```

- `OnLoad` runs when the mod loads, maybe before the game has. `OnReady` runs once the game is ready. Returning `false` logs an error.
- `LogDebug`, `LogInfo`, `LogWarn` and `LogError` are static, so Harmony patch classes can use them.
- Use the Harmony that comes with the package. Don't reference a newer Harmony or HarmonyX, because each loader brings its own.

See `ExampleMod` and `ExamplePrePatcher` for more.

### Config

- Every `ConfigurationKey` field of the config class is a key. Mods without config use an empty config class.
- Key ids can't contain `= \n \t \ " ' [ ]` or start or end with whitespace. Values can be `string`, `bool`, number types, `decimal`, enums or Resonite primitives (like `float3` or `colorX`). Anything else throws when the key is created.
- `SetValue` throws for values the validator rejects, `TrySetValue` returns `false`. Invalid values in a config file become the default (on BepisLoader, range values are clamped and list values become the first one).
- `OnChanged` only fires when the value changes.
- For string keys, treat `null` and `""` the same.
- `RangeValueValidator` shows as a slider on BepisLoader and MonkeyLoader, `ListValueValidator` as a dropdown on BepisLoader.
- `ConfigTags.Hidden` hides a key from config UIs.

### Features

Not every loader has every feature (RML has no pre-patching). `[FeatureRequirement(Feature.PrePatching)]` stops the mod loading where it's missing. With `RequirementType.Optional` the mod loads anyway and checks `HasFeature`.

### Pre-patchers

Derive from `UniPrePatcher<TPatcher, TConfig>` and reference `Mono.Cecil` 0.11.6. Patch methods work like BepInEx's:

- `[TargetAssembly]` or `[TargetType]` (both can be repeated), or `[TargetAllAssemblies]`;
- an `AssemblyDefinition` (optionally `ref`) or `TypeDefinition` parameter, and optionally a `string` parameter that gets the file name;
- return `void` or `true` if the method changed something, `false` if not.

On BepisLoader patch methods still run if `OnInitialize` fails.

## Building

```bash
dotnet build UniModFramework.slnx -p:CopyToPlugins=false
dotnet test UniModFramework.Tests --no-build
```

To copy builds into the game, add a `Directory.Build.props.user` (git ignores it):

```xml
<Project>
  <PropertyGroup>
    <GamePath>/path/to/Resonite/</GamePath>
    <BepInExProfileDir>/path/to/BepInEx/profile/</BepInExProfileDir>
    <CopyToPlugins>true</CopyToPlugins>
  </PropertyGroup>
</Project>
```

Without `GamePath`, the default Steam locations and the `ResonitePath` environment variable are tried.

How the backends work, the design decisions and the release steps are in `CLAUDE.md`.
