# Changelog

## 1.1.0

Complete rewrite — the 1.0.0 approach could never find a player and did nothing.

- Use the game's real `Character` class instead of guessing GameObjects by name ("player"/"character" — UCH characters are not named that, so 1.0.0 never found anyone).
- Apply knockback through `Character.AddImpulse`, the same API used by the game's springs, punching blocks and cannons, instead of a raw `Rigidbody2D.AddForce` that the character controller immediately damps out.
- Find the local player via the game's `PlayerManager`/`Character.LocalPlayer` instead of picking the first "player-like" object (which could be a remote player or UI element).
- Add upward boost so grounded characters actually lift off.
- Add trigger cooldown (default 0.5 s) to prevent click spam.
- Remove the pointless Harmony `PatchAll()` call (there were no patches).
- Skip dead/dying characters.
- Fix per-click scan of every GameObject in the scene (thousands of objects) — now iterates the game's own character list.

## 1.0.0

- Initial release (non-functional).
