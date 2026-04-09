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
            this.labelDeviceText = new System.Windows.Forms.Label();
            this.labelDeviceId = new System.Windows.Forms.Label();
            this.textBoxDeviceId = new System.Windows.Forms.TextBox();
            this.labelAuthToken = new System.Windows.Forms.Label();
            this.textBoxAuthToken = new System.Windows.Forms.TextBox();
            this.checkBoxShowToken = new System.Windows.Forms.CheckBox();
            this.btnToggleCapabilities = new System.Windows.Forms.Button();
            this.btnToggleLogs = new System.Windows.Forms.Button();
            this.buttonOpenLogs = new System.Windows.Forms.Button();
            this.groupBoxCapabilities = new System.Windows.Forms.GroupBox();
            this.checkedListBoxCapabilities = new System.Windows.Forms.CheckedListBox();
            this.textBoxLogs = new System.Windows.Forms.RichTextBox();
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.groupBoxServer.SuspendLayout();
            this.groupBoxCapabilities.SuspendLayout();
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
            this.buttonOpenPanel.Enabled = false;
            this.buttonOpenPanel.Location = new System.Drawing.Point(15, 180);
            this.buttonOpenPanel.Name = "buttonOpenPanel";
            this.buttonOpenPanel.Size = new System.Drawing.Size(200, 28);
            this.buttonOpenPanel.TabIndex = 5;
            this.buttonOpenPanel.Text = "Open Control Panel";
            this.buttonOpenPanel.Click += new System.EventHandler(this.buttonOpenPanel_Click);
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
            this.textBoxDeviceId.Location = new System.Drawing.Point(220, 93);
            this.textBoxDeviceId.Name = "textBoxDeviceId";
            this.textBoxDeviceId.Size = new System.Drawing.Size(150, 25);
            this.textBoxDeviceId.TabIndex = 9;
            this.textBoxDeviceId.TextChanged += new System.EventHandler(this.textBoxDeviceId_TextChanged);
            // 
            // labelAuthToken
            // 
            this.labelAuthToken.Location = new System.Drawing.Point(15, 135);
            this.labelAuthToken.Name = "labelAuthToken";
            this.labelAuthToken.Size = new System.Drawing.Size(69, 23);
            this.labelAuthToken.TabIndex = 10;
            this.labelAuthToken.Text = "Auth Token:";
            // 
            // textBoxAuthToken
            // 
            this.textBoxAuthToken.Location = new System.Drawing.Point(90, 133);
            this.textBoxAuthToken.Name = "textBoxAuthToken";
            this.textBoxAuthToken.Size = new System.Drawing.Size(280, 25);
            this.textBoxAuthToken.TabIndex = 11;
            this.textBoxAuthToken.UseSystemPasswordChar = true;
            // 
            // checkBoxShowToken
            // 
            this.checkBoxShowToken.Location = new System.Drawing.Point(380, 133);
            this.checkBoxShowToken.Name = "checkBoxShowToken";
            this.checkBoxShowToken.Size = new System.Drawing.Size(69, 20);
            this.checkBoxShowToken.TabIndex = 12;
            this.checkBoxShowToken.Text = "Show";
            this.checkBoxShowToken.CheckedChanged += new System.EventHandler(this.checkBoxShowToken_CheckedChanged);
            // 
            // btnToggleCapabilities
            // 
            this.btnToggleCapabilities.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnToggleCapabilities.Location = new System.Drawing.Point(26, 276);
            this.btnToggleCapabilities.Name = "btnToggleCapabilities";
            this.btnToggleCapabilities.Size = new System.Drawing.Size(128, 28);
            this.btnToggleCapabilities.TabIndex = 13;
            this.btnToggleCapabilities.Text = "Show features";
            this.btnToggleCapabilities.UseVisualStyleBackColor = true;
            this.btnToggleCapabilities.Click += new System.EventHandler(this.btnToggleCapabilities_Click);
            // 
            // btnToggleLogs
            // 
            this.btnToggleLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnToggleLogs.Location = new System.Drawing.Point(160, 276);
            this.btnToggleLogs.Name = "btnToggleLogs";
            this.btnToggleLogs.Size = new System.Drawing.Size(128, 28);
            this.btnToggleLogs.TabIndex = 14;
            this.btnToggleLogs.Text = "Show logs";
            this.btnToggleLogs.UseVisualStyleBackColor = true;
            this.btnToggleLogs.Click += new System.EventHandler(this.btnToggleLogs_Click);
            // 
            // buttonOpenLogs
            // 
            this.buttonOpenLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOpenLogs.Location = new System.Drawing.Point(294, 276);
            this.buttonOpenLogs.Name = "buttonOpenLogs";
            this.buttonOpenLogs.Size = new System.Drawing.Size(134, 28);
            this.buttonOpenLogs.TabIndex = 6;
            this.buttonOpenLogs.Text = "Open Logs Folder";
            this.buttonOpenLogs.Click += new System.EventHandler(this.buttonOpenLogs_Click);
            // 
            // groupBoxCapabilities
            // 
            this.groupBoxCapabilities.Controls.Add(this.checkedListBoxCapabilities);
            this.groupBoxCapabilities.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBoxCapabilities.Location = new System.Drawing.Point(20, 310);
            this.groupBoxCapabilities.Name = "groupBoxCapabilities";
            this.groupBoxCapabilities.Size = new System.Drawing.Size(520, 200);
            this.groupBoxCapabilities.TabIndex = 2;
            this.groupBoxCapabilities.TabStop = false;
            this.groupBoxCapabilities.Text = "Enabled Capabilities";
            this.groupBoxCapabilities.Visible = false;
            // 
            // checkedListBoxCapabilities
            // 
            this.checkedListBoxCapabilities.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(31)))), ((int)(((byte)(38)))));
            this.checkedListBoxCapabilities.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBoxCapabilities.CheckOnClick = true;
            this.checkedListBoxCapabilities.ForeColor = System.Drawing.Color.White;
            this.checkedListBoxCapabilities.FormattingEnabled = true;
            this.checkedListBoxCapabilities.Location = new System.Drawing.Point(15, 25);
            this.checkedListBoxCapabilities.Name = "checkedListBoxCapabilities";
            this.checkedListBoxCapabilities.Size = new System.Drawing.Size(490, 160);
            this.checkedListBoxCapabilities.TabIndex = 0;
            // 
            // textBoxLogs
            // 
            this.textBoxLogs.BackColor = System.Drawing.Color.Black;
            this.textBoxLogs.Font = new System.Drawing.Font("Consolas", 9F);
            this.textBoxLogs.ForeColor = System.Drawing.Color.White;
            this.textBoxLogs.Location = new System.Drawing.Point(20, 526);
            this.textBoxLogs.Name = "textBoxLogs";
            this.textBoxLogs.ReadOnly = true;
            this.textBoxLogs.Size = new System.Drawing.Size(520, 150);
            this.textBoxLogs.TabIndex = 1;
            this.textBoxLogs.Text = "";
            this.textBoxLogs.Visible = false;
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
            this.ClientSize = new System.Drawing.Size(560, 680);
            this.Controls.Add(this.btnToggleCapabilities);
            this.Controls.Add(this.btnToggleLogs);
            this.Controls.Add(this.groupBoxServer);
            this.Controls.Add(this.groupBoxCapabilities);
            this.Controls.Add(this.textBoxLogs);
            this.Controls.Add(this.buttonOpenLogs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Yuki PC";
            this.groupBoxServer.ResumeLayout(false);
            this.groupBoxServer.PerformLayout();
            this.groupBoxCapabilities.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox groupBoxCapabilities;
        private System.Windows.Forms.CheckedListBox checkedListBoxCapabilities;
        private System.Windows.Forms.RichTextBox textBoxLogs;
        private ContextMenuStrip trayMenu;
        private NotifyIcon trayIcon;
        private Button btnToggleCapabilities;
        private Button btnToggleLogs;
    }
}