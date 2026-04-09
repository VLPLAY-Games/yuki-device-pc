using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace Yuki_PC
{
    public partial class Form1 : Form
    {
        String server_ip = "ws://localhost:8000";
        String connected_server_ip = null;

        public Form1()
        {
            InitializeComponent();

            Logger.OnLog = (msg) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => AddLog(msg)));
                }
                else
                {
                    AddLog(msg);
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
        }

        void AddLog(string message)
        {
            textBoxLogs.AppendText(message + Environment.NewLine);
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            connected_server_ip = server_ip;

            Logger.Info($"Connected to {connected_server_ip}");

            labelStatusValue.Text = "Connected";
            labelStatusValue.ForeColor = Color.Green;
        }

        private void buttonOpenPanel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(connected_server_ip))
            {
                Logger.Warning("Not connected to server");
                return;
            }

            string httpUrl = connected_server_ip
                .Replace("ws://", "http://")
                .Replace("wss://", "https://");

            Logger.Info($"Opening control panel: {httpUrl}");

            Process.Start(new ProcessStartInfo
            {
                FileName = httpUrl,
                UseShellExecute = true
            });
        }

        private void buttonOpenLogs_Click(object sender, EventArgs e)
        {
            string logFolder = Logger.GetLogFolder();

            Logger.Info("Opening logs folder...");

            Process.Start(new ProcessStartInfo
            {
                FileName = logFolder,
                UseShellExecute = true
            });
        }

        private void textBoxAddress_TextChanged(object sender, EventArgs e)
        {
            server_ip = textBoxAddress.Text;
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void ExitApp()
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }

            base.OnFormClosing(e);
        }
    }
}