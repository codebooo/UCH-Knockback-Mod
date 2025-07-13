# UCH Knockback Mod

A BepInEx mod for Ultimate Chicken Horse that adds player knockback functionality when left-clicking.

## Features

- **Left-click knockback**: Hit or push other players by left-clicking near them
- **Configurable settings**: Adjust knockback force and radius through the config file
- **r2modman compatible**: Easy installation through mod managers
- **Safe implementation**: Uses Harmony patches to safely modify game behavior

### Manual Installation
1. Make sure BepInEx is installed in your Ultimate Chicken Horse directory
2. Download the mod DLL file
3. Place `KnockbackMod.dll` in `BepInEx/plugins/` folder
4. Launch the game

## Configuration

After first launch, a config file will be created at `BepInEx/config/com.uchknockback.mod.cfg`

Available settings:
- **EnableMod**: Enable/disable the mod (default: true)
- **Force**: Knockback force strength (default: 15.0)
- **Radius**: Knockback effect radius (default: 3.0)
- **KnockbackKey**: Key to trigger knockback (default: Mouse0 - Left Click)

## Usage

1. Get close to other players (within the configured radius)
2. Left-click to apply knockback force
3. Players within range will be pushed away from your position

## Compatibility

- Works with Ultimate Chicken Horse
- Compatible with r2modman and other mod managers
- Requires BepInEx 5.4.21 or later

## Troubleshooting

If the mod isn't working:
1. Check that BepInEx is properly installed
2. Ensure the mod DLL is in the correct plugins folder
3. Check the BepInEx console/log for any error messages
4. Try adjusting the configuration settings

## Development

Built with:
- BepInEx 5.x
- Harmony for runtime patching
- .NET Framework 4.7.2

## License

This mod is provided as-is for the Ultimate Chicken Horse community.
