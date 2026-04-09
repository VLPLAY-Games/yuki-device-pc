using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Yuki_PC
{
    public static class CommandHandler
    {
        // WinAPI for volume control
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public static async Task<(bool success, object result, string error)> ExecuteAsync(
            string command, JsonElement payload)
        {
            try
            {
                object result = null;
                switch (command?.ToLowerInvariant())
                {
                    // --- Browser & URLs ---
                    case "open_browser":
                    case "open_url":
                        var url = "https://www.google.com";
                        if (payload.TryGetProperty("url", out var urlProp))
                            url = urlProp.GetString();
                        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                        result = new { opened = true, url };
                        break;

                    // --- System power ---
                    case "shutdown":
                        Process.Start("shutdown", "/s /t 5");
                        result = new { shutdown_initiated = true, delay_seconds = 5 };
                        break;

                    case "restart":
                        Process.Start("shutdown", "/r /t 5");
                        result = new { restart_initiated = true, delay_seconds = 5 };
                        break;

                    case "sleep":
                        Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
                        result = new { sleep_initiated = true };
                        break;

                    // --- Volume control ---
                    case "volume_up":
                        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND,
                            Process.GetCurrentProcess().MainWindowHandle,
                            (IntPtr)APPCOMMAND_VOLUME_UP);
                        result = new { volume = "up" };
                        break;

                    case "volume_down":
                        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND,
                            Process.GetCurrentProcess().MainWindowHandle,
                            (IntPtr)APPCOMMAND_VOLUME_DOWN);
                        result = new { volume = "down" };
                        break;

                    case "volume_mute":
                        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND,
                            Process.GetCurrentProcess().MainWindowHandle,
                            (IntPtr)APPCOMMAND_VOLUME_MUTE);
                        result = new { muted = true };
                        break;

                    // --- File Explorer & folders ---
                    case "open_folder":
                        if (payload.TryGetProperty("path", out var folderProp))
                        {
                            var folder = folderProp.GetString();
                            Process.Start("explorer.exe", folder);
                            result = new { opened = folder };
                        }
                        else
                        {
                            return (false, null, "Missing 'path' parameter");
                        }
                        break;

                    case "open_explorer":
                        // Open "This PC" if no path given
                        var explorerPath = "";
                        if (payload.TryGetProperty("path", out var expProp))
                            explorerPath = expProp.GetString();
                        Process.Start("explorer.exe", string.IsNullOrEmpty(explorerPath) ? "" : explorerPath);
                        result = new { opened = string.IsNullOrEmpty(explorerPath) ? "This PC" : explorerPath };
                        break;

                    // --- Applications ---
                    case "open_notepad":
                        Process.Start("notepad.exe");
                        result = new { opened = "notepad" };
                        break;

                    case "open_calculator":
                        Process.Start("calc.exe");
                        result = new { opened = "calculator" };
                        break;

                    default:
                        return (false, new { executed = false, reason = "unknown_command" }, null);
                }

                return (true, result, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}