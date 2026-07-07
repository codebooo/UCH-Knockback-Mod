# UCH Knockback Mod

A BepInEx mod for Ultimate Chicken Horse: left-click to knock back other characters near you.

## How it works

- Press the knockback key (default: left mouse button) during a round.
- Every living character within the configured radius of **your** character gets pushed away, using the same impulse system the game's springs and punching blocks use — so the knockback interacts correctly with jumps, walls and gravity.

## Configuration

`BepInEx/config/com.uchknockback.mod.cfg` (created on first launch):

| Setting | Default | Description |
|---|---|---|
| EnableMod | true | Master switch |
| Force | 15 | Impulse strength (springs use ~10-20) |
| Radius | 3 | Effect radius in world units |
| UpwardBoost | 0.35 | Upward bias so grounded characters lift off |
| Cooldown | 0.5 | Seconds between triggers |
| KnockbackKey | Mouse0 | Trigger key |

## Notes

- Works fully in local (couch) multiplayer.
- In online games, physics for remote players is controlled by their machine, so knockback applied to them will not sync unless they also run the mod.

## Installation

r2modman: install normally. Manual: drop `KnockbackMod.dll` into `BepInEx/plugins/`.
