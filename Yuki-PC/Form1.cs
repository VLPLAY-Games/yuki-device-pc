using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            "volume_up", "volume_down", "volume_mute",
            "open_folder", "open_explorer", "open_notepad", "open_calculator"
        };

        public Form1()
        {
            InitializeComponent();
            InitializeClient();
            InitializeCapabilitiesList();

            NativeMethods.HideConsoleWindow();

            string deviceId = $"pc-{Environment.MachineName.ToLowerInvariant()}";
            textBoxDeviceId.Text = deviceId;
            labelDeviceId.Text = deviceId;

            SetupTrayIcon();

            Logger.OnLog = (msg, level) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => AddLog(msg, level)));
                else
                    AddLog(msg, level);
            };

            Logger.Info("=== Yuki PC started ===");
            Logger.Info($"Device ID: {deviceId}");
            Logger.Info($"OS: {Environment.OSVersion}");
            Logger.Info($"Machine: {Environment.MachineName}");
            Logger.Info($"User: {Environment.UserName}");

            UpdateFormHeight();
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
            if (_client.Status == YukiClient.ConnectionStatus.Connected)
            {
                _isUserDisconnect = true;
                await _client.DisconnectAsync();
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