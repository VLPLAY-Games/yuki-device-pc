using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yuki_PC
{
    public partial class Form1 : Form
    {
        private YukiClient _client;
        private bool _isUserDisconnect = false;
        private bool _capabilitiesVisible = false;
        private bool _logsVisible = false;
        private bool _isDarkTheme = true;

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
            LoadSettingsAndApply();
            ApplyTheme();

            NativeMethods.HideConsoleWindow();

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

            UpdateFormLayout();

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

        private void ApplyTheme()
        {
            if (_isDarkTheme)
            {
                this.BackColor = Color.FromArgb(32, 32, 32);
                this.ForeColor = Color.FromArgb(220, 220, 220);

                foreach (Control ctrl in GetAllControls(this))
                {
                    ApplyThemeToControl(ctrl);
                }

                // Особые настройки для RichTextBox
                textBoxLogs.BackColor = Color.FromArgb(20, 20, 20);
                textBoxLogs.ForeColor = Color.FromArgb(200, 200, 200);
                textBoxLogs.BorderStyle = BorderStyle.None;

                checkedListBoxCapabilities.BackColor = Color.FromArgb(25, 25, 25);
                checkedListBoxCapabilities.ForeColor = Color.FromArgb(200, 200, 200);

                btnToggleCapabilities.BackColor = Color.FromArgb(60, 60, 60);
                btnToggleLogs.BackColor = Color.FromArgb(60, 60, 60);
                buttonOpenLogs.BackColor = Color.FromArgb(60, 60, 60);
                btnThemeToggle.BackColor = Color.FromArgb(60, 60, 60);
                btnUpdateStatus.BackColor = Color.FromArgb(60, 60, 60);
                btnSendToDevice.BackColor = Color.FromArgb(60, 60, 60);
                buttonConnect.BackColor = Color.FromArgb(60, 60, 60);
                buttonOpenPanel.BackColor = Color.FromArgb(60, 60, 60);
            }
            else
            {
                this.BackColor = Color.FromArgb(240, 240, 240);
                this.ForeColor = Color.FromArgb(40, 40, 40);

                foreach (Control ctrl in GetAllControls(this))
                {
                    ApplyThemeToControl(ctrl);
                }

                textBoxLogs.BackColor = Color.White;
                textBoxLogs.ForeColor = Color.Black;
                textBoxLogs.BorderStyle = BorderStyle.FixedSingle;

                checkedListBoxCapabilities.BackColor = Color.White;
                checkedListBoxCapabilities.ForeColor = Color.Black;

                btnToggleCapabilities.BackColor = Color.FromArgb(220, 220, 220);
                btnToggleLogs.BackColor = Color.FromArgb(220, 220, 220);
                buttonOpenLogs.BackColor = Color.FromArgb(220, 220, 220);
                btnThemeToggle.BackColor = Color.FromArgb(220, 220, 220);
                btnUpdateStatus.BackColor = Color.FromArgb(220, 220, 220);
                btnSendToDevice.BackColor = Color.FromArgb(220, 220, 220);
                buttonConnect.BackColor = Color.FromArgb(220, 220, 220);
                buttonOpenPanel.BackColor = Color.FromArgb(220, 220, 220);
            }
        }

        private void ApplyThemeToControl(Control ctrl)
        {
            if (ctrl is GroupBox groupBox)
            {
                groupBox.ForeColor = _isDarkTheme ? Color.FromArgb(200, 200, 200) : Color.FromArgb(40, 40, 40);
            }
            else if (ctrl is Label label)
            {
                label.ForeColor = _isDarkTheme ? Color.FromArgb(200, 200, 200) : Color.FromArgb(40, 40, 40);
            }
            else if (ctrl is TextBox textBox)
            {
                textBox.BackColor = _isDarkTheme ? Color.FromArgb(45, 45, 45) : Color.White;
                textBox.ForeColor = _isDarkTheme ? Color.FromArgb(220, 220, 220) : Color.Black;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (ctrl is ComboBox comboBox)
            {
                comboBox.BackColor = _isDarkTheme ? Color.FromArgb(45, 45, 45) : Color.White;
                comboBox.ForeColor = _isDarkTheme ? Color.FromArgb(220, 220, 220) : Color.Black;
                comboBox.FlatStyle = FlatStyle.Flat;
            }
            else if (ctrl is CheckBox checkBox)
            {
                checkBox.ForeColor = _isDarkTheme ? Color.FromArgb(200, 200, 200) : Color.FromArgb(40, 40, 40);
            }
            else if (ctrl is Button button)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.BackColor = _isDarkTheme ? Color.FromArgb(60, 60, 60) : Color.FromArgb(220, 220, 220);
                button.ForeColor = _isDarkTheme ? Color.White : Color.Black;
            }
        }

        private IEnumerable<Control> GetAllControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                yield return control;
                foreach (Control child in GetAllControls(control))
                {
                    yield return child;
                }
            }
        }

        private void LoadSettingsAndApply()
        {
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

            // Загрузка настройки темы
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        _isDarkTheme = settings.IsDarkTheme;
                        btnThemeToggle.Text = _isDarkTheme ? "🌙 Dark" : "☀️ Light";
                    }
                }
                catch { }
            }
        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            _client?.SetExtendedStatus(comboSubstatus.SelectedItem.ToString());
            Logger.Info($"Extended status updated to: {comboSubstatus.SelectedItem}");
        }

        private async void btnSendToDevice_Click(object sender, EventArgs e)
        {
            if (_client.Status != YukiClient.ConnectionStatus.Connected)
            {
                MessageBox.Show("Not connected to server.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(textTargetDevice.Text))
            {
                MessageBox.Show("Enter target device ID.", "Missing Target", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(textCustomCommand.Text))
            {
                MessageBox.Show("Enter command.", "Missing Command", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object payload = null;
            if (!string.IsNullOrEmpty(textPayload.Text) && textPayload.Text != "{}")
            {
                try
                {
                    payload = JsonSerializer.Deserialize<object>(textPayload.Text);
                }
                catch
                {
                    Logger.Error("Invalid JSON payload");
                    MessageBox.Show("Invalid JSON payload.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            await _client.SendToDeviceAsync(textTargetDevice.Text, textCustomCommand.Text, payload);
            Logger.Info($"Sent to {textTargetDevice.Text}: {textCustomCommand.Text}");
            Logger.Success($"→ {textTargetDevice.Text}: {textCustomCommand.Text}");
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
            _client.OnDeviceCommand += (fromDevice, command, payload) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => AddDeviceCommandToLog(fromDevice, command, payload)));
                }
                else
                {
                    AddDeviceCommandToLog(fromDevice, command, payload);
                }

                switch (command?.ToLowerInvariant())
                {
                    case "show_message":
                        if (payload is JsonElement el && el.TryGetProperty("message", out var msgProp))
                        {
                            MessageBox.Show(msgProp.GetString(), $"Message from {fromDevice}",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                    case "get_status":
                        _ = _client.SendToDeviceAsync(fromDevice, "status_response",
                            new { status = "online", substatus = comboSubstatus?.SelectedItem?.ToString() ?? "idle" });
                        break;
                }
            };
            _client.OnDeviceBroadcast += (command, payload) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => Logger.Info($"[BROADCAST] {command}")));
                }
                else
                {
                    Logger.Info($"[BROADCAST] {command}");
                }
            };
        }

        private void AddDeviceCommandToLog(string fromDevice, string command, object payload)
        {
            var rtb = textBoxLogs as RichTextBox;
            if (rtb != null)
            {
                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;
                rtb.SelectionColor = _isDarkTheme ? Color.Cyan : Color.Blue;
                rtb.AppendText($"[{DateTime.Now:HH:mm:ss}] ");
                rtb.SelectionColor = _isDarkTheme ? Color.Yellow : Color.Orange;
                rtb.AppendText($"[DEVICE→CMD] ");
                rtb.SelectionColor = _isDarkTheme ? Color.LightGreen : Color.Green;
                rtb.AppendText($"from {fromDevice}: {command}");
                if (payload != null && payload.ToString() != "{}")
                {
                    rtb.SelectionColor = _isDarkTheme ? Color.Gray : Color.DimGray;
                    rtb.AppendText($" {payload}");
                }
                rtb.AppendText(Environment.NewLine);
                rtb.SelectionColor = rtb.ForeColor;
                rtb.ScrollToCaret();
            }
            else
            {
                textBoxLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] [DEVICE→CMD] from {fromDevice}: {command}\r\n");
            }
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
                if (!string.IsNullOrEmpty(settings.Substatus))
                {
                    int idx = comboSubstatus.Items.IndexOf(settings.Substatus);
                    if (idx >= 0) comboSubstatus.SelectedIndex = idx;
                }
                if (settings.WindowBounds != null)
                {
                    this.Bounds = settings.WindowBounds;
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
                    EnabledCapabilities = checkedListBoxCapabilities.CheckedItems.Cast<string>().ToArray(),
                    Substatus = comboSubstatus?.SelectedItem?.ToString(),
                    IsDarkTheme = _isDarkTheme,
                    WindowBounds = this.WindowState == FormWindowState.Normal ? this.Bounds : this.RestoreBounds
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
                    btnSendToDevice.Enabled = false;
                    _client?.StopMetricsReporting();
                    _client?.StopPeriodicStatus();
                    break;
                case YukiClient.ConnectionStatus.Connecting:
                    labelStatusValue.Text = "Connecting...";
                    labelStatusValue.ForeColor = Color.Yellow;
                    buttonConnect.Enabled = false;
                    btnSendToDevice.Enabled = false;
                    break;
                case YukiClient.ConnectionStatus.Connected:
                    labelStatusValue.Text = "Connected";
                    labelStatusValue.ForeColor = Color.Green;
                    buttonConnect.Text = "Disconnect";
                    buttonConnect.Enabled = true;
                    buttonOpenPanel.Enabled = true;
                    btnSendToDevice.Enabled = true;
                    _isUserDisconnect = false;

                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        if (_client.Status == YukiClient.ConnectionStatus.Connected)
                        {
                            _client.StartMetricsReporting(60);
                            _client.StartPeriodicStatus(30);
                            _client.SetExtendedStatus(comboSubstatus.SelectedItem.ToString());
                        }
                    });
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
                    btnSendToDevice.Enabled = false;
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
                Logger.LogLevel.INFO => _isDarkTheme ? Color.LightBlue : Color.Blue,
                Logger.LogLevel.WARN => _isDarkTheme ? Color.Orange : Color.OrangeRed,
                Logger.LogLevel.ERROR => Color.Red,
                Logger.LogLevel.DEBUG => _isDarkTheme ? Color.Gray : Color.DimGray,
                Logger.LogLevel.SUCCESS => _isDarkTheme ? Color.LightGreen : Color.Green,
                _ => _isDarkTheme ? Color.White : Color.Black
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
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, (s, e) => ShowWindow());
            trayMenu.Items.Add("Exit", null, (s, e) => ExitApp());

            var trayIcon = new NotifyIcon()
            {
                Text = "Yuki PC",
                Icon = SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => ShowWindow();
            trayIcon.ShowBalloonTip(2000, "Yuki PC", "Application running in system tray", ToolTipIcon.Info);
        }

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            SaveSettings();

            if (_client.Status == YukiClient.ConnectionStatus.Connected ||
                _client.Status == YukiClient.ConnectionStatus.Reconnecting ||
                _client.Status == YukiClient.ConnectionStatus.Handshaking ||
                _client.Status == YukiClient.ConnectionStatus.Connecting)
            {
                _isUserDisconnect = true;

                if (_client.Status == YukiClient.ConnectionStatus.Handshaking ||
                    _client.Status == YukiClient.ConnectionStatus.Connecting)
                {
                    _client.ForceDisconnect();
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

        private void UpdateFormLayout()
        {
            int currentY = btnToggleCapabilities.Bottom + 15;
            if (currentY < 300) currentY = 315;

            groupExtended.Location = new Point(20, currentY);
            groupD2D.Location = new Point(290, currentY);

            currentY += groupExtended.Height + 15;

            if (_capabilitiesVisible)
            {
                groupBoxCapabilities.Location = new Point(20, currentY + 60);
                groupBoxCapabilities.Visible = true;
                currentY += groupBoxCapabilities.Height + 15;
            }
            else
            {
                groupBoxCapabilities.Visible = false;
            }

            if (_logsVisible)
            {
                textBoxLogs.Location = new Point(20, currentY + 60);
                textBoxLogs.Visible = true;
                currentY += textBoxLogs.Height + 15;
            }
            else
            {
                textBoxLogs.Visible = false;
            }

            int formHeight = currentY + 60;
            if (formHeight < 600) formHeight = 600;
            if (formHeight > 1000) formHeight = 1000;
            this.ClientSize = new Size(580, formHeight);

            this.Refresh();
        }

        private void btnToggleCapabilities_Click(object sender, EventArgs e)
        {
            _capabilitiesVisible = !_capabilitiesVisible;
            btnToggleCapabilities.Text = _capabilitiesVisible ? "Hide features" : "Show features";
            UpdateFormLayout();
        }

        private void btnToggleLogs_Click(object sender, EventArgs e)
        {
            _logsVisible = !_logsVisible;
            btnToggleLogs.Text = _logsVisible ? "Hide logs" : "Show logs";
            UpdateFormLayout();
        }

        private void btnThemeToggle_Click(object sender, EventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            btnThemeToggle.Text = _isDarkTheme ? "🌙 Dark" : "☀️ Light";
            ApplyTheme();
            SaveSettings();
        }

        private class AppSettings
        {
            public string ServerAddress { get; set; }
            public string DeviceId { get; set; }
            public string AuthToken { get; set; }
            public string[] EnabledCapabilities { get; set; }
            public string Substatus { get; set; }
            public bool IsDarkTheme { get; set; } = true;
            public Rectangle WindowBounds { get; set; }
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