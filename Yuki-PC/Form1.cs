using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Yuki_PC
{
    public partial class Form1 : Form
    {
        private YukiClient _client;
        private bool _isUserDisconnect = false;

        private bool _capabilitiesVisible = false;
        private bool _logsVisible = false;

        private readonly string[] _allCapabilities = new[]
        {
            "open_browser", "open_url", "shutdown", "restart", "sleep",
            "volume_up", "volume_down", "volume_mute", "volume_unmute", "lock", "set_volume",
            "open_folder", "open_explorer", "open_notepad", "open_calculator"
        };

        private readonly string _settingsFilePath = "settings.json";

        public Form1()
        {
            InitializeComponent();
            InitializeClient();
            InitializeCapabilitiesList();

            NativeMethods.HideConsoleWindow();

            LoadSettings();

            if (string.IsNullOrWhiteSpace(textBoxDeviceId.Text))
            {
                string deviceId = $"pc-{Environment.MachineName.ToLowerInvariant()}";
                textBoxDeviceId.Text = deviceId;
                labelDeviceId.Text = deviceId;
            }
            else
            {
                labelDeviceId.Text = textBoxDeviceId.Text;
            }

            SetupTrayIcon();

            Logger.OnLog = (msg, level) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => AddLog(msg, level)));
                else
                    AddLog(msg, level);
            };

            Logger.Info("=== Yuki PC started ===");
            Logger.Info($"Device ID: {labelDeviceId.Text}");
            Logger.Info($"OS: {Environment.OSVersion}");
            Logger.Info($"Machine: {Environment.MachineName}");
            Logger.Info($"User: {Environment.UserName}");

            UpdateFormHeight();

            textBoxAddress.TextChanged += (s, e) => SaveSettings();
            textBoxDeviceId.TextChanged += (s, e) =>
            {
                labelDeviceId.Text = textBoxDeviceId.Text;
                SaveSettings();
            };
            textBoxAuthToken.TextChanged += (s, e) => SaveSettings();
            checkedListBoxCapabilities.ItemCheck += (s, e) =>
            {
                BeginInvoke(new Action(SaveSettings));
            };
        }

        private void InitializeClient()
        {
            _client = new YukiClient();
            _client.OnStatusChanged += Client_OnStatusChanged;
            _client.OnLog += Client_OnLog;
            _client.OnDeviceIdUpdated += (id) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => labelDeviceId.Text = id));
                else
                    labelDeviceId.Text = id;
            };
            _client.OnTokenUpdated += (newToken) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        textBoxAuthToken.Text = newToken;
                        SaveSettings();
                        Logger.Success("Auth token updated automatically from server.");
                    }));
                }
                else
                {
                    textBoxAuthToken.Text = newToken;
                    SaveSettings();
                    Logger.Success("Auth token updated automatically from server.");
                }
            };
        }

        private void InitializeCapabilitiesList()
        {
            checkedListBoxCapabilities.Items.Clear();
            foreach (var cap in _allCapabilities)
            {
                bool isChecked = !(cap == "shutdown" || cap == "restart" || cap == "sleep");
                checkedListBoxCapabilities.Items.Add(cap, isChecked);
            }
        }

        private void LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
                return;

            try
            {
                string json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings == null) return;

                if (!string.IsNullOrEmpty(settings.ServerAddress))
                    textBoxAddress.Text = settings.ServerAddress;

                if (!string.IsNullOrEmpty(settings.DeviceId))
                    textBoxDeviceId.Text = settings.DeviceId;

                if (!string.IsNullOrEmpty(settings.AuthToken))
                    textBoxAuthToken.Text = settings.AuthToken;

                if (settings.EnabledCapabilities != null && settings.EnabledCapabilities.Length > 0)
                {
                    for (int i = 0; i < checkedListBoxCapabilities.Items.Count; i++)
                    {
                        string cap = checkedListBoxCapabilities.Items[i].ToString();
                        checkedListBoxCapabilities.SetItemChecked(i, settings.EnabledCapabilities.Contains(cap));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new AppSettings
                {
                    ServerAddress = textBoxAddress.Text.Trim(),
                    DeviceId = textBoxDeviceId.Text.Trim(),
                    AuthToken = textBoxAuthToken.Text.Trim(),
                    EnabledCapabilities = checkedListBoxCapabilities.CheckedItems.Cast<string>().ToArray()
                };
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save settings: {ex.Message}");
            }
        }

        private void Client_OnStatusChanged(object sender, YukiClient.ConnectionStatus status)
        {
            if (InvokeRequired)
                Invoke(new Action(() => UpdateUIForStatus(status)));
            else
                UpdateUIForStatus(status);
        }

        private void UpdateUIForStatus(YukiClient.ConnectionStatus status)
        {
            switch (status)
            {
                case YukiClient.ConnectionStatus.Disconnected:
                    labelStatusValue.Text = "Offline";
                    labelStatusValue.ForeColor = Color.Red;
                    buttonConnect.Text = "Connect";
                    buttonConnect.Enabled = true;
                    buttonOpenPanel.Enabled = false;
                    break;
                case YukiClient.ConnectionStatus.Connecting:
                    labelStatusValue.Text = "Connecting...";
                    labelStatusValue.ForeColor = Color.Yellow;
                    buttonConnect.Enabled = false;
                    break;
                case YukiClient.ConnectionStatus.Connected:
                    labelStatusValue.Text = "Connected";
                    labelStatusValue.ForeColor = Color.Green;
                    buttonConnect.Text = "Disconnect";
                    buttonConnect.Enabled = true;
                    buttonOpenPanel.Enabled = true;
                    _isUserDisconnect = false;
                    break;
                case YukiClient.ConnectionStatus.Handshaking:
                    labelStatusValue.Text = "Handshaking...";
                    labelStatusValue.ForeColor = Color.Yellow;
                    break;
                case YukiClient.ConnectionStatus.Reconnecting:
                    labelStatusValue.Text = "Reconnecting...";
                    labelStatusValue.ForeColor = Color.Orange;
                    buttonConnect.Text = "Disconnect";
                    buttonConnect.Enabled = true;
                    buttonOpenPanel.Enabled = false;
                    break;
            }
        }

        private void Client_OnLog(string message, Logger.LogLevel level)
        {
            if (InvokeRequired)
                Invoke(new Action(() => AddLog(message, level)));
            else
                AddLog(message, level);
        }

        private void AddLog(string message, Logger.LogLevel level)
        {
            Color color = level switch
            {
                Logger.LogLevel.INFO => Color.LightBlue,
                Logger.LogLevel.WARN => Color.Orange,
                Logger.LogLevel.ERROR => Color.Red,
                Logger.LogLevel.DEBUG => Color.Gray,
                Logger.LogLevel.SUCCESS => Color.LightGreen,
                _ => Color.White
            };

            if (textBoxLogs is RichTextBox rtb)
            {
                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;
                rtb.SelectionColor = color;
                rtb.AppendText(message + Environment.NewLine);
                rtb.SelectionColor = rtb.ForeColor;
                rtb.ScrollToCaret();
            }
            else
            {
                textBoxLogs.AppendText(message + Environment.NewLine);
            }
        }

        private void SetupTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, (s, e) => ShowWindow());
            trayMenu.Items.Add("Exit", null, (s, e) => ExitApp());

            trayIcon = new NotifyIcon()
            {
                Text = "Yuki PC",
                Icon = SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => ShowWindow();
            trayIcon.ShowBalloonTip(2000, "Yuki PC", "Application started in system tray", ToolTipIcon.Info);
        }

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            SaveSettings();

            if (_client.Status == YukiClient.ConnectionStatus.Connected ||
                _client.Status == YukiClient.ConnectionStatus.Reconnecting ||
                _client.Status == YukiClient.ConnectionStatus.Handshaking ||
                _client.Status == YukiClient.ConnectionStatus.Connecting)  // Добавили все состояния
            {
                _isUserDisconnect = true;

                // Если зависло в Handshaking - принудительно
                if (_client.Status == YukiClient.ConnectionStatus.Handshaking ||
                    _client.Status == YukiClient.ConnectionStatus.Connecting)
                {
                    _client.ForceDisconnect();
                    // Обновляем UI сразу
                    UpdateUIForStatus(YukiClient.ConnectionStatus.Disconnected);
                }
                else
                {
                    await _client.DisconnectAsync(userInitiated: true);
                }
            }
            else if (_client.Status == YukiClient.ConnectionStatus.Disconnected)
            {
                string serverAddress = textBoxAddress.Text.Trim();
                string deviceId = textBoxDeviceId.Text.Trim();
                string authToken = textBoxAuthToken.Text.Trim();

                if (string.IsNullOrEmpty(deviceId))
                {
                    Logger.Warning("Device ID cannot be empty");
                    MessageBox.Show("Please enter a Device ID.", "Invalid ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedCapabilities = checkedListBoxCapabilities.CheckedItems.Cast<string>().ToArray();
                _client.SetCapabilities(selectedCapabilities);
                _client.DeviceId = deviceId;
                _client.AuthToken = authToken;
                labelDeviceId.Text = deviceId;

                _isUserDisconnect = false;
                await _client.ConnectAsync(serverAddress);
            }
        }

        private void buttonOpenPanel_Click(object sender, EventArgs e)
        {
            if (_client.Status != YukiClient.ConnectionStatus.Connected)
            {
                MessageBox.Show("Not connected to server.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string httpUrl = textBoxAddress.Text
                .Replace("ws://", "http://")
                .Replace("wss://", "https://")
                .Replace(":8000", ":5000");

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = httpUrl,
                    UseShellExecute = true
                });
                Logger.Info($"Opened control panel: {httpUrl}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to open browser: {ex.Message}");
                MessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonOpenLogs_Click(object sender, EventArgs e)
        {
            string logFolder = Logger.GetLogFolder();
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = logFolder,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to open logs: {ex.Message}");
            }
        }

        private void textBoxAddress_TextChanged(object sender, EventArgs e) { }
        private void textBoxDeviceId_TextChanged(object sender, EventArgs e) { }

        private void checkBoxShowToken_CheckedChanged(object sender, EventArgs e)
        {
            textBoxAuthToken.UseSystemPasswordChar = !checkBoxShowToken.Checked;
        }

        private void ShowWindow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        }

        private void ExitApp()
        {
            SaveSettings();

            Logger.Info("Application exiting...");
            _client?.Dispose();
            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
                Hide();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                ExitApp();
            }
            base.OnFormClosing(e);
        }

        private void btnToggleCapabilities_Click(object sender, EventArgs e)
        {
            _capabilitiesVisible = !_capabilitiesVisible;
            groupBoxCapabilities.Visible = _capabilitiesVisible;
            btnToggleCapabilities.Text = _capabilitiesVisible ? "Hide features" : "Show features";
            UpdateFormHeight();
        }

        private void btnToggleLogs_Click(object sender, EventArgs e)
        {
            _logsVisible = !_logsVisible;
            textBoxLogs.Visible = _logsVisible;
            btnToggleLogs.Text = _logsVisible ? "Hide logs" : "Show logs";
            UpdateFormHeight();
        }

        private void UpdateFormHeight()
        {
            int buttonsBottom = btnToggleCapabilities.Bottom;

            int yPos = buttonsBottom + 10;

            if (_capabilitiesVisible)
            {
                groupBoxCapabilities.Top = yPos;
                yPos += groupBoxCapabilities.Height + 10;
            }
            if (_logsVisible)
            {
                textBoxLogs.Top = yPos;
                yPos += textBoxLogs.Height + 10;
            }

            int totalHeight = yPos + 20;
            if (totalHeight < buttonsBottom + 40) totalHeight = buttonsBottom + 40;

            this.ClientSize = new Size(this.ClientSize.Width, totalHeight);
        }

        private class AppSettings
        {
            public string ServerAddress { get; set; }
            public string DeviceId { get; set; }
            public string AuthToken { get; set; }
            public string[] EnabledCapabilities { get; set; }
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_HIDE = 0;

        public static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
                ShowWindow(handle, SW_HIDE);
        }
    }
}