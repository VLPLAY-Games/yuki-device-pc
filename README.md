# Yuki PC (Windows)

A WinForms remote-control client for Windows, connecting to `yuki-core` and executing commands the
server (or other devices) sends it - lock the screen, open a URL, adjust volume, and so on. Runs in
the system tray; closing the window hides it rather than exiting.

## Requirements

.NET 9 (`net9.0-windows`), Windows. Open `Yuki-PC.sln` in Visual Studio (or `dotnet build`) and run.

## Configuration

Everything is configured from the GUI itself and persisted to `settings.json` next to the
executable: server address, device id, auth token, which of the 14 command capabilities are
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

Speaks Yuki Protocol `yuki/1.0` via the vendored copy in `libs/yuki-protocol/csharp/` - see
[`yuki-protocol`](../yuki-protocol). A Linux equivalent with the same feature set lives at
[`yuki-device-pc-linux`](../yuki-device-pc-linux).
