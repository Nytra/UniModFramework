NOTE: This is still being developed and there may be breaking changes!

## How it works

Mods are compiled against the `UniModFramework` project, which only builds a reference assembly. Each loader has its own backend project (`UniModFramework.RML`, `UniModFramework.MonkeyLoader`, `UniModFramework.BepisLoader`) that builds a `UniModFramework.dll` with the same types and members on top of that loader's API, and the user installs the backend for their loader.

- `Shared/` is compiled into every project: the config interfaces, validators, key discovery and the loader-independent half of `ConfigurationKey`.
- `UniModFramework/UniMod.cs`, `UniPrePatcher.cs` and `Attributes.cs` are compiled into the backends too. Each backend adds its loader-specific parts with `partial` classes and methods (`StoreValue`, `WriteLog`, `Backend.HasFeature`), so a backend missing one doesn't compile.
- `UniModFramework/Backend.cs` and `Config.cs` are stubs only used by the reference assembly.

Every public or protected member of the reference assembly must also exist in each backend (declared or inherited), or mods fail at runtime. `ApiCompatibilityTests` checks this.

`CLAUDE.md` has maintainer notes: how each loader behaves, why things are done the way they are, and open items.

## Writing mods

- Derive from `UniMod<TMod, TConfig>`, pre-patchers from `UniPrePatcher<TPatcher, TConfig>`. Mods without config keys use an empty config class.
- Harmony comes with the `Nytra.UniModFramework` package as `Lib.Harmony.Ref` 2.3.6, the newest Harmony whose whole API HarmonyX (used by BepisLoader) also has. Each loader provides its own Harmony at runtime (RML and MonkeyLoader use Harmony 2.4, BepisLoader uses HarmonyX), so don't reference a newer Harmony or HarmonyX, or the mod may use members that are missing on some loaders.
- Pre-patchers also need `Mono.Cecil` for the patch methods. MonkeyLoader and BepisLoader ship 0.11.6.
- `Config` is available from `OnLoad`/`OnInitialize` onwards. Every `ConfigurationKey` field of the config class (and its base classes) is a key, and `Config.ConfigurationKeys` lists them.
- Keys follow BepInEx's rules on every loader, so they work everywhere: ids can't contain `= \n \t \ " ' [ ]` or start or end with whitespace, and values can be `string`, `bool`, a number type, `decimal`, an enum or a Resonite primitive (like `float3`, `colorX`, `Uri` or `DateTime`). Other ids and types throw an `ArgumentException` when the key is created.
- `OnChanged` only fires when the value actually changes. For string keys, `null` and `""` should be treated the same, since BepInEx saves `null` as `""`.
- `SetValue` throws an `ArgumentException` for values the validator rejects and keeps the old value, `TrySetValue` returns `false` instead. `ResetToDefault` sets the default.
- Invalid values loaded from a config file are replaced with the default and a warning is logged. On BepisLoader, BepInEx clamps range values and replaces list values with the first one instead.
- Range validators show as sliders in BepisModSettings and MonkeyLoader's settings, and list validators as dropdowns in BepisModSettings. ResoniteModSettings doesn't show sliders for them yet.
- Tags work like BepInEx's and are only used by BepisLoader, except `ConfigTags.Hidden`, which hides the key from config UIs on every loader (MonkeyLoader and RML make it internal access only).
- `LogDebug`, `LogInfo`, `LogWarn` and `LogError` write to the loader's log and are static, so Harmony patch classes can use them.
- `[FeatureRequirement(Feature.PrePatching)]` on the mod class stops it loading (with an error logged) on loaders without that feature. With `RequirementType.Optional` the mod still loads, a message is logged saying some of its features are off, and the mod checks `HasFeature` to work without it.
- Returning `false` from `OnLoad`, `OnReady`, `OnInitialize` or `OnFinalize` logs an error. On BepisLoader patch methods still run if `OnInitialize` fails.
- Pre-patch methods follow BepInEx's rules, which MonkeyLoader emulates: `[TargetAssembly]`, `[TargetType]` (both can be repeated) or `[TargetAllAssemblies]`, an `AssemblyDefinition` (optionally `ref`) or `TypeDefinition` parameter, an optional second `string` parameter that gets the target file name, and returning `void` or `true` to report changes.

## Versioning

Mods built against an older UniModFramework keep working on newer ones, as long as nothing they use is removed or changed. To keep it that way:

- The `Nytra.UniModFramework` package has NuGet package validation enabled. Once a version is published, set `PackageValidationBaselineVersion` in `UniModFramework/UniModFramework.csproj` to it, and `dotnet pack` fails if the API breaks mods built against that version.
- Mark members `[Obsolete]` and add new ones instead of removing or changing them, and keep behaviour mods rely on.
- Bump `Version` in `Directory.Build.props` for each release. A mod built against a newer UniModFramework than the installed one isn't loaded and an error is logged instead, since it may use members the installed one doesn't have.

## Building

Builds don't copy into the game unless you ask. Put machine-specific settings in a `Directory.Build.props.user` next to `Directory.Build.props` (git ignores it):

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

## Testing

```bash
dotnet build UniModFramework.slnx -p:CopyToPlugins=false
dotnet test UniModFramework.Tests --no-build
```

`ApiCompatibilityTests` checks every backend build against the reference assembly's API, so the backends must be built first. The other tests cover the shared config, validator, version check and feature requirement code. The loader backends themselves need testing in-game.
