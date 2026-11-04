# ONLY FENCE

Keep Fence. Hide the rest.

A small client-side mod that lets you decide which traders appear in the Escape from Tarkov trading screen. Hide the original traders, traders added by mods, or both. **Fence stays visible.**

For **SPT 4.1.6**. Client only.

## What it does

- **Fence stays.** The trader list is filtered without removing Fence, even when both hiding options are enabled.
- **Hide the originals.** Turn off the standard EFT traders and leave Fence on the screen.
- **Hide modded traders too, if you like.** A separate option hides traders whose IDs are not in the mod's list of original EFT traders.
- **Change it in F12.** No file editing or server restart needed to change the visibility settings. If the trading screen is already open, reopen it if the cards do not refresh cleanly.

## Settings (F12)

Find **Only Fence Client** in the BepInEx configuration menu.

- **Hide default traders** — On by default. Hides original EFT traders except Fence.
- **Hide modded traders** — Off by default. Hides traders not recognized as original EFT traders by the mod.
- **Enable debug logging** — Off by default. Writes detailed UI diagnostics when either trader visibility setting changes.

Fence is always exempt from both hiding options.

## Installation

1. Download the release ZIP from [GitHub Releases](https://github.com/Klidevich/OnlyFence/releases).
2. Extract it into your SPT installation folder.
3. Start the game. Press **F12** to configure Only Fence.

The installed file should be at:

```text
BepInEx/plugins/OnlyFence/OnlyFenceClient.dll
```

## Known issue

Changing visibility while the trading screen is open can leave dark or empty trader card slots. **Close and reopen the trading screen** to refresh the layout.

## Troubleshooting

Enable **F12 > Only Fence Client > Enable debug logging**, then change either visibility option to trigger a diagnostic dump. Check `BepInEx/LogOutput.log` and include the relevant lines in your issue report. Review logs for personal information before sharing them.

## Build

Only Fence is written in C# with BepInEx and Harmony, targeting .NET Standard 2.1. Build against your own **SPT 4.1.6** installation:

```powershell
dotnet build Client/OnlyFenceClient.csproj -c Release -p:SPT_PATH="C:\Path\To\SPT"
```

The compiled DLL is written to `Client/bin/Release/netstandard2.1/OnlyFenceClient.dll`.

## Uninstall

Delete `BepInEx/plugins/OnlyFence` from your SPT installation.

## Compatibility

Built and tested for **SPT 4.1.6**. Compatibility with other SPT versions is not guaranteed.

**Author:** Klide, ChatGPT  
**Version:** 1.0.12
