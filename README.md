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
Place `DSSettings.txt` in the server root beside `StarRuptureServerEOS.exe`. JSON example:

```
{
  "SessionName": "SESSIONNAME",
  "SaveGameInterval": "300",
  "StartNewGame": "true",
  "LoadSavedGame": "false",
  "SaveGameName": "AutoSave0.sav"
}
```

### Options
- SessionName: Folder name for the world save; also shown as the session title.
- SaveGameInterval: Autosave frequency in seconds (e.g., 300 = 5 minutes).
- StartNewGame: `true` to generate a new world; `false` to load an existing one.
- LoadSavedGame: Opposite of StartNewGame; `true` loads an existing save.
- SaveGameName: File name of the save under `/StarRupture/Saved/SaveGames/<SessionName>/`.

## Tips
- If the server fails to start, confirm ports 7777/UDP and 27015/UDP are open on the firewall and router.
- Use WindowsGSM embedded console to watch logs (`-Log` is enabled by default).
- For updates, run the plugin's Update action (validate to force file checks).