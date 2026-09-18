using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Yuki_PC
{
    public static class CommandHandler
    {
        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

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
                        {
                            var url = "https://www.google.com";
                            if (payload.TryGetProperty("url", out var urlProp))
                                url = urlProp.GetString();

                            // Reject anything but http(s): UseShellExecute otherwise lets a remote string launch local exes, UNC paths, file:// URIs, etc.
                            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                            {
                                return (false, null, "Invalid or unsupported URL scheme");
                            }

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = uri.AbsoluteUri,
                                UseShellExecute = true
                            });

                            result = new { opened = true, url = uri.AbsoluteUri };
                            break;
                        }

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
                        if (!locked)
                            return (false, null, "Failed to lock workstation");
                        break;

                    case "set_volume":
                        if (!payload.TryGetProperty("level", out var levelProp))
                            return (false, null, "Missing 'level' parameter (0-100)");

                        int target = levelProp.GetInt32();
                        if (target < 0) target = 0;
                        if (target > 100) target = 100;

                        bool success = SetVolumeExact(target);
                        if (!success)
                            return (false, null, "Failed to set volume");

                        result = new { volume = target, unit = "percent" };
                        break;

                    case "volume_up":
                        {
                            var volume = AdjustVolume(+2);
                            if (volume == null)
                                return (false, null, "Failed to increase volume");

                            result = new { volume = volume, unit = "percent" };
                            break;
                        }

                    case "volume_down":
                        {
                            var volume = AdjustVolume(-2);
                            if (volume == null)
                                return (false, null, "Failed to decrease volume");

                            result = new { volume = volume, unit = "percent" };
                            break;
                        }

                    case "volume_mute":
                        if (!SetMute(true))
                            return (false, null, "Failed to mute volume");

                        result = new { muted = true };
                        break;

                    case "volume_unmute":
                        if (!SetMute(false))
                            return (false, null, "Failed to unmute volume");

                        result = new { muted = false };
                        break;

                    case "open_folder":
                        if (payload.TryGetProperty("path", out var folderProp))
                        {
                            var folder = folderProp.GetString();
                            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                                return (false, null, "Path does not exist or is not a directory");

                            var openFolderPsi = new ProcessStartInfo("explorer.exe");
                            openFolderPsi.ArgumentList.Add(folder);
                            Process.Start(openFolderPsi);
                            result = new { opened = folder };
                        }
                        else
                        {
                            return (false, null, "Missing 'path' parameter");
                        }
                        break;

                    case "open_explorer":
                        {
                            var explorerPath = "";
                            if (payload.TryGetProperty("path", out var expProp))
                                explorerPath = expProp.GetString();

                            if (string.IsNullOrEmpty(explorerPath))
                            {
                                Process.Start("explorer.exe");
                                result = new { opened = "This PC" };
                            }
                            else
                            {
                                if (!Directory.Exists(explorerPath))
                                    return (false, null, "Path does not exist or is not a directory");

                                var openExplorerPsi = new ProcessStartInfo("explorer.exe");
                                openExplorerPsi.ArgumentList.Add(explorerPath);
                                Process.Start(openExplorerPsi);
                                result = new { opened = explorerPath };
                            }
                            break;
                        }

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
                var endpoint = GetDefaultAudioEndpoint();
                if (endpoint == null)
                    return false;

                float scalar = targetPercent / 100f;
                scalar = Math.Max(0f, Math.Min(scalar, 1f));

                int hr = endpoint.SetMasterVolumeLevelScalar(scalar, Guid.Empty);
                return hr >= 0;
            }
            catch
            {
                return false;
            }
        }

        private static int? AdjustVolume(int deltaPercent)
        {
            try
            {
                var endpoint = GetDefaultAudioEndpoint();
                if (endpoint == null)
                    return null;

                int hr = endpoint.GetMasterVolumeLevelScalar(out float current);
                if (hr < 0)
                    return null;

                float newValue = current + (deltaPercent / 100f);
                newValue = Math.Max(0f, Math.Min(newValue, 1f));

                hr = endpoint.SetMasterVolumeLevelScalar(newValue, Guid.Empty);
                if (hr < 0)
                    return null;

                return (int)(newValue * 100);
            }
            catch
            {
                return null;
            }
        }

        private static bool SetMute(bool mute)
        {
            try
            {
                var endpoint = GetDefaultAudioEndpoint();
                if (endpoint == null)
                    return false;

                int hr = endpoint.SetMute(mute, Guid.Empty);
                return hr >= 0;
            }
            catch
            {
                return false;
            }
        }

        private static IAudioEndpointVolume GetDefaultAudioEndpoint()
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)(new MMDeviceEnumeratorComObject());

                int hr = enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out IMMDevice device);
                if (hr < 0 || device == null)
                    return null;

                Guid iid = typeof(IAudioEndpointVolume).GUID;
                hr = device.Activate(ref iid, 23, IntPtr.Zero, out object endpointObj);
                if (hr < 0 || endpointObj == null)
                    return null;

                return (IAudioEndpointVolume)endpointObj;
            }
            catch
            {
                return null;
            }
        }

        // ===== Core Audio COM interop =====

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumeratorComObject
        {
        }

        private enum EDataFlow
        {
            eRender = 0,
            eCapture = 1,
            eAll = 2
        }

        private enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2
        }

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            [PreserveSig]
            int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, out IntPtr ppDevices);

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

            [PreserveSig]
            int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);

            [PreserveSig]
            int RegisterEndpointNotificationCallback(IntPtr pClient);

            [PreserveSig]
            int UnregisterEndpointNotificationCallback(IntPtr pClient);
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            [PreserveSig]
            int Activate(
                ref Guid iid,
                uint dwClsCtx,
                IntPtr pActivationParams,
                [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        }

        [ComImport]
        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            [PreserveSig] int RegisterControlChangeNotify(IntPtr pNotify);
            [PreserveSig] int UnregisterControlChangeNotify(IntPtr pNotify);
            [PreserveSig] int GetChannelCount(out uint pnChannelCount);

            [PreserveSig] int SetMasterVolumeLevel(float fLevelDB, Guid pguidEventContext);
            [PreserveSig] int SetMasterVolumeLevelScalar(float fLevel, Guid pguidEventContext);
            [PreserveSig] int GetMasterVolumeLevel(out float pfLevelDB);
            [PreserveSig] int GetMasterVolumeLevelScalar(out float pfLevel);

            [PreserveSig] int SetChannelVolumeLevel(uint nChannel, float fLevelDB, Guid pguidEventContext);
            [PreserveSig] int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, Guid pguidEventContext);
            [PreserveSig] int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
            [PreserveSig] int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);

            [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, Guid pguidEventContext);
            [PreserveSig] int GetMute(out bool pbMute);

            [PreserveSig] int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
            [PreserveSig] int VolumeStepUp(Guid pguidEventContext);
            [PreserveSig] int VolumeStepDown(Guid pguidEventContext);
            [PreserveSig] int QueryHardwareSupport(out uint pdwHardwareSupportMask);
            [PreserveSig] int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
        }
    }
}