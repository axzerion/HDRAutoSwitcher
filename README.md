# HDR Auto Switcher

A small Windows-only WPF utility that automatically switches HDR on/off when launching configured games and restores the previous HDR state after the game exits.

Current features
- Add local or Steam games and mark whether they require HDR.
- Launch games (Steam or executable) from the UI.
- Automatically enable HDR before the game if required, and restore previous HDR state after the game exits.
- Detect installed Steam games (reads Steam library files and finds likely executables).
- Simple UI with list, search/filter, and per-game actions (launch/edit/remove).

Requirements
- Windows 10/11
- .NET 8 runtime / SDK (for building)
- Visual Studio 2022/2023 or another .NET 8-capable IDE

How to build / run
1. Open the solution (`HDRAutoSwitcher.slnx`) in Visual Studio or use the dotnet CLI.
2. Build the solution (Debug or Release).
3. Run the application. The app stores local game profiles in `games.json` in the application folder (AppContext.BaseDirectory).

Notes and WIP
- Windows-only (uses Windows APIs and Steam registry).
- `games.json` contains local user executable paths; do not commit it.
- The app currently writes diagnostic messages using Debug.WriteLine; consider integrating a logging framework for production.
- UI polishing and additional error logging are still in progress.

Privacy / credentials
- The project does not require secrets by default. Ensure your local `games.json` is not committed (it may contain absolute executable paths).
