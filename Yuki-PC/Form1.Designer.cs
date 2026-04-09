using System.Windows.Forms;

namespace Yuki_PC
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
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
            this.textBoxDeviceId = new System.Windows.Forms.TextBox();
            this.labelAuthToken = new System.Windows.Forms.Label();
            this.textBoxAuthToken = new System.Windows.Forms.TextBox();
            this.checkBoxShowToken = new System.Windows.Forms.CheckBox();
            this.textBoxLogs = new System.Windows.Forms.RichTextBox();
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
            this.groupBoxServer.Controls.Add(this.textBoxDeviceId);
            this.groupBoxServer.Controls.Add(this.labelAuthToken);
            this.groupBoxServer.Controls.Add(this.textBoxAuthToken);
            this.groupBoxServer.Controls.Add(this.checkBoxShowToken);
            this.groupBoxServer.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBoxServer.Location = new System.Drawing.Point(20, 20);
            this.groupBoxServer.Name = "groupBoxServer";
            this.groupBoxServer.Size = new System.Drawing.Size(520, 250);
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
            this.buttonOpenPanel.Location = new System.Drawing.Point(15, 180);
            this.buttonOpenPanel.Name = "buttonOpenPanel";
            this.buttonOpenPanel.Size = new System.Drawing.Size(200, 32);
            this.buttonOpenPanel.TabIndex = 5;
            this.buttonOpenPanel.Text = "Open Control Panel";
            this.buttonOpenPanel.Click += new System.EventHandler(this.buttonOpenPanel_Click);
            // 
            // buttonOpenLogs
            // 
            this.buttonOpenLogs.Location = new System.Drawing.Point(230, 180);
            this.buttonOpenLogs.Name = "buttonOpenLogs";
            this.buttonOpenLogs.Size = new System.Drawing.Size(200, 32);
            this.buttonOpenLogs.TabIndex = 6;
            this.buttonOpenLogs.Text = "Open Logs Folder";
            this.buttonOpenLogs.Click += new System.EventHandler(this.buttonOpenLogs_Click);
            // 
            // labelDeviceText
            // 
            this.labelDeviceText.Location = new System.Drawing.Point(15, 100);
            this.labelDeviceText.Name = "labelDeviceText";
            this.labelDeviceText.Size = new System.Drawing.Size(69, 23);
            this.labelDeviceText.TabIndex = 7;
            this.labelDeviceText.Text = "Device ID:";
            // 
            // labelDeviceId
            // 
            this.labelDeviceId.Location = new System.Drawing.Point(90, 100);
            this.labelDeviceId.Name = "labelDeviceId";
            this.labelDeviceId.Size = new System.Drawing.Size(100, 23);
            this.labelDeviceId.TabIndex = 8;
            this.labelDeviceId.Text = "pc-1";
            // 
            // textBoxDeviceId
            // 
            this.textBoxDeviceId.Location = new System.Drawing.Point(220, 98);
            this.textBoxDeviceId.Name = "textBoxDeviceId";
            this.textBoxDeviceId.Size = new System.Drawing.Size(150, 25);
            this.textBoxDeviceId.TabIndex = 9;
            this.textBoxDeviceId.TextChanged += new System.EventHandler(this.textBoxDeviceId_TextChanged);
            // 
            // labelAuthToken
            // 
            this.labelAuthToken.Location = new System.Drawing.Point(15, 135);
            this.labelAuthToken.Name = "labelAuthToken";
            this.labelAuthToken.Size = new System.Drawing.Size(80, 23);
            this.labelAuthToken.TabIndex = 10;
            this.labelAuthToken.Text = "Auth Token:";
            // 
            // textBoxAuthToken
            // 
            this.textBoxAuthToken.Location = new System.Drawing.Point(100, 132);
            this.textBoxAuthToken.Name = "textBoxAuthToken";
            this.textBoxAuthToken.Size = new System.Drawing.Size(200, 25);
            this.textBoxAuthToken.TabIndex = 11;
            this.textBoxAuthToken.UseSystemPasswordChar = true;
            // 
            // checkBoxShowToken
            // 
            this.checkBoxShowToken.Location = new System.Drawing.Point(310, 135);
            this.checkBoxShowToken.Name = "checkBoxShowToken";
            this.checkBoxShowToken.Size = new System.Drawing.Size(75, 20);
            this.checkBoxShowToken.TabIndex = 12;
            this.checkBoxShowToken.Text = "Show";
            this.checkBoxShowToken.CheckedChanged += new System.EventHandler(this.checkBoxShowToken_CheckedChanged);
            // 
            // textBoxLogs
            // 
            this.textBoxLogs.BackColor = System.Drawing.Color.Black;
            this.textBoxLogs.Font = new System.Drawing.Font("Consolas", 9F);
            this.textBoxLogs.ForeColor = System.Drawing.Color.White;
            this.textBoxLogs.Location = new System.Drawing.Point(20, 280);
            this.textBoxLogs.Name = "textBoxLogs";
            this.textBoxLogs.ReadOnly = true;
            this.textBoxLogs.Size = new System.Drawing.Size(520, 150);
            this.textBoxLogs.TabIndex = 1;
            this.textBoxLogs.Text = "";
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
            this.ClientSize = new System.Drawing.Size(560, 450);
            this.Controls.Add(this.groupBoxServer);
            this.Controls.Add(this.textBoxLogs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Yuki PC";
            this.groupBoxServer.ResumeLayout(false);
            this.groupBoxServer.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.TextBox textBoxDeviceId;
        private System.Windows.Forms.Label labelAuthToken;
        private System.Windows.Forms.TextBox textBoxAuthToken;
        private System.Windows.Forms.CheckBox checkBoxShowToken;
        private System.Windows.Forms.RichTextBox textBoxLogs;
        private ContextMenuStrip trayMenu;
        private NotifyIcon trayIcon;
    }
}