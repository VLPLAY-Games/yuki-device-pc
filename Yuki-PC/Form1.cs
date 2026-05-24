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

        // New UI controls
        private ComboBox comboSubstatus;
        private TextBox textTargetDevice;
        private TextBox textCustomCommand;
        private RichTextBox textDeviceMessages;
        private Button btnSendToDevice;
        private Button btnBroadcast;

        public Form1()
        {
            InitializeComponent();
            InitializeClient();
            InitializeCapabilitiesList();
            InitializeNewControls();

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

        private void InitializeNewControls()
        {
            // Extended Status Group
            var groupExtended = new GroupBox
            {
                Text = "Extended Status",
                Location = new Point(20, 276),
                Size = new Size(250, 100),
                Font = new Font("Segoe UI", 10F)
            };

            var labelSub = new Label
            {
                Text = "Substatus:",
                Location = new Point(10, 30),
                Size = new Size(70, 25)
            };

            comboSubstatus = new ComboBox
            {
                Location = new Point(90, 28),
                Size = new Size(140, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboSubstatus.Items.AddRange(new[] { "idle", "working", "sleeping", "charging", "error", "updating", "maintenance" });
            comboSubstatus.SelectedIndex = 0;
            comboSubstatus.SelectedIndexChanged += (s, e) =>
            {
                _client?.SetExtendedStatus(comboSubstatus.SelectedItem.ToString());
            };

            var btnUpdateStatus = new Button
            {
                Text = "Update",
                Location = new Point(90, 60),
                Size = new Size(80, 25)
            };
            btnUpdateStatus.Click += (s, e) =>
            {
                _client?.SetExtendedStatus(comboSubstatus.SelectedItem.ToString());
                Logger.Info($"Extended status updated to: {comboSubstatus.SelectedItem}");
            };

            groupExtended.Controls.Add(labelSub);
            groupExtended.Controls.Add(comboSubstatus);
            groupExtended.Controls.Add(btnUpdateStatus);

            // Device-to-Device Group
            var groupD2D = new GroupBox
            {
                Text = "Send to Device",
                Location = new Point(290, 276),
                Size = new Size(250, 170),
                Font = new Font("Segoe UI", 10F)
            };

            var labelTarget = new Label
            {
                Text = "Target Device:",
                Location = new Point(10, 30),
                Size = new Size(90, 25)
            };

            textTargetDevice = new TextBox
            {
                Location = new Point(110, 28),
                Size = new Size(120, 25),
                PlaceholderText = "device-id"
            };

            var labelCmd = new Label
            {
                Text = "Command:",
                Location = new Point(10, 65),
                Size = new Size(70, 25)
            };

            textCustomCommand = new TextBox
            {
                Location = new Point(90, 63),
                Size = new Size(140, 25),
                PlaceholderText = "command"
            };

            var labelPayload = new Label
            {
                Text = "Payload (JSON):",
                Location = new Point(10, 100),
                Size = new Size(90, 25)
            };

            var textPayload = new TextBox
            {
                Location = new Point(110, 98),
                Size = new Size(120, 25),
                PlaceholderText = "{}"
            };

            btnSendToDevice = new Button
            {
                Text = "Send to Device",
                Location = new Point(90, 135),
                Size = new Size(100, 28),
                Enabled = false
            };
            btnSendToDevice.Click += async (s, e) =>
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
                AddDeviceMessage($"→ {textTargetDevice.Text}: {textCustomCommand.Text}", Color.LightGreen);
            };

            groupD2D.Controls.Add(labelTarget);
            groupD2D.Controls.Add(textTargetDevice);
            groupD2D.Controls.Add(labelCmd);
            groupD2D.Controls.Add(textCustomCommand);
            groupD2D.Controls.Add(labelPayload);
            groupD2D.Controls.Add(textPayload);
            groupD2D.Controls.Add(btnSendToDevice);

            // Broadcast Button
            btnBroadcast = new Button
            {
                Text = "Broadcast",
                Location = new Point(200, 276),
                Size = new Size(80, 28),
                Enabled = false,
                BackColor = Color.DarkOrange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBroadcast.Click += async (s, e) =>
            {
                if (_client.Status != YukiClient.ConnectionStatus.Connected)
                {
                    MessageBox.Show("Not connected to server.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var dialog = new Form
                {
                    Text = "Broadcast Command",
                    Size = new Size(400, 250),
                    StartPosition = FormStartPosition.CenterParent
                };

                var cmdBox = new TextBox { Location = new Point(10, 40), Size = new Size(360, 25), PlaceholderText = "Command" };
                var payloadBox = new TextBox { Location = new Point(10, 90), Size = new Size(360, 60), Multiline = true, PlaceholderText = "Payload (JSON)", Text = "{}" };
                var sendBtn = new Button { Text = "Broadcast", Location = new Point(150, 170), Size = new Size(100, 30), DialogResult = DialogResult.OK };
                var cancelBtn = new Button { Text = "Cancel", Location = new Point(260, 170), Size = new Size(100, 30), DialogResult = DialogResult.Cancel };

                dialog.Controls.Add(new Label { Text = "Command:", Location = new Point(10, 20), Size = new Size(100, 20) });
                dialog.Controls.Add(cmdBox);
                dialog.Controls.Add(new Label { Text = "Payload:", Location = new Point(10, 70), Size = new Size(100, 20) });
                dialog.Controls.Add(payloadBox);
                dialog.Controls.Add(sendBtn);
                dialog.Controls.Add(cancelBtn);

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(cmdBox.Text))
                {
                    object payload = null;
                    if (!string.IsNullOrEmpty(payloadBox.Text) && payloadBox.Text != "{}")
                    {
                        try
                        {
                            payload = JsonSerializer.Deserialize<object>(payloadBox.Text);
                        }
                        catch { }
                    }
                    await _client.BroadcastToDevicesAsync(cmdBox.Text, payload);
                    Logger.Info($"Broadcast command: {cmdBox.Text}");
                }
            };

            // Device Messages Group
            var groupMessages = new GroupBox
            {
                Text = "Messages from Other Devices",
                Location = new Point(20, 460),
                Size = new Size(520, 150),
                Font = new Font("Segoe UI", 10F)
            };

            textDeviceMessages = new RichTextBox
            {
                Location = new Point(10, 25),
                Size = new Size(500, 110),
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                ReadOnly = true,
                Font = new Font("Consolas", 9F)
            };

            groupMessages.Controls.Add(textDeviceMessages);

            // Add to form and adjust positions
            Controls.Add(groupExtended);
            Controls.Add(groupD2D);
            Controls.Add(btnBroadcast);
            Controls.Add(groupMessages);

            // Move existing controls
            groupBoxCapabilities.Location = new Point(20, 620);
            textBoxLogs.Location = new Point(20, 780);
            Height = 950;
        }

        private void AddDeviceMessage(string message, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddDeviceMessage(message, color)));
                return;
            }

            textDeviceMessages.SelectionStart = textDeviceMessages.TextLength;
            textDeviceMessages.SelectionLength = 0;
            textDeviceMessages.SelectionColor = color;
            textDeviceMessages.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            textDeviceMessages.SelectionColor = textDeviceMessages.ForeColor;
            textDeviceMessages.ScrollToCaret();
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
                AddDeviceMessage($"CMD from {fromDevice}: {command} -> {payload}", Color.Yellow);

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
                AddDeviceMessage($"BROADCAST: {command} -> {payload}", Color.Cyan);
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
                if (!string.IsNullOrEmpty(settings.Substatus))
                {
                    int idx = comboSubstatus.Items.IndexOf(settings.Substatus);
                    if (idx >= 0) comboSubstatus.SelectedIndex = idx;
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
                    Substatus = comboSubstatus?.SelectedItem?.ToString()
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
                    btnBroadcast.Enabled = false;
                    break;
                case YukiClient.ConnectionStatus.Connecting:
                    labelStatusValue.Text = "Connecting...";
                    labelStatusValue.ForeColor = Color.Yellow;
                    buttonConnect.Enabled = false;
                    btnSendToDevice.Enabled = false;
                    btnBroadcast.Enabled = false;
                    break;
                case YukiClient.ConnectionStatus.Connected:
                    labelStatusValue.Text = "Connected";
                    labelStatusValue.ForeColor = Color.Green;
                    buttonConnect.Text = "Disconnect";
                    buttonConnect.Enabled = true;
                    buttonOpenPanel.Enabled = true;
                    btnSendToDevice.Enabled = true;
                    btnBroadcast.Enabled = true;
                    _isUserDisconnect = false;
                    _client.StartMetricsReporting(60);
                    _client.StartPeriodicStatus(30);
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
                    btnBroadcast.Enabled = false;
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
            public string Substatus { get; set; }
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