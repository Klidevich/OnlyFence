# Only Fence

**A lightweight client-side trader visibility mod for SPT 4.1.6.**

Only Fence allows you to customize which traders appear in the Escape from Tarkov trading interface.

Hide vanilla traders, modded traders, or both — while keeping **Fence** visible at all times.

Why? Because now you can.

## Features

- Hide ALL vanilla EFT traders except Fence.
- Optionally hide ALL custom traders added by other mods.
- Configure trader visibility through the BepInEx F12 menu.
- Optional debug logging for troubleshooting.
- Fully client-side — no server mod required.
- No dependencies.

## Requirements

- **SPT 4.1.6**
- BepInEx and Harmony (included in the supported SPT environment)

No additional mods are required.

## Installation

1. Download the latest Release ZIP from GitHub Releases.
2. Extract the archive into your SPT installation directory.
3. Start the game.
4. Press **F12** to configure Only Fence.

The installed DLL should be located at:

`BepInEx/plugins/OnlyFence/OnlyFenceClient.dll`

## Configuration

Open the BepInEx F12 menu and find **Only Fence Client**.

| Setting | Default | Description |
|---|---|---|
| Hide default traders | ON | Hides vanilla EFT traders except Fence |
| Hide modded traders | OFF | Hides traders added by other mods |
| Enable debug logging | OFF | Enables detailed diagnostics that is saved in LogOutput.log |

**Fence always remains visible**, regardless of configuration.

## Known Issues

Changing trader visibility while the trading screen is open may leave empty or dark trader card slots.

If this happens, close and reopen the trading screen.

## Troubleshooting

If you encounter a problem:

1. Press F12.
2. Enable **Enable debug logging**.
3. Change one of the trader visibility settings to trigger diagnostic logging.
4. Reproduce the issue.
5. Close the game.
6. Locate `BepInEx/LogOutput.log`.
7. Attach the relevant log when reporting the issue.

Debug logging is disabled by default.

Please review logs for personal information before stating an issue.

## Building from Source

Only Fence is written in C# using BepInEx, Harmony and .NET Standard 2.1.

Build with:

```powershell
dotnet build Client/OnlyFenceClient.csproj -c Release -p:SPT_PATH="C:\Path\To\SPT"
```

The project may reference assemblies from a local SPT installation.
Update the paths in the .csproj file if necessary.

## Uninstallation

Delete:

`BepInEx/plugins/OnlyFence`

No server-side files need to be removed.

## Compatibility

Tested with **SPT 4.1.6**.

Compatibility with other SPT versions is not guaranteed.

**Author:** Klide  
**Version:** 1.0.12  
**SPT Version:** 4.1.6  
**License:** MIT

