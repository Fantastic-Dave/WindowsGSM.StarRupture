# WindowsGSM.StarRupture

WindowsGSM plugin for installing and running the StarRupture Dedicated Server via SteamCMD.

## Requirements
- WindowsGSM 1.25+ (or newer).
- SteamCMD reachable on the host.
- Visual C++ Redistributables (x64) installed on the host.

## Installation
1) Copy `StarRupture.cs` into your WindowsGSM `plugins` folder.
2) Restart WindowsGSM; the plugin appears as `StarRupture Dedicated Server`.
3) Add a new server with this plugin and click Install (Steam AppID `3809400`).

## Default Server Settings
- Game port: 7777
- Query port: 27015
- Port increment spacing: 2
- Executable: `StarRupture\Binaries\Win64\StarRuptureServerEOS-Win64-Shipping.exe`

## Launch Parameters
The plugin passes common flags automatically: `-Log`, `-ServerName`, `-MULTIHOME`, `-Port`, `-QueryPort`, plus any custom parameters you enter. Use `-`-prefixed flags (e.g. `-NoCrashDialog`) or UE-style `?` options if supported by the game.

## Configuration File (DSSettings.txt)
The plugin **automatically generates** `DSSettings.txt` during server startup using WindowsGSM inputs. No manual file creation needed!

### Automatic Values
- **SessionName**: Uses your WindowsGSM Server Name; falls back to `StarRupture-<ServerID>` if blank.
- **SaveGameInterval**: 300 seconds (5 minutes) by default.
- **StartNewGame**: `true` by default.
- **LoadSavedGame**: `false` by default.
- **SaveGameName**: `AutoSave0.sav` by default.

### Override via ServerParam
To customize DSSettings at launch, add parameters to the **Additional Arguments** field using `Key=Value` format:

```
SessionName=MyWorld SaveGameInterval=600 StartNewGame=false LoadSavedGame=true
```

Supported override keys (case-insensitive):
- `SessionName` — Custom world folder/session name
- `SaveGameInterval` — Autosave frequency in seconds
- `StartNewGame` — `true` or `false` to create new world
- `LoadSavedGame` — `true` or `false` to load existing save

If both `StartNewGame=false` and `LoadSavedGame=true` are set, `StartNewGame` is automatically set to `false`.

### Example Generated File
```json
{
  "SessionName": "MyServer",
  "SaveGameInterval": "300",
  "StartNewGame": "true",
  "LoadSavedGame": "false",
  "SaveGameName": "AutoSave0.sav"
}
```

The file is always written to the server root next to `StarRuptureServerEOS.exe`.

## Tips
- If the server fails to start, confirm ports 7777/UDP and 27015/UDP are open on the firewall and router.
- Use WindowsGSM embedded console to watch logs (`-Log` is enabled by default).
- For updates, run the plugin's Update action (validate to force file checks).