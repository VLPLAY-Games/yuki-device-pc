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

            // ========== НОВЫЕ КОНТРОЛЫ ==========
            this.groupExtended = new System.Windows.Forms.GroupBox();
            this.labelSub = new System.Windows.Forms.Label();
            this.comboSubstatus = new System.Windows.Forms.ComboBox();
            this.btnUpdateStatus = new System.Windows.Forms.Button();

            this.groupD2D = new System.Windows.Forms.GroupBox();
            this.labelTarget = new System.Windows.Forms.Label();
            this.textTargetDevice = new System.Windows.Forms.TextBox();
            this.labelCmd = new System.Windows.Forms.Label();
            this.textCustomCommand = new System.Windows.Forms.TextBox();
            this.labelPayload = new System.Windows.Forms.Label();
            this.textPayload = new System.Windows.Forms.TextBox();
            this.btnSendToDevice = new System.Windows.Forms.Button();

            this.btnBroadcast = new System.Windows.Forms.Button();
            // ===================================

            this.groupBoxServer.SuspendLayout();
            this.groupBoxCapabilities.SuspendLayout();
            this.groupExtended.SuspendLayout();
            this.groupD2D.SuspendLayout();
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
            this.btnToggleCapabilities.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnToggleCapabilities.Location = new System.Drawing.Point(20, 276);
            this.btnToggleCapabilities.Name = "btnToggleCapabilities";
            this.btnToggleCapabilities.Size = new System.Drawing.Size(128, 28);
            this.btnToggleCapabilities.TabIndex = 13;
            this.btnToggleCapabilities.Text = "Show features";
            this.btnToggleCapabilities.Click += new System.EventHandler(this.btnToggleCapabilities_Click);
            // 
            // btnToggleLogs
            // 
            this.btnToggleLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnToggleLogs.Location = new System.Drawing.Point(154, 276);
            this.btnToggleLogs.Name = "btnToggleLogs";
            this.btnToggleLogs.Size = new System.Drawing.Size(128, 28);
            this.btnToggleLogs.TabIndex = 14;
            this.btnToggleLogs.Text = "Show logs";
            this.btnToggleLogs.Click += new System.EventHandler(this.btnToggleLogs_Click);
            // 
            // buttonOpenLogs
            // 
            this.buttonOpenLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.buttonOpenLogs.Location = new System.Drawing.Point(288, 276);
            this.buttonOpenLogs.Name = "buttonOpenLogs";
            this.buttonOpenLogs.Size = new System.Drawing.Size(134, 28);
            this.buttonOpenLogs.TabIndex = 6;
            this.buttonOpenLogs.Text = "Open Logs Folder";
            this.buttonOpenLogs.Click += new System.EventHandler(this.buttonOpenLogs_Click);
            // 
            // ========== EXTENDED STATUS GROUP ==========
            // 
            this.groupExtended.Text = "Extended Status";
            this.groupExtended.Size = new System.Drawing.Size(250, 100);
            this.groupExtended.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupExtended.TabIndex = 15;
            this.groupExtended.TabStop = false;
            // 
            this.labelSub.Text = "Substatus:";
            this.labelSub.Location = new System.Drawing.Point(10, 30);
            this.labelSub.Size = new System.Drawing.Size(70, 25);
            // 
            this.comboSubstatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSubstatus.Location = new System.Drawing.Point(90, 28);
            this.comboSubstatus.Size = new System.Drawing.Size(140, 25);
            this.comboSubstatus.Items.AddRange(new object[] {
                "idle", "working", "sleeping", "charging", "error", "updating", "maintenance"});
            this.comboSubstatus.SelectedIndex = 0;
            // 
            this.btnUpdateStatus.Text = "Update";
            this.btnUpdateStatus.Location = new System.Drawing.Point(90, 60);
            this.btnUpdateStatus.Size = new System.Drawing.Size(80, 25);
            this.btnUpdateStatus.Click += new System.EventHandler(this.btnUpdateStatus_Click);
            // 
            this.groupExtended.Controls.Add(this.labelSub);
            this.groupExtended.Controls.Add(this.comboSubstatus);
            this.groupExtended.Controls.Add(this.btnUpdateStatus);
            // 
            // ========== DEVICE TO DEVICE GROUP ==========
            // 
            this.groupD2D.Text = "Send to Device";
            this.groupD2D.Size = new System.Drawing.Size(250, 170);
            this.groupD2D.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupD2D.TabIndex = 16;
            this.groupD2D.TabStop = false;
            // 
            this.labelTarget.Text = "Target Device:";
            this.labelTarget.Location = new System.Drawing.Point(10, 30);
            this.labelTarget.Size = new System.Drawing.Size(90, 25);
            // 
            this.textTargetDevice.Location = new System.Drawing.Point(110, 28);
            this.textTargetDevice.Size = new System.Drawing.Size(120, 25);
            this.textTargetDevice.PlaceholderText = "device-id";
            // 
            this.labelCmd.Text = "Command:";
            this.labelCmd.Location = new System.Drawing.Point(10, 65);
            this.labelCmd.Size = new System.Drawing.Size(70, 25);
            // 
            this.textCustomCommand.Location = new System.Drawing.Point(90, 63);
            this.textCustomCommand.Size = new System.Drawing.Size(140, 25);
            this.textCustomCommand.PlaceholderText = "command";
            // 
            this.labelPayload.Text = "Payload (JSON):";
            this.labelPayload.Location = new System.Drawing.Point(10, 100);
            this.labelPayload.Size = new System.Drawing.Size(90, 25);
            // 
            this.textPayload.Location = new System.Drawing.Point(110, 98);
            this.textPayload.Size = new System.Drawing.Size(120, 25);
            this.textPayload.PlaceholderText = "{}";
            // 
            this.btnSendToDevice.Text = "Send to Device";
            this.btnSendToDevice.Location = new System.Drawing.Point(90, 135);
            this.btnSendToDevice.Size = new System.Drawing.Size(100, 28);
            this.btnSendToDevice.Enabled = false;
            this.btnSendToDevice.Click += new System.EventHandler(this.btnSendToDevice_Click);
            // 
            this.groupD2D.Controls.Add(this.labelTarget);
            this.groupD2D.Controls.Add(this.textTargetDevice);
            this.groupD2D.Controls.Add(this.labelCmd);
            this.groupD2D.Controls.Add(this.textCustomCommand);
            this.groupD2D.Controls.Add(this.labelPayload);
            this.groupD2D.Controls.Add(this.textPayload);
            this.groupD2D.Controls.Add(this.btnSendToDevice);
            // 
            // ========== BROADCAST BUTTON ==========
            // 
            this.btnBroadcast.Text = "Broadcast";
            this.btnBroadcast.Size = new System.Drawing.Size(80, 28);
            this.btnBroadcast.Enabled = false;
            this.btnBroadcast.BackColor = System.Drawing.Color.DarkOrange;
            this.btnBroadcast.ForeColor = System.Drawing.Color.White;
            this.btnBroadcast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBroadcast.TabIndex = 17;
            this.btnBroadcast.Click += new System.EventHandler(this.btnBroadcast_Click);
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
            this.checkedListBoxCapabilities.BackColor = System.Drawing.Color.FromArgb(26, 31, 38);
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
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(580, 700);
            this.Controls.Add(this.btnBroadcast);
            this.Controls.Add(this.groupD2D);
            this.Controls.Add(this.groupExtended);
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
            this.groupExtended.ResumeLayout(false);
            this.groupD2D.ResumeLayout(false);
            this.groupD2D.PerformLayout();
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
        private Button btnToggleCapabilities;
        private Button btnToggleLogs;

        // Новые контролы
        private GroupBox groupExtended;
        private Label labelSub;
        private ComboBox comboSubstatus;
        private Button btnUpdateStatus;

        private GroupBox groupD2D;
        private Label labelTarget;
        private TextBox textTargetDevice;
        private Label labelCmd;
        private TextBox textCustomCommand;
        private Label labelPayload;
        private TextBox textPayload;
        private Button btnSendToDevice;

        private Button btnBroadcast;
    }
}