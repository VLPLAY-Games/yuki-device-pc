using System.Windows.Forms;

namespace Yuki_PC
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxServer = new System.Windows.Forms.GroupBox();
            this.labelStatusText = new System.Windows.Forms.Label();
            this.labelStatusValue = new System.Windows.Forms.Label();
            this.labelAddress = new System.Windows.Forms.Label();
            this.textBoxAddress = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonOpenPanel = new System.Windows.Forms.Button();
            this.buttonOpenLogs = new System.Windows.Forms.Button();
            this.labelDeviceText = new System.Windows.Forms.Label();
            this.labelDeviceId = new System.Windows.Forms.Label();
            this.textBoxLogs = new System.Windows.Forms.TextBox();
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.groupBoxServer.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxServer
            // 
            this.groupBoxServer.Controls.Add(this.labelStatusText);
            this.groupBoxServer.Controls.Add(this.labelStatusValue);
            this.groupBoxServer.Controls.Add(this.labelAddress);
            this.groupBoxServer.Controls.Add(this.textBoxAddress);
            this.groupBoxServer.Controls.Add(this.buttonConnect);
            this.groupBoxServer.Controls.Add(this.buttonOpenPanel);
            this.groupBoxServer.Controls.Add(this.buttonOpenLogs);
            this.groupBoxServer.Controls.Add(this.labelDeviceText);
            this.groupBoxServer.Controls.Add(this.labelDeviceId);
            this.groupBoxServer.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBoxServer.Location = new System.Drawing.Point(20, 20);
            this.groupBoxServer.Name = "groupBoxServer";
            this.groupBoxServer.Size = new System.Drawing.Size(520, 180);
            this.groupBoxServer.TabIndex = 0;
            this.groupBoxServer.TabStop = false;
            this.groupBoxServer.Text = "Server";
            // 
            // labelStatusText
            // 
            this.labelStatusText.Location = new System.Drawing.Point(15, 30);
            this.labelStatusText.Name = "labelStatusText";
            this.labelStatusText.Size = new System.Drawing.Size(55, 23);
            this.labelStatusText.TabIndex = 0;
            this.labelStatusText.Text = "Status:";
            // 
            // labelStatusValue
            // 
            this.labelStatusValue.ForeColor = System.Drawing.Color.Red;
            this.labelStatusValue.Location = new System.Drawing.Point(90, 30);
            this.labelStatusValue.Name = "labelStatusValue";
            this.labelStatusValue.Size = new System.Drawing.Size(100, 23);
            this.labelStatusValue.TabIndex = 1;
            this.labelStatusValue.Text = "Offline";
            // 
            // labelAddress
            // 
            this.labelAddress.Location = new System.Drawing.Point(15, 65);
            this.labelAddress.Name = "labelAddress";
            this.labelAddress.Size = new System.Drawing.Size(69, 23);
            this.labelAddress.TabIndex = 2;
            this.labelAddress.Text = "Address:";
            // 
            // textBoxAddress
            // 
            this.textBoxAddress.Location = new System.Drawing.Point(90, 62);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(280, 25);
            this.textBoxAddress.TabIndex = 3;
            this.textBoxAddress.Text = "ws://localhost:8000";
            this.textBoxAddress.TextChanged += new System.EventHandler(this.textBoxAddress_TextChanged);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(380, 60);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(100, 28);
            this.buttonConnect.TabIndex = 4;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonOpenPanel
            // 
            this.buttonOpenPanel.Location = new System.Drawing.Point(15, 100);
            this.buttonOpenPanel.Name = "buttonOpenPanel";
            this.buttonOpenPanel.Size = new System.Drawing.Size(200, 32);
            this.buttonOpenPanel.TabIndex = 5;
            this.buttonOpenPanel.Text = "Open Control Panel";
            this.buttonOpenPanel.Click += new System.EventHandler(this.buttonOpenPanel_Click);
            // 
            // buttonOpenLogs
            // 
            this.buttonOpenLogs.Location = new System.Drawing.Point(230, 100);
            this.buttonOpenLogs.Name = "buttonOpenLogs";
            this.buttonOpenLogs.Size = new System.Drawing.Size(200, 32);
            this.buttonOpenLogs.TabIndex = 6;
            this.buttonOpenLogs.Text = "Open Logs Folder";
            this.buttonOpenLogs.Click += new System.EventHandler(this.buttonOpenLogs_Click);
            // 
            // labelDeviceText
            // 
            this.labelDeviceText.Location = new System.Drawing.Point(15, 140);
            this.labelDeviceText.Name = "labelDeviceText";
            this.labelDeviceText.Size = new System.Drawing.Size(100, 23);
            this.labelDeviceText.TabIndex = 7;
            this.labelDeviceText.Text = "Device ID:";
            // 
            // labelDeviceId
            // 
            this.labelDeviceId.Location = new System.Drawing.Point(90, 140);
            this.labelDeviceId.Name = "labelDeviceId";
            this.labelDeviceId.Size = new System.Drawing.Size(100, 23);
            this.labelDeviceId.TabIndex = 8;
            this.labelDeviceId.Text = "pc-1";
            // 
            // textBoxLogs
            // 
            this.textBoxLogs.Font = new System.Drawing.Font("Consolas", 9F);
            this.textBoxLogs.Location = new System.Drawing.Point(20, 210);
            this.textBoxLogs.Multiline = true;
            this.textBoxLogs.Name = "textBoxLogs";
            this.textBoxLogs.ReadOnly = true;
            this.textBoxLogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLogs.Size = new System.Drawing.Size(520, 150);
            this.textBoxLogs.TabIndex = 1;
            // 
            // trayMenu
            // 
            this.trayMenu.Name = "contextMenuStrip1";
            this.trayMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // trayIcon
            // 
            this.trayIcon.Text = "Yuki PC";
            this.trayIcon.Visible = true;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(560, 380);
            this.Controls.Add(this.groupBoxServer);
            this.Controls.Add(this.textBoxLogs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Yuki Device PC";
            this.groupBoxServer.ResumeLayout(false);
            this.groupBoxServer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxServer;
        private System.Windows.Forms.Label labelStatusText;
        private System.Windows.Forms.Label labelStatusValue;
        private System.Windows.Forms.Label labelAddress;
        private System.Windows.Forms.TextBox textBoxAddress;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonOpenPanel;
        private System.Windows.Forms.Button buttonOpenLogs;
        private System.Windows.Forms.Label labelDeviceText;
        private System.Windows.Forms.Label labelDeviceId;
        private System.Windows.Forms.TextBox textBoxLogs;
        private ContextMenuStrip trayMenu;
        private NotifyIcon trayIcon;
    }
}