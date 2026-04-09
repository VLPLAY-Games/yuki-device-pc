using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Yuki_PC
{
    public partial class Form1 : Form
    {
        String server_ip = "ws://localhost:8000";
        String connected_server_ip = null;

        // Import for hiding console window
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public Form1()
        {
            InitializeComponent();

            // Hide console window if it exists
            var handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, SW_HIDE);
            }

            Logger.OnLog = (msg, level) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => AddLog(msg, level)));
                }
                else
                {
                    AddLog(msg, level);
                }
            };

            textBoxAddress.Text = server_ip;

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

            // Set up RichTextBox for colored logs
            SetupLogTextBox();

            // Log application startup
            Logger.Info("=== Application Form1 initialized ===");
            Logger.Info($"OS Version: {Environment.OSVersion}");
            Logger.Info($"Machine Name: {Environment.MachineName}");
            Logger.Info($"User Name: {Environment.UserName}");
            Logger.Info($"Default Server: {server_ip}");

            // Show startup balloon tip
            trayIcon.ShowBalloonTip(2000, "Yuki PC", "Application started in system tray", ToolTipIcon.Info);
        }

        private void SetupLogTextBox()
        {
            // Remove the default TextBox and replace with RichTextBox if needed
            // This assumes textBoxLogs is already a RichTextBox in the designer
            // If it's a TextBox, you need to change it in Form1.Designer.cs
        }

        void AddLog(string message, Logger.LogLevel level)
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

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Logger.Debug($"Connect button clicked. Target server: {server_ip}");

            if (string.IsNullOrEmpty(server_ip))
            {
                Logger.Warning("Cannot connect: Server address is empty");
                labelStatusValue.Text = "Error: Empty address";
                labelStatusValue.ForeColor = Color.Red;
                return;
            }

            if (server_ip == connected_server_ip)
            {
                Logger.Info($"Already connected to {connected_server_ip}");
                return;
            }

            Logger.Info($"Attempting to connect to {server_ip}...");

            // Simulate connection process
            if (server_ip.StartsWith("ws://") || server_ip.StartsWith("wss://"))
            {
                connected_server_ip = server_ip;
                Logger.Success($"Successfully connected to {connected_server_ip}");

                labelStatusValue.Text = "Connected";
                labelStatusValue.ForeColor = Color.Green;

                // Enable control panel button
                buttonOpenPanel.Enabled = true;

                trayIcon.ShowBalloonTip(1000, "Yuki PC", $"Connected to {connected_server_ip}", ToolTipIcon.Info);
            }
            else
            {
                Logger.Error($"Invalid server address format: {server_ip}");
                Logger.Info("Server address must start with ws:// or wss://");

                labelStatusValue.Text = "Error: Invalid format";
                labelStatusValue.ForeColor = Color.Red;
            }
        }

        private void buttonOpenPanel_Click(object sender, EventArgs e)
        {
            Logger.Debug("Open Panel button clicked");

            if (string.IsNullOrEmpty(connected_server_ip))
            {
                Logger.Warning("Cannot open panel: Not connected to any server");
                MessageBox.Show("Please connect to a server first.", "Not Connected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string httpUrl = connected_server_ip
                .Replace("ws://", "http://")
                .Replace("wss://", "https://");

            Logger.Info($"Opening control panel in browser: {httpUrl}");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = httpUrl,
                    UseShellExecute = true
                });
                Logger.Success("Browser launched successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to open browser: {ex.Message}");
                MessageBox.Show($"Failed to open browser: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonOpenLogs_Click(object sender, EventArgs e)
        {
            Logger.Debug("Open Logs button clicked");

            string logFolder = Logger.GetLogFolder();
            string logFile = Logger.GetLogFile();

            Logger.Info($"Opening logs folder: {logFolder}");
            Logger.Debug($"Current log file: {System.IO.Path.GetFileName(logFile)}");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logFolder,
                    UseShellExecute = true
                });
                Logger.Success("Logs folder opened in Explorer");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to open logs folder: {ex.Message}");
                MessageBox.Show($"Failed to open logs folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxAddress_TextChanged(object sender, EventArgs e)
        {
            string oldServerIp = server_ip;
            server_ip = textBoxAddress.Text;

            if (oldServerIp != server_ip)
            {
                Logger.Debug($"Server address changed: '{oldServerIp}' -> '{server_ip}'");
            }
        }

        private void ShowWindow()
        {
            Logger.Debug("Showing main window from system tray");
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            Logger.Info("Main window restored");
        }

        private void ExitApp()
        {
            Logger.Info("=== Application exit requested by user ===");

            if (!string.IsNullOrEmpty(connected_server_ip))
            {
                Logger.Info($"Disconnecting from {connected_server_ip}");
            }

            Logger.Info($"Application uptime: {DateTime.Now - Process.GetCurrentProcess().StartTime}");
            Logger.Info("=== Application shutting down ===");

            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Logger.Debug("Form1.OnLoad() called");
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Logger.Debug("Form1.OnShown() called - Main form is visible");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            Logger.Debug($"Form resized. WindowState: {this.WindowState}");

            if (this.WindowState == FormWindowState.Minimized)
            {
                Logger.Info("Application minimized to system tray");
                this.Hide();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Logger.Debug($"Form closing. Reason: {e.CloseReason}");

            if (e.CloseReason == CloseReason.UserClosing)
            {
                Logger.Info("User closed window - minimizing to system tray");
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                Logger.Info($"Application closing with reason: {e.CloseReason}");
            }

            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Logger.Info("Form closed completely");
            base.OnFormClosed(e);
        }
    }
}