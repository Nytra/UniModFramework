# UniModFramework maintainer notes

`README.md` covers how the framework works and how to write mods. This file holds what was learned while building it: how each loader behaves, why things are done the way they are, and what is still open.

## Commands

```bash
dotnet build UniModFramework.slnx -p:CopyToPlugins=false
dotnet test UniModFramework.Tests --no-build
```

Use `-p:CopyToPlugins=false` so test builds don't copy into the game or Gale profile (`Directory.Build.props.user` turns copying on for local dev). After changing analyzers or build settings, use `--no-incremental`: incremental builds can skip the compile and hide errors.

## Rules for changes

- Every public or protected member of the reference project must exist in each backend, declared or inherited, or mods built against it fail at runtime. `ApiCompatibilityTests` checks this against the built DLLs.
- Don't remove or change public members once a version is published. Mark them `[Obsolete]` and add new ones. Package validation enforces this once `PackageValidationBaselineVersion` is set.
- Code that is the same for every loader goes in `Shared/` or the shared `UniModFramework/UniMod.cs`, `UniPrePatcher.cs`, `Attributes.cs`. Loader-specific parts are `partial` classes and methods in each backend, so a missing implementation is a compile error.
- Follow the design below, and check loader behaviour against the loader's source, not assumptions. Source repos:
  - MonkeyLoader: https://github.com/MonkeyModdingTroop/MonkeyLoader (the NuGet package used is 0.29.0)
  - ResoniteModLoader: https://github.com/resonite-modding-group/ResoniteModLoader
  - BepisLoader (includes BepInEx.Core and BepInEx.Preloader.Core): https://github.com/ResoniteModding/BepisLoader
  - BepisModSettings: https://github.com/ResoniteModding/BepisModSettings

## Design

These were agreed after several rounds of going back and forth, so change them only deliberately.

1. Same behaviour on every loader. A mod must not need to know which loader it runs on.
2. Forward to the loader when all three agree (change events, log levels, resetting to the default). The framework adds nothing there.
3. When the loaders disagree, pick what makes most sense for mod authors, not what one particular loader does, and emulate it on the others. If two options are about equally good, prefer the one that's cheaper to emulate.
4. Things a loader can't provide become `Feature`s that mods check with `HasFeature` or require with `[FeatureRequirement]`, like pre-patching on RML.
5. Keep the API small. Adding something later doesn't break mods; removing it does.

Decisions made with these:

| Topic | BepInEx | MonkeyLoader | RML | Decision |
|---|---|---|---|---|
| `SetValue` with an invalid value | replaces it | throws | throws (`ModConfiguration.Set`) | Throw `ArgumentException` and keep the old value. Surfaces bugs in the mod's code. |
| `TrySetValue` | – | ✓ | – | Keep, it's the natural partner of a throwing `SetValue`. |
| Invalid value in a config file | replaces it | stops loading the section | keeps it | Use the default and log a warning, so the user knows. Exception: on BepisLoader, BepInEx clamps range values and replaces list values with the first one before the framework sees them, because those use BepInEx's own validator types (for BepisModSettings' sliders and dropdowns). |
| Reset to default | `Value = DefaultValue` | `Reset()` | `ModConfiguration.Unset` | `ResetToDefault`, using each loader's way. |
| Unset | – | ✓ (returns `default(T)`) | ✓ (returns the default) | Not in the API, the loaders disagree on what it means. |
| Tags | free-form | internal-access-only flag | internal-access-only flag | Free-form tags, passed to BepInEx. `ConfigTags.Hidden` works everywhere. |
| Pre-patching | full | most of BepInEx's model | none | BepInEx's model (`[TargetAllAssemblies]`, the optional second parameter, `void`/`bool` returns), emulated on MonkeyLoader. A `Feature` because RML has none. |
| Sliders for range validators | BepisModSettings, via `AcceptableValueRange` | MonkeyLoader's settings, via `IConfigKeyRange<T>` | ResoniteModSettings, only for mod-class fields with `FrooxEngine.RangeAttribute` | Use each loader's way. Not possible on RML yet, see open items. |
| Dropdowns for list validators | BepisModSettings, via `AcceptableValueList` | enums only | enums only | Only on BepisLoader. |
| Key ids | rejects `= \n \t \ " ' [ ]` and surrounding whitespace | anything | anything | BepInEx's rules on every loader, checked when the key is created (`ConfigKeyRules`). |
| Value types | string, bool, numbers, decimal, enums, plus Resonite's engine primitives (`GenericTypesAttribute` group `EnginePrimitives`, e.g. `float3`, `colorX`, `Uri`, `DateTime`), which the BepisLoader shim adds | anything JSON can store | anything JSON can store | BepisLoader's set on every loader, checked when the key is created. Supporting more types on BepisLoader (e.g. by storing them as JSON text) was considered and left for when mods need it. |
| `OnChanged` when setting the same value | not fired | not fired | fired | Not fired: `TrySetValue` and `ResetToDefault` skip unchanged values. |
| Description | required (`ConfigDescription` throws on `null`) | optional | optional | Optional, the BepisLoader backend passes `""`. |
| `null` strings | saved as `""`, so `null` comes back as `""` after a restart | kept | kept | `null` and `""` mean the same for string keys, mods shouldn't rely on the difference. |
| When changes are saved | on every set | at shutdown | at shutdown (`AutoSave`) | Left to the loaders. All three can save explicitly, so a `Config.Save()` could be added if needed. |

