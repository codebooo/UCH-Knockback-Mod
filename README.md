# UCH Knockback Mod

A BepInEx mod for Ultimate Chicken Horse that lets you knock back other players by left-clicking near them.

Unlike v1.0.0 (which guessed at GameObject names and never actually found a player), this version hooks into the game's real `Character` API and applies knockback through `Character.AddImpulse` — the same impulse system used by the game's own springs, punching blocks and cannons.

## Features

- **Left-click knockback**: every living character within the radius of your character gets pushed away
- **Game-native physics**: uses `Character.AddImpulse`, so knockback interacts correctly with jumps, walls and gravity
- **Configurable**: force, radius, upward boost, cooldown and trigger key
- **r2modman compatible**

## Installation

### r2modman
Install `BepInExPack` for Ultimate Chicken Horse, then install this mod.

### Manual
1. Install [BepInEx 5.x](https://thunderstore.io/c/ultimate-chicken-horse/p/BepInEx/BepInExPack/) into your Ultimate Chicken Horse directory
2. Download `KnockbackMod.dll` from the [releases](https://github.com/codebooo/UCH-Knockback-Mod/releases) or the packaged zip
3. Place `KnockbackMod.dll` in `BepInEx/plugins/`
4. Launch the game

## Configuration

Created on first launch at `BepInEx/config/com.uchknockback.mod.cfg`:

| Setting | Default | Description |
|---|---|---|
| EnableMod | true | Master switch |
| Force | 15 | Impulse strength (the game's springs use ~10-20) |
| Radius | 3 | Effect radius in world units |
| UpwardBoost | 0.35 | Upward bias so grounded characters lift off the floor |
| Cooldown | 0.5 | Seconds between knockback triggers |
| KnockbackKey | Mouse0 | Trigger key (left mouse button) |

## Multiplayer notes

- **Local (couch) play**: works fully — all characters are simulated on your machine.
- **Online play**: remote characters are simulated on their owner's machine, so knockback you apply to them won't sync unless they run the mod too. Your own character is always affected correctly.

## Building from source

```
cd src
dotnet build -c Release -p:GameDir="C:\Path\To\Steam\steamapps\common\Ultimate Chicken Horse"
```

The csproj references `BepInEx.dll` from `<GameDir>\BepInEx\core` and the game assemblies from `<GameDir>\UltimateChickenHorse_Data\Managed`. Alternatively, place those DLLs in a `libs/` folder at the repo root (see `src/KnockbackMod.csproj` for the exact list) and build without `-p:GameDir`.

Output: `src/bin/Release/net472/KnockbackMod.dll`

## Troubleshooting

1. Check `BepInEx/LogOutput.log` for `UCH Knockback Mod v1.1.0 loaded`
2. Knockback only works during a round, while your character is alive
3. If nothing happens on click, another window may have focus, or `EnableMod` is false in the config

## UCH Modding Communities

- **Thunderstore (UCH community page)**: https://thunderstore.io/c/ultimate-chicken-horse/
- **BepInEx Pack for UCH**: https://thunderstore.io/c/ultimate-chicken-horse/p/BepInEx/BepInExPack/
- **Community modding projects on GitHub**:
  - https://github.com/fifty-six/UltimateChickenHorse.Modding
  - https://github.com/batram/UCH-CustomBlocks
  - https://github.com/notfood/UCH-UltimateBuilder

## License

This mod is provided as-is for the Ultimate Chicken Horse community.
