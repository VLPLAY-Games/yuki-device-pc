# Yuki PC (Windows)

A WinForms remote-control client for Windows, connecting to `yuki-core` and executing commands the
server (or other devices) sends it - lock the screen, open a URL, adjust volume, and so on. Runs in
the system tray; closing the window hides it rather than exiting.

## Requirements

.NET 9 (`net9.0-windows`), Windows. Open `Yuki-PC.sln` in Visual Studio (or `dotnet build`) and run.

## Configuration

Everything is configured from the GUI itself and persisted to `settings.json` next to the
executable: server address, device id, auth token, which of the 15 command capabilities are
enabled (all except `shutdown`/`restart`/`sleep` by default), the chosen substatus, and window
position/theme. The auth token is encrypted at rest with Windows DPAPI
(`System.Security.Cryptography.ProtectedData`, per-user scope) before being written to
`settings.json` - it's decrypted back to plaintext only in memory.

- **Server Connection**: type either `ws://host:port` or `wss://host:port` (TLS works out of the
  box if `yuki-core` has it enabled - see [`yuki-core`](../yuki-core)'s README) and hit Connect.
- **Open Control Panel** opens `yuki-webui` in your browser, derived from the address field
  (`ws→http`/`wss→https`, port `8000→5000`).
- **Capabilities**: uncheck anything you don't want this PC to accept remotely - the app enforces
  this locally in addition to whatever `yuki-core` allows.

## Commands

`open_browser`/`open_url` (only accepts `http`/`https` URLs), `shutdown`, `restart`, `sleep`,
`lock`, `volume_up`/`volume_down`/`volume_mute`/`volume_unmute`/`set_volume` (Core Audio),
`open_folder`/`open_explorer` (validated existing directories only), `open_notepad`,
`open_calculator`.

## Protocol

Speaks Yuki Protocol `yuki/1.0` via the `libs/yuki-protocol` git submodule
([VLPLAY-Games/yuki-protocol](https://github.com/VLPLAY-Games/yuki-protocol), C# implementation) -
uses the legacy handshake (token sent directly in `hello`); `yuki-core` also supports a
challenge-response handshake that keeps the token off the wire entirely, which this client doesn't
use yet - see [`yuki-protocol`](../yuki-protocol) for details. A Linux equivalent with the same
feature set lives at
[`yuki-device-pc-linux`](../yuki-device-pc-linux).

## License

GNU General Public License v3.0 (GPLv3), same as the rest of the Yuki ecosystem - see
[yuki-system](https://github.com/VLPLAY-Games/yuki-system) for details.