## Loader behaviour

### MonkeyLoader (0.29.0)

- `PrePatchTarget.AllAvailable` is an empty enumerable and nothing treats it specially, so returning it gives no patch jobs. `UniPrePatcher` targets every `.dll` in `MonkeyLoader.GameAssemblyPath` instead, the same files MonkeyLoader loads definitions for.
- If `PatchJob.Changes` isn't set, `PatchJob.Finish` restores the original assembly and the patch is lost. Setting `PatchJob.Assembly` to a different instance also sets it.
- Each assembly in the pool has an `AutoResetEvent`, and `EarlyMonkey.Run` creates every patch job before running any. Two targets for the same assembly in one monkey wait forever, so `GetPrePatchTargets` makes one target per assembly.
- `MonkeyLoader.AssemblyName` compares names ignoring case, so targets are deduplicated and matched ignoring case too.
- `Config.LoadSection` calls `InitializeKeys` (which calls `GetConfigKeys`) before reading saved values, so a section is usable with defaults even if loading throws.
- `ConfigSection.OnLoad` stops at the first value that fails validation, throws `ConfigLoadException` and sets `Saveable = false`, so later keys keep their defaults and nothing is saved that session. `ConfigLoadException`'s constructors are internal. `Config.OnLoad` therefore removes invalid values from the JSON before calling the base, and reports them as warnings.
- MonkeyLoader has its own validator components: `ConfigKeyRange<T>` (also an `IConfigKeyRange<T>`, which makes MonkeyLoader's settings show a slider) for range validators, and the predicate `ConfigKeyValidator<T>` for the others.
- `ConfigSection` already has a `Keys` member, which is why the framework's is called `ConfigurationKeys`.
- `DefiningConfigKey.Reset()` sets the default and fires a change event labelled `"Reset"`, which `ResetToDefault` uses. `Unset()` removes the value, after which `GetValue()` returns `default(T)`, not the key's default.
- `MonkeyBase<T>.Logger` is static in the packaged version (it's an instance property on the repo's main branch).
- `ConfiguredMonkey.OnLoaded` and `ConfiguredEarlyMonkey.Prepare` only load the config section and call the base. `Monkey.OnLoaded` does `Harmony.PatchCategory(...)`, which `UniMod` deliberately skips since mods patch in `OnLoad`.
- `EarlyMonkey.Prepare()` returning false skips pre-patching without failing. `UniPrePatcher.Validate` returns `OnFinalize()` instead of the base check that no job failed. Failed jobs are still restored, but the pre-patcher as a whole doesn't report the failure.

### ResoniteModLoader

- `ModConfigurationKey<T>.Value`'s setter and config file loading don't validate values. Only `ModConfiguration.Set` does, and it throws. The shared `TrySetValue` validates before setting `Value`, and `Config.ResetInvalidValues` handles invalid values loaded from the config file.
- ResoniteModSettings maps keys to fields by scanning the mod class for fields with `[AutoRegisterConfigKey]`, and only shows a slider when that field has `FrooxEngine.RangeAttribute`. The framework's keys are fields of the config class, so they never get a slider.
- `DefineConfiguration` runs before `OnEngineInit`, and the config file is loaded in between.
- Keys are reset by removing their value with the public `ModConfiguration.Unset(key)` (the key's own `Unset` is internal), after which the key returns its default. Keys don't know their `ModConfiguration`, so `ResetToDefault` sets the default value instead, which the config file then stores.
- The static `ResoniteMod.Debug`/`Msg`/`Warn`/`Error` work out the calling mod from the stack trace.
- There is no pre-patching, so nothing runs a `UniPrePatcher`.

### BepisLoader / BepInEx

- Pre-patch methods: the first parameter is `AssemblyDefinition` (optionally `ref`) or `TypeDefinition`, an optional second `string` parameter gets the target file name, and the return type is `void` or `bool`. `void` or `true` means changed. A `ref` assembly is only used when the method reports changes. Target types are looked up by full name in `MainModule.Types`. BepInEx can also target all assemblies. The MonkeyLoader backend emulates all of this.
- BepInEx's target attributes are `AllowMultiple = true`.
- There's no reset method, the way to reset is setting `Value` to `DefaultValue`.
- `ConfigEntry` clamps values through `AcceptableValues.Clamp` when they're set and loaded. `AcceptableValueRange` clamps to its bounds, and `AcceptableValueList` falls back to its first value. `AcceptableValueBase` requires a `Clamp`, so `ValidatorAdapter` (used for validators other than range and list) returns the default. The framework validates in `TrySetValue` first, so these only matter for values loaded from config files and set from config UIs.
- BepisModSettings only shows sliders and dropdowns when `AcceptableValues`' generic type definition is exactly `AcceptableValueRange<>` or `AcceptableValueList<>`, so subclasses don't work. The backend creates the real BepInEx types from the shared validators. Other validators go through `ValidatorAdapter`, whose `Clamp` returns the default.
- BepisModSettings hides keys tagged with the string `"Hidden"` (or its own `HiddenConfig`), which is `ConfigTags.Hidden`. Tags are passed straight to BepInEx. MonkeyLoader and RML only have an internal-access-only flag, which `ConfigTags.Hidden` sets; other tags do nothing there.
- `BasePlugin.Log` and `BasePatcher.Log` are instance properties set in the base constructor, so the backends copy them to a static field.
- BepInEx runs a patcher's patch methods even when `OnInitialize` fails.

## Dependencies

- Each loader ships its own `0Harmony`: RML has Harmony 2.4.3, MonkeyLoader Harmony 2.4.2, BepisLoader HarmonyX 2.16.1. Mods bind to whichever is installed, as long as the version they compiled against isn't higher.
- HarmonyX 2.16.1 lacks 87 public members of Harmony 2.4.2 (inner prefixes/postfixes, event and operator `MethodType`s, some `CodeMatcher` methods, `Harmony.SetSwitch`). 2.2.2 and 2.3.3 have over 200 members HarmonyX lacks. `Lib.Harmony.Thin`/`Lib.Harmony.Ref` 2.3.6 have none, apart from the nested enum `InlineSignature.ModifierType`. That's why the package depends on `Lib.Harmony.Ref` 2.3.6. Re-check this when updating Harmony or when BepisLoader updates HarmonyX.
- MonkeyLoader and BepisLoader both ship Mono.Cecil 0.11.6.

## Build decisions

- The reference project uses `ProduceOnlyReferenceAssembly`, so its DLL can't be loaded by mistake and the stub bodies don't matter. The tests compile its source directly instead of referencing it.
- `Microsoft.CodeAnalysis.PublicApiAnalyzers` was tried and dropped: with `ProduceOnlyReferenceAssembly` it can't see any members (false RS0017 errors on every member), and listing every new member was too much friction before 1.0. NuGet package validation is used instead. Verified locally: packing 1.1.0 against a 1.0.0 baseline with a removed member fails with `CP0002`.
- The examples use `ProjectReference` with `Private="false" PrivateAssets="all"`, so `UniModFramework.dll` isn't copied and their MonkeyLoader packages don't depend on `Nytra.UniModFramework`, which wouldn't resolve.
- `UniMod<T>` / `UniPrePatcher<T>` without a config were tried and removed: they only save typing `, Config`, add API surface, and add an untested inheritance level for BepInEx plugin discovery. Mods without config keys use an empty config class.
- `FeatureRequirementAttribute`'s `RequirementType.Optional` has no effect yet besides showing that the mod uses the feature.

## Open items

- Nothing has been tested in-game since the redesign. Check on each loader:
  - logging;
  - config loading, including a config file with an invalid value, which should become the default with a warning (except range and list values on BepisLoader);
  - MonkeyLoader's settings showing sliders for range keys;
  - pre-patching on MonkeyLoader, including two patch methods on the same assembly and `[TargetAllAssemblies]`;
  - BepisModSettings sliders and dropdowns for range and list keys;
  - BepInEx still finding plugins through `UniMod<,>`.
- No sliders on RML, deliberately left for later. If it becomes important, the preferred option is a runtime patch of ResoniteModSettings so it finds a range for the framework's keys (depends on its internals). Asking its maintainer to read a range from the key itself was considered and rejected.
- The config UIs of the loaders handle invalid input their own way, the framework can't change that without patching them.
- After the first NuGet release, set `PackageValidationBaselineVersion` in `UniModFramework/UniModFramework.csproj`.
- The reference project's PostBuild still copies the reference-only `UniModFramework.dll` to `out/`, which nothing uses now.
- No unit tests cover the backends themselves (they need the loaders running), only the shared code and the API surface.
