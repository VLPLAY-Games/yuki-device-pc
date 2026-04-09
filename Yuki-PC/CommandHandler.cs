using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Yuki_PC
{
    public static class CommandHandler
    {
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

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
                    case "open_browser":
                    case "open_url":
                        var url = "https://www.google.com";
                        if (payload.TryGetProperty("url", out var urlProp))
                            url = urlProp.GetString();
                        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                        result = new { opened = true, url };
                        break;

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

                    case "lock":
                        bool locked = LockWorkStation();
                        result = new { locked = locked };
                        if (!locked) return (false, null, "Failed to lock workstation");
                        break;

                    case "set_volume":
                        if (!payload.TryGetProperty("level", out var levelProp))
                            return (false, null, "Missing 'level' parameter (0-100)");
                        int target = levelProp.GetInt32();
                        if (target < 0) target = 0;
                        if (target > 100) target = 100;
                        bool success = SetVolumeExact(target);
                        if (!success) return (false, null, "Failed to set volume");
                        result = new { volume = target, unit = "percent" };
                        break;

                    case "volume_up":
                        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND,
                            Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_UP);
                        result = new { volume = "up" };
                        break;

                    case "volume_down":
                        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND,
                            Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
                        result = new { volume = "down" };
                        break;

                    case "volume_mute":
                        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND,
                            Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
                        result = new { muted = true };
                        break;

                    case "open_folder":
                        if (payload.TryGetProperty("path", out var folderProp))
                        {
                            var folder = folderProp.GetString();
                            Process.Start("explorer.exe", folder);
                            result = new { opened = folder };
                        }
                        else return (false, null, "Missing 'path' parameter");
                        break;

                    case "open_explorer":
                        var explorerPath = "";
                        if (payload.TryGetProperty("path", out var expProp))
                            explorerPath = expProp.GetString();
                        Process.Start("explorer.exe", string.IsNullOrEmpty(explorerPath) ? "" : explorerPath);
                        result = new { opened = string.IsNullOrEmpty(explorerPath) ? "This PC" : explorerPath };
                        break;

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

        private static bool SetVolumeExact(int targetPercent)
        {
            try
            {
                var handle = Process.GetCurrentProcess().MainWindowHandle;
                // Сброс до 0 (50 нажатий VolumeDown, т.к. шаг ~2%)
                for (int i = 0; i < 50; i++)
                {
                    SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
                    System.Threading.Thread.Sleep(5);
                }
                // Поднятие до targetPercent
                int steps = targetPercent / 2;
                for (int i = 0; i < steps; i++)
                {
                    SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_UP);
                    System.Threading.Thread.Sleep(5);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}