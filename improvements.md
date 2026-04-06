## 1. Add bounded config values with `AcceptableValueRange`
**Problem**: `KnockbackPlugin.Awake` binds `Force` and `Radius` without bounds, so invalid values (negative/very large) can be loaded from `com.uchknockback.mod.cfg`.

**Solution**: Use `ConfigDescription` + `AcceptableValueRange<float>` when binding.
```csharp
KnockbackForce = Config.Bind(
    "Knockback", "Force", 15f,
    new ConfigDescription("Impulse strength", new AcceptableValueRange<float>(0f, 100f)));
KnockbackRadius = Config.Bind(
    "Knockback", "Radius", 3f,
    new ConfigDescription("Effect radius", new AcceptableValueRange<float>(0.1f, 20f)));
```

**Impact**: Prevents broken physics behavior and improves config safety.

## 2. Validate plugin metadata version consistency
**Problem**: `[BepInPlugin(..., "1.0.0")]` in `KnockbackPlugin` conflicts with documented release `1.0.1`.

**Solution**: Replace hardcoded version with a single constant used by both attribute and logs.
```csharp
internal const string PluginVersion = "1.0.1";
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
```

**Impact**: Avoids user confusion and mismatched mod manager metadata.

## 3. Replace scene-wide scans with physics overlap queries
**Problem**: `FindLocalPlayer` and `ApplyKnockbackToNearbyPlayers` each call `GameObject.FindObjectsOfType<GameObject>()`, causing full-scene iteration and heavy allocations every activation.

**Solution**: Query nearby colliders from source position (`Physics2D.OverlapCircleNonAlloc` / `Physics.OverlapSphereNonAlloc`) and resolve candidate players from hit colliders.
```csharp
private readonly Collider2D[] _hits2D = new Collider2D[32];
int count = Physics2D.OverlapCircleNonAlloc(sourcePosition, KnockbackRadius.Value, _hits2D);
```

**Impact**: Lower CPU and GC pressure; scales better in busy levels.

## 4. Cache local player reference and refresh lazily
**Problem**: `FindLocalPlayer` performs costly object-name heuristics on every key press.

**Solution**: Cache `Transform`/`GameObject` for local player and refresh only when null, inactive, or scene changes.
```csharp
if (_localPlayer == null || !_localPlayer.activeInHierarchy) _localPlayer = ResolveLocalPlayer();
```

**Impact**: Better runtime performance and fewer false positives.

## 5. Eliminate `ToLower()` string allocations in player heuristics
**Problem**: `obj.name.ToLower()` in `FindLocalPlayer`/`IsLikelyPlayer` allocates per object.

**Solution**: Use `IndexOf(..., StringComparison.OrdinalIgnoreCase)`.
```csharp
var n = obj.name;
bool looksPlayer = n.IndexOf("player", StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("character", StringComparison.OrdinalIgnoreCase) >= 0;
```

**Impact**: Reduces per-activation garbage and improves search speed.

## 6. Move knockback application to `FixedUpdate` queue
**Problem**: `Update` directly calls `AddForce`; physics impulses are best synchronized with fixed timestep.

**Solution**: Capture trigger input in `Update`, then consume pending action in `FixedUpdate`.
```csharp
private bool _pendingKnockback;
private void Update() { if (Input.GetKeyDown(KnockbackKey.Value)) _pendingKnockback = true; }
private void FixedUpdate() { if (_pendingKnockback) { _pendingKnockback = false; ApplyKnockbackFromLocalPlayer(); } }
```

**Impact**: More stable and deterministic force application.

## 7. Remove transform-position fallback for non-rigidbody targets
**Problem**: `ApplyKnockbackToPlayer` directly modifies `player.transform.position` when no rigidbody exists, bypassing physics/network reconciliation.

**Solution**: Skip non-physics targets (or only move own local proxy) and log once at debug level.
```csharp
if (rb2d == null && rb == null) return false;
```

**Impact**: Fewer desyncs/teleports and safer multiplayer behavior.

## 8. Compute planar knockback direction explicitly
**Problem**: Current 2D force uses normalized 3D direction cast to `Vector2`; z-offset can skew magnitude.

**Solution**: Build 2D direction from x/y only.
```csharp
Vector2 dir2D = ((Vector2)player.transform.position - (Vector2)sourcePosition).normalized;
rb2d.AddForce(dir2D * KnockbackForce.Value, ForceMode2D.Impulse);
```

**Impact**: Correct 2D physics behavior and predictable force strength.

## 9. Gate verbose per-target logs behind a debug config
**Problem**: `ApplyKnockbackToPlayer` logs each target at `Info`; this can spam logs in active sessions.

**Solution**: Add `ConfigEntry<bool> DebugLogging` and guard high-frequency logs.
```csharp
if (DebugLogging.Value) Logger.LogInfo($"Applied 2D knockback to {player.name}");
```

**Impact**: Cleaner logs and lower logging overhead.

## 10. Make Harmony usage explicit and correct for actual hooks
**Problem**: `harmony.PatchAll()` is called in `KnockbackPlugin.Awake` (`KnockbackPlugin.cs` lines 39-40), but `KnockbackPlugin.cs` defines no `[HarmonyPatch]` classes, so patching intent is unclear.

**Solution**: Either remove Harmony initialization until real patches exist, or add explicit patches with correct `Prefix`/`Postfix` semantics (e.g., `Prefix` to cancel default behavior, `Postfix` to react after game state updates).
```csharp
[HarmonyPatch(typeof(TargetType), nameof(TargetType.TargetMethod))]
static class TargetPatch
{
    static void Postfix(TargetType __instance) { /* read final state, then apply mod logic */ }
}
```

**Impact**: Improves patch correctness, maintainability, and compatibility with other mods.
