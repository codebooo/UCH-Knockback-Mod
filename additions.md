## 1. Directional Knockback by Aim Vector
**Description**: Push targets in the direction of mouse/cursor aim instead of pure radial push to add skill-based control.

**Technical approach**: In `KnockbackPlugin.Update`, read cursor world position from camera and compute aim vector from local player. Reuse existing knockback loop but optionally override direction with aim. Keep force application in `FixedUpdate`.
```csharp
Vector3 mouse = Input.mousePosition;
var cam = Camera.main;
if (cam == null) return; // or fallback to radial behavior
Vector3 world = cam.ScreenToWorldPoint(mouse);
Vector2 aim = ((Vector2)world - (Vector2)localPlayer.transform.position).normalized;
```

**Config keys**: `UseDirectionalAim` (`ConfigEntry<bool>`), `AimFallbackToRadial` (`ConfigEntry<bool>`).

**Multiplayer consideration**: Apply only to local simulation unless remote validation is available; if remote clients lack mod, keep default radial/local-only effect to reduce divergent outcomes.

## 2. Cooldown and Anti-Spam Control
**Description**: Prevent rapid-fire knockback spam by enforcing a configurable cooldown between activations.

**Technical approach**: Add timestamp check in trigger path (`Time.time`), reject when still on cooldown, optionally play feedback log/UI cue.
```csharp
if (Time.time < _nextAllowedTime) return;
_nextAllowedTime = Time.time + KnockbackCooldown.Value;
```

**Config keys**: `KnockbackCooldown` (`ConfigEntry<float>`), `ShowCooldownLogs` (`ConfigEntry<bool>`).

**Multiplayer consideration**: Deterministically local and safe when others do not have mod; reduces network stress by lowering impulse frequency.

## 3. Team/Friend Exclusion Rules
**Description**: Avoid affecting teammates or configured friend IDs for cooperative/custom sessions.

**Technical approach**: Patch or inspect UCH player identity/team component in candidate filtering (`IsLikelyPlayer` replacement). Skip force when team relation matches exclusion rule.
```csharp
if (ExcludeSameTeam.Value && source.TeamId == target.TeamId) return false;
```

**Config keys**: `ExcludeSameTeam` (`ConfigEntry<bool>`), `ExcludedPlayerNames` (`ConfigEntry<string>` CSV).

**Multiplayer consideration**: Local filtering only; if peers are unmodded they still observe fewer local impulses, which is safer than inconsistent target selection across modded peers.

## 4. Line-of-Sight Requirement
**Description**: Require unobstructed path before knockback applies, preventing through-wall hits.

**Technical approach**: During target evaluation, run `Physics2D.Raycast`/`Physics.Raycast` from source to target with obstacle layers; apply force only when unobstructed.
```csharp
var hit = Physics2D.Raycast(src, dir, dist, ObstacleMask.Value);
if (hit.collider != null) return false;
```

**Config keys**: `RequireLineOfSight` (`ConfigEntry<bool>`), `ObstacleLayerMask` (`ConfigEntry<int>`).

**Multiplayer consideration**: Purely local rule; gracefully degrades when peers are unmodded because blocked targets simply receive no impulse from modded player.

## 5. Falloff Force Curve
**Description**: Scale impulse by distance (full force near center, weaker at edge) for better game feel and balance.

**Technical approach**: Compute normalized distance and evaluate linear/exponential falloff before `AddForce`.
```csharp
float t = Mathf.Clamp01(distance / KnockbackRadius.Value);
float scaledForce = KnockbackForce.Value * Mathf.Lerp(1f, MinForceMultiplier.Value, t);
```

**Config keys**: `UseForceFalloff` (`ConfigEntry<bool>`), `MinForceMultiplier` (`ConfigEntry<float>`), `FalloffExponent` (`ConfigEntry<float>`).

**Multiplayer consideration**: Deterministic formula keeps behavior aligned among modded clients; unmodded peers still see only host/local physics authority results.

## 6. Chargeable Knockback
**Description**: Holding the key charges impulse strength, creating risk/reward timing in close encounters.

**Technical approach**: Track key down duration (`Input.GetKey`), clamp to max charge, apply multiplier on release (`Input.GetKeyUp`).
```csharp
if (Input.GetKey(KnockbackKey.Value)) _charge = Mathf.Min(_charge + Time.deltaTime, MaxChargeTime.Value);
if (Input.GetKeyUp(KnockbackKey.Value)) TriggerKnockback(1f + _charge * ChargeRate.Value);
```

**Config keys**: `EnableCharge` (`ConfigEntry<bool>`), `MaxChargeTime` (`ConfigEntry<float>`), `ChargeRate` (`ConfigEntry<float>`).

**Multiplayer consideration**: Local charge state only; if mixed-mod lobby, keep conservative max values to reduce visible divergence.

## 7. Self-Recoil / Risk Tradeoff
**Description**: Add optional backward recoil to the caster to balance aggressive use and create movement tech.

**Technical approach**: After applying outward impulses, apply opposite impulse to local player's rigidbody.
```csharp
if (SelfRecoilEnabled.Value) localRb2d.AddForce(-aimDir * SelfRecoilForce.Value, ForceMode2D.Impulse);
```

**Config keys**: `SelfRecoilEnabled` (`ConfigEntry<bool>`), `SelfRecoilForce` (`ConfigEntry<float>`).

**Multiplayer consideration**: Recoil affects only local controlled actor; low risk in mixed lobbies since it does not require remote mod support.

## 8. Optional Host-Only Enforcement
**Description**: Restrict knockback authority to host/master client to reduce conflicting impulses in multiplayer.

**Technical approach**: Patch/inspect UCH network session state (host flag in game networking manager) before triggering knockback; deny on non-host clients when enabled.
```csharp
if (HostOnlyKnockback.Value && !IsLocalHost()) return;
```

**Config keys**: `HostOnlyKnockback` (`ConfigEntry<bool>`), `WarnWhenBlockedByHostOnly` (`ConfigEntry<bool>`).

**Multiplayer consideration**: Explicitly mitigates mixed-mod desync by centralizing authority; clients without mod still receive host-side physics outcomes only.

## 9. Target Filtering by Rigidbody Type
**Description**: Limit knockback to dynamic rigidbodies, skipping kinematic/static actors and props.

**Technical approach**: In `ApplyKnockbackToPlayer`, validate `Rigidbody2D.bodyType == Dynamic` or `!Rigidbody.isKinematic` before force.
```csharp
if (rb2d != null && rb2d.bodyType != RigidbodyType2D.Dynamic) return false;
if (rb != null && rb.isKinematic) return false;
```

**Config keys**: `AffectOnlyDynamicBodies` (`ConfigEntry<bool>`), `IncludeKinematic` (`ConfigEntry<bool>`).

**Multiplayer consideration**: Safer in mixed environments because non-authoritative transforms are less likely to be forcibly moved.

## 10. In-Game Config Reload Hotkey
**Description**: Reload mod configuration without restarting game to support rapid tuning during matches.

**Technical approach**: Add second input binding to call `Config.Reload()` and refresh cached runtime values; log success/errors.
```csharp
if (Input.GetKeyDown(ReloadConfigKey.Value))
{
    Config.Reload();
    RefreshCachedSettings();
}
```

**Config keys**: `ReloadConfigKey` (`ConfigEntry<KeyCode>`), `LogConfigReload` (`ConfigEntry<bool>`).

**Multiplayer consideration**: Local-only settings refresh; does not require peer mods and avoids forced session restarts.
