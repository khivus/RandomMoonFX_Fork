# RandomMoonFX Fork

A **Lethal Company** mod forked from **RandomMoonFX**, adding configurable weights to moon randomization. Give your favorite moons higher weights, or use the distribution list as a whitelist of destinations.

When you start a landing, the mod routes the ship to a random moon, waits for the routing animation, then starts the landing. The current moon is excluded from the next selection. Company destinations use separate last-day routing settings.

## Installation

1. Set up Lethal Company with BepInEx 5.
2. Copy `RandomMoonFX_Fork.dll` into `BepInEx/plugins/RandomMoonFX_Fork/` in your game installation or mod manager profile.
3. Launch the game once to generate `BepInEx/config/zigzag.randommoonfx.cfg`.
4. Close the game, edit the configuration, and relaunch.

Remove the original RandomMoonFX plugin before installing this fork: both use the plugin ID `zigzag.randommoonfx` and the same configuration filename.

## Custom moon chances

Weighted selection is **disabled by default**. Enable it in the generated configuration:

```ini
[Moons Distribution]

Activate Moons Distributions = true
Moons Distribution = Rend=1,Dine=1,Titan=1,Artifice=2
```

Each value is a relative weight, not a percentage. In this example, Artifice has twice the weight of each other listed moon. If all four are eligible, their shares are 20%, 20%, 20%, and 40%. Excluding the current moon or previously visited moons changes the final chances.

With distributions enabled, only listed moons can be selected for normal landings. With distributions disabled, the mod draws from the loaded levels and applies its active validity checks.

Use comma-separated `MoonName=Weight` entries with unique names and positive integer weights. Names are case-sensitive; omit the leading moon number (use `Rend`, not `85 Rend`). Custom moons can be listed if they are loaded by the game. The default distribution is:

```ini
Moons Distribution = Vow=1,Adamance=1,Rend=3,Dine=1,Titan=1,Artifice=5,Embrion=1
```

## Other settings

| Section | Setting | Default | Behavior |
| --- | --- | --- | --- |
| Randomization method | Activate Random Moons | `true` | Enables automatic moon selection. |
| Randomization method | Exclude previously visited | `false` | Attempts to avoid visited moons until its visit history resets. See limitations below. |
| Last day check | Quota check | `true` | Auto-routes to a Company destination on the last day while quota remains unmet. If disabled, auto-routing applies on every last day. |
| Last day check | Randomize last day | `false` | Allows a random landing on the last day if estimated available scrap and potential body value cannot meet quota. |
| Last day check | Company routing mode | `Random` | `Random` chooses a Company destination; `Select` uses the configured destination; `Manual` leaves routing to players on the last day. |
| Last day check | Selected Company | `Gordion` | Destination for `Select` mode, with Gordion as the fallback if the name is not found. |
| Misc | Activate Free Moons | `true` | Removes terminal moon-routing costs, even when randomization is disabled. |

Routing animation settings include optional Celestial_Tint and Chameleon integration, plus an explicit duration override. These mods are optional dependencies. The normal delay is 1.5 seconds; detected Chameleon uses 7.5 seconds and detected Celestial_Tint uses 4 seconds. Celestial_Tint takes precedence if both integrations are active. A nonnegative `Animation time override` takes precedence over both.

## Current limitations

- Distribution entries are parsed at startup without robust validation. Missing `=`, duplicate names, or invalid weights can cause errors. Only list moons that are actually loaded: weights for missing moons are still counted in the total and can break selection.
- Keep at least two eligible destinations with positive weights. The selection loop rejects the current moon and can keep retrying indefinitely if no alternative is available.
- Leave `Exclude previously visited` disabled when using a restricted distribution list. Its reset threshold uses the full level list, so the whitelist can run out of eligible destinations before history resets.
- Normal moon selection currently does not enforce `Moons Blacklist` or the LethalConstellations membership check; those checks are commented out in the source. The blacklist still affects Company destination discovery. Detected LethalConstellations compatibility also bypasses visit-history filtering.

## Building from source

The project targets `netstandard2.1` and references BepInEx 5 and `LethalCompany.GameLibs.Steam` version `73.0.0-ngd.0`. Install a .NET SDK capable of building this target, then run these PowerShell commands from the repository directory containing `RandomMoonFX_Fork.csproj`:

```powershell
dotnet restore .\RandomMoonFX_Fork.csproj --source https://api.nuget.org/v3/index.json --source https://nuget.bepinex.dev/v3/index.json
dotnet build .\RandomMoonFX_Fork.csproj --configuration Release --no-restore
```

The plugin DLL is written to `bin/Release/netstandard2.1/RandomMoonFX_Fork.dll`. Copy that DLL into the plugin directory described above.

## Credits

Based on RandomMoonFX. Credit for the original mod belongs to its original authors; this fork adds configurable moon distribution weights.
