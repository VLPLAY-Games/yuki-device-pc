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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            groupBoxServer = new GroupBox();
            labelStatusText = new Label();
            labelStatusValue = new Label();
            labelAddress = new Label();
            textBoxAddress = new TextBox();
            buttonConnect = new Button();
            buttonOpenPanel = new Button();
            labelDeviceText = new Label();
            labelDeviceId = new Label();
            textBoxDeviceId = new TextBox();
            labelAuthToken = new Label();
            textBoxAuthToken = new TextBox();
            checkBoxShowToken = new CheckBox();
            btnToggleCapabilities = new Button();
            btnToggleLogs = new Button();
            buttonOpenLogs = new Button();
            btnThemeToggle = new Button();
            groupBoxCapabilities = new GroupBox();
            checkedListBoxCapabilities = new CheckedListBox();
            textBoxLogs = new RichTextBox();
            groupExtended = new GroupBox();
            labelSub = new Label();
            comboSubstatus = new ComboBox();
            btnUpdateStatus = new Button();
            groupD2D = new GroupBox();
            labelTarget = new Label();
            textTargetDevice = new TextBox();
            labelCmd = new Label();
            textCustomCommand = new TextBox();
            labelPayload = new Label();
            textPayload = new TextBox();
            btnSendToDevice = new Button();
            groupBoxServer.SuspendLayout();
            groupBoxCapabilities.SuspendLayout();
            groupExtended.SuspendLayout();
            groupD2D.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxServer
            // 
            groupBoxServer.Controls.Add(labelStatusText);
            groupBoxServer.Controls.Add(labelStatusValue);
            groupBoxServer.Controls.Add(labelAddress);
            groupBoxServer.Controls.Add(textBoxAddress);
            groupBoxServer.Controls.Add(buttonConnect);
            groupBoxServer.Controls.Add(buttonOpenPanel);
            groupBoxServer.Controls.Add(labelDeviceText);
            groupBoxServer.Controls.Add(labelDeviceId);
            groupBoxServer.Controls.Add(textBoxDeviceId);
            groupBoxServer.Controls.Add(labelAuthToken);
            groupBoxServer.Controls.Add(textBoxAuthToken);
            groupBoxServer.Controls.Add(checkBoxShowToken);
            groupBoxServer.FlatStyle = FlatStyle.Flat;
            groupBoxServer.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            groupBoxServer.Location = new System.Drawing.Point(20, 20);
            groupBoxServer.Name = "groupBoxServer";
            groupBoxServer.Padding = new Padding(10);
            groupBoxServer.Size = new System.Drawing.Size(520, 250);
            groupBoxServer.TabIndex = 0;
            groupBoxServer.TabStop = false;
            groupBoxServer.Text = "Server Connection";
            // 
            // labelStatusText
            // 
            labelStatusText.Location = new System.Drawing.Point(15, 30);
            labelStatusText.Name = "labelStatusText";
            labelStatusText.Size = new System.Drawing.Size(55, 23);
            labelStatusText.TabIndex = 0;
            labelStatusText.Text = "Status:";
            // 
            // labelStatusValue
            // 
            labelStatusValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelStatusValue.ForeColor = System.Drawing.Color.Red;
            labelStatusValue.Location = new System.Drawing.Point(90, 30);
            labelStatusValue.Name = "labelStatusValue";
            labelStatusValue.Size = new System.Drawing.Size(100, 23);
            labelStatusValue.TabIndex = 1;
            labelStatusValue.Text = "Offline";
            // 
            // labelAddress
            // 
            labelAddress.Location = new System.Drawing.Point(15, 65);
            labelAddress.Name = "labelAddress";
            labelAddress.Size = new System.Drawing.Size(69, 23);
            labelAddress.TabIndex = 2;
            labelAddress.Text = "Address:";
            // 
            // textBoxAddress
            // 
            textBoxAddress.Font = new System.Drawing.Font("Segoe UI", 10F);
            textBoxAddress.Location = new System.Drawing.Point(90, 62);
            textBoxAddress.Name = "textBoxAddress";
            textBoxAddress.Size = new System.Drawing.Size(280, 25);
            textBoxAddress.TabIndex = 3;
            textBoxAddress.Text = "ws://localhost:8000";
            // 
            // buttonConnect
            // 
            buttonConnect.FlatStyle = FlatStyle.Flat;
            buttonConnect.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            buttonConnect.Location = new System.Drawing.Point(380, 60);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new System.Drawing.Size(110, 28);
            buttonConnect.TabIndex = 4;
            buttonConnect.Text = "Connect";
            buttonConnect.Click += buttonConnect_Click;
            // 
            // buttonOpenPanel
            // 
            buttonOpenPanel.Enabled = false;
            buttonOpenPanel.FlatStyle = FlatStyle.Flat;
            buttonOpenPanel.Location = new System.Drawing.Point(15, 180);
            buttonOpenPanel.Name = "buttonOpenPanel";
            buttonOpenPanel.Size = new System.Drawing.Size(200, 28);
            buttonOpenPanel.TabIndex = 5;
            buttonOpenPanel.Text = "Open Control Panel";
            buttonOpenPanel.Click += buttonOpenPanel_Click;
            // 
            // labelDeviceText
            // 
            labelDeviceText.Location = new System.Drawing.Point(15, 100);
            labelDeviceText.Name = "labelDeviceText";
            labelDeviceText.Size = new System.Drawing.Size(69, 23);
            labelDeviceText.TabIndex = 7;
            labelDeviceText.Text = "Device ID:";
            // 
            // labelDeviceId
            // 
            labelDeviceId.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            labelDeviceId.Location = new System.Drawing.Point(90, 100);
            labelDeviceId.Name = "labelDeviceId";
            labelDeviceId.Size = new System.Drawing.Size(120, 23);
            labelDeviceId.TabIndex = 8;
            labelDeviceId.Text = "pc-1";
            // 
            // textBoxDeviceId
            // 
            textBoxDeviceId.Location = new System.Drawing.Point(220, 93);
            textBoxDeviceId.Name = "textBoxDeviceId";
            textBoxDeviceId.Size = new System.Drawing.Size(150, 25);
            textBoxDeviceId.TabIndex = 9;
            // 
            // labelAuthToken
            // 
            labelAuthToken.Location = new System.Drawing.Point(15, 135);
            labelAuthToken.Name = "labelAuthToken";
            labelAuthToken.Size = new System.Drawing.Size(69, 23);
            labelAuthToken.TabIndex = 10;
            labelAuthToken.Text = "Auth Token:";
            // 
            // textBoxAuthToken
            // 
            textBoxAuthToken.Location = new System.Drawing.Point(90, 133);
            textBoxAuthToken.Name = "textBoxAuthToken";
            textBoxAuthToken.Size = new System.Drawing.Size(280, 25);
            textBoxAuthToken.TabIndex = 11;
            textBoxAuthToken.UseSystemPasswordChar = true;
            // 
            // checkBoxShowToken
            // 
            checkBoxShowToken.Location = new System.Drawing.Point(380, 133);
            checkBoxShowToken.Name = "checkBoxShowToken";
            checkBoxShowToken.Size = new System.Drawing.Size(69, 20);
            checkBoxShowToken.TabIndex = 12;
            checkBoxShowToken.Text = "Show";
            checkBoxShowToken.CheckedChanged += checkBoxShowToken_CheckedChanged;
            // 
            // btnToggleCapabilities
            // 
            btnToggleCapabilities.FlatStyle = FlatStyle.Flat;
            btnToggleCapabilities.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            btnToggleCapabilities.Location = new System.Drawing.Point(20, 276);
            btnToggleCapabilities.Name = "btnToggleCapabilities";
            btnToggleCapabilities.Size = new System.Drawing.Size(128, 32);
            btnToggleCapabilities.TabIndex = 13;
            btnToggleCapabilities.Text = "Show features";
            btnToggleCapabilities.Click += btnToggleCapabilities_Click;
            // 
            // btnToggleLogs
            // 
            btnToggleLogs.FlatStyle = FlatStyle.Flat;
            btnToggleLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            btnToggleLogs.Location = new System.Drawing.Point(154, 276);
            btnToggleLogs.Name = "btnToggleLogs";
            btnToggleLogs.Size = new System.Drawing.Size(128, 32);
            btnToggleLogs.TabIndex = 14;
            btnToggleLogs.Text = "Show logs";
            btnToggleLogs.Click += btnToggleLogs_Click;
            // 
            // buttonOpenLogs
            // 
            buttonOpenLogs.FlatStyle = FlatStyle.Flat;
            buttonOpenLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            buttonOpenLogs.Location = new System.Drawing.Point(288, 276);
            buttonOpenLogs.Name = "buttonOpenLogs";
            buttonOpenLogs.Size = new System.Drawing.Size(134, 32);
            buttonOpenLogs.TabIndex = 6;
            buttonOpenLogs.Text = "Open Logs Folder";
            buttonOpenLogs.Click += buttonOpenLogs_Click;
            // 
            // btnThemeToggle
            // 
            btnThemeToggle.FlatStyle = FlatStyle.Flat;
            btnThemeToggle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            btnThemeToggle.Location = new System.Drawing.Point(428, 276);
            btnThemeToggle.Name = "btnThemeToggle";
            btnThemeToggle.Size = new System.Drawing.Size(112, 32);
            btnThemeToggle.TabIndex = 17;
            btnThemeToggle.Text = "🌙 Dark";
            btnThemeToggle.Click += btnThemeToggle_Click;
            // 
            // groupBoxCapabilities
            // 
            groupBoxCapabilities.Controls.Add(checkedListBoxCapabilities);
            groupBoxCapabilities.FlatStyle = FlatStyle.Flat;
            groupBoxCapabilities.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            groupBoxCapabilities.Location = new System.Drawing.Point(20, 491);
            groupBoxCapabilities.Name = "groupBoxCapabilities";
            groupBoxCapabilities.Padding = new Padding(10);
            groupBoxCapabilities.Size = new System.Drawing.Size(520, 200);
            groupBoxCapabilities.TabIndex = 2;
            groupBoxCapabilities.TabStop = false;
            groupBoxCapabilities.Text = "Enabled Capabilities";
            groupBoxCapabilities.Visible = false;
            // 
            // checkedListBoxCapabilities
            // 
            checkedListBoxCapabilities.BorderStyle = BorderStyle.None;
            checkedListBoxCapabilities.CheckOnClick = true;
            checkedListBoxCapabilities.Font = new System.Drawing.Font("Segoe UI", 9F);
            checkedListBoxCapabilities.FormattingEnabled = true;
            checkedListBoxCapabilities.Location = new System.Drawing.Point(15, 30);
            checkedListBoxCapabilities.Name = "checkedListBoxCapabilities";
            checkedListBoxCapabilities.Size = new System.Drawing.Size(490, 144);
            checkedListBoxCapabilities.TabIndex = 0;
            // 
            // textBoxLogs
            // 
            textBoxLogs.Font = new System.Drawing.Font("Consolas", 9F);
            textBoxLogs.Location = new System.Drawing.Point(20, 706);
            textBoxLogs.Name = "textBoxLogs";
            textBoxLogs.ReadOnly = true;
            textBoxLogs.Size = new System.Drawing.Size(520, 150);
            textBoxLogs.TabIndex = 1;
            textBoxLogs.Text = "";
            textBoxLogs.Visible = false;
            // 
            // groupExtended
            // 
            groupExtended.Controls.Add(labelSub);
            groupExtended.Controls.Add(comboSubstatus);
            groupExtended.Controls.Add(btnUpdateStatus);
            groupExtended.FlatStyle = FlatStyle.Flat;
            groupExtended.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            groupExtended.Location = new System.Drawing.Point(20, 315);
            groupExtended.Name = "groupExtended";
            groupExtended.Padding = new Padding(10);
            groupExtended.Size = new System.Drawing.Size(250, 110);
            groupExtended.TabIndex = 15;
            groupExtended.TabStop = false;
            groupExtended.Text = "Extended Status";
            // 
            // labelSub
            // 
            labelSub.Location = new System.Drawing.Point(10, 30);
            labelSub.Name = "labelSub";
            labelSub.Size = new System.Drawing.Size(74, 25);
            labelSub.TabIndex = 0;
            labelSub.Text = "Substatus:";
            // 
            // comboSubstatus
            // 
            comboSubstatus.DropDownStyle = ComboBoxStyle.DropDownList;
            comboSubstatus.FlatStyle = FlatStyle.Flat;
            comboSubstatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            comboSubstatus.Items.AddRange(new object[] { "idle", "working", "sleeping", "charging", "error", "updating", "maintenance" });
            comboSubstatus.Location = new System.Drawing.Point(97, 28);
            comboSubstatus.Name = "comboSubstatus";
            comboSubstatus.Size = new System.Drawing.Size(140, 23);
            comboSubstatus.TabIndex = 1;
            // 
            // btnUpdateStatus
            // 
            btnUpdateStatus.FlatStyle = FlatStyle.Flat;
            btnUpdateStatus.Location = new System.Drawing.Point(90, 65);
            btnUpdateStatus.Name = "btnUpdateStatus";
            btnUpdateStatus.Size = new System.Drawing.Size(80, 28);
            btnUpdateStatus.TabIndex = 2;
            btnUpdateStatus.Text = "Update";
            btnUpdateStatus.Click += btnUpdateStatus_Click;
            // 
            // groupD2D
            // 
            groupD2D.Controls.Add(labelTarget);
            groupD2D.Controls.Add(textTargetDevice);
            groupD2D.Controls.Add(labelCmd);
            groupD2D.Controls.Add(textCustomCommand);
            groupD2D.Controls.Add(labelPayload);
            groupD2D.Controls.Add(textPayload);
            groupD2D.Controls.Add(btnSendToDevice);
            groupD2D.FlatStyle = FlatStyle.Flat;
            groupD2D.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            groupD2D.Location = new System.Drawing.Point(290, 315);
            groupD2D.Name = "groupD2D";
            groupD2D.Padding = new Padding(10);
            groupD2D.Size = new System.Drawing.Size(250, 180);
            groupD2D.TabIndex = 16;
            groupD2D.TabStop = false;
            groupD2D.Text = "Send to Device";
            // 
            // labelTarget
            // 
            labelTarget.Location = new System.Drawing.Point(10, 30);
            labelTarget.Name = "labelTarget";
            labelTarget.Size = new System.Drawing.Size(90, 25);
            labelTarget.TabIndex = 0;
            labelTarget.Text = "Target Device:";
            // 
            // textTargetDevice
            // 
            textTargetDevice.Font = new System.Drawing.Font("Segoe UI", 9F);
            textTargetDevice.Location = new System.Drawing.Point(110, 28);
            textTargetDevice.Name = "textTargetDevice";
            textTargetDevice.PlaceholderText = "device-id";
            textTargetDevice.Size = new System.Drawing.Size(120, 23);
            textTargetDevice.TabIndex = 1;
            // 
            // labelCmd
            // 
            labelCmd.Location = new System.Drawing.Point(10, 65);
            labelCmd.Name = "labelCmd";
            labelCmd.Size = new System.Drawing.Size(78, 25);
            labelCmd.TabIndex = 2;
            labelCmd.Text = "Command:";
            // 
            // textCustomCommand
            // 
            textCustomCommand.Font = new System.Drawing.Font("Segoe UI", 9F);
            textCustomCommand.Location = new System.Drawing.Point(110, 62);
            textCustomCommand.Name = "textCustomCommand";
            textCustomCommand.PlaceholderText = "command";
            textCustomCommand.Size = new System.Drawing.Size(120, 23);
            textCustomCommand.TabIndex = 3;
            // 
            // labelPayload
            // 
            labelPayload.Location = new System.Drawing.Point(10, 100);
            labelPayload.Name = "labelPayload";
            labelPayload.Size = new System.Drawing.Size(90, 25);
            labelPayload.TabIndex = 4;
            labelPayload.Text = "Payload (JSON):";
            // 
            // textPayload
            // 
            textPayload.Font = new System.Drawing.Font("Segoe UI", 9F);
            textPayload.Location = new System.Drawing.Point(110, 98);
            textPayload.Name = "textPayload";
            textPayload.PlaceholderText = "{}";
            textPayload.Size = new System.Drawing.Size(120, 23);
            textPayload.TabIndex = 5;
            // 
            // btnSendToDevice
            // 
            btnSendToDevice.Enabled = false;
            btnSendToDevice.FlatStyle = FlatStyle.Flat;
            btnSendToDevice.Location = new System.Drawing.Point(10, 136);
            btnSendToDevice.Name = "btnSendToDevice";
            btnSendToDevice.Size = new System.Drawing.Size(110, 28);
            btnSendToDevice.TabIndex = 6;
            btnSendToDevice.Text = "Send to Device";
            btnSendToDevice.Click += btnSendToDevice_Click;
            // 
            // Form1
            // 
            ClientSize = new System.Drawing.Size(564, 874);
            Controls.Add(groupD2D);
            Controls.Add(groupExtended);
            Controls.Add(btnThemeToggle);
            Controls.Add(btnToggleCapabilities);
            Controls.Add(btnToggleLogs);
            Controls.Add(groupBoxServer);
            Controls.Add(groupBoxCapabilities);
            Controls.Add(textBoxLogs);
            Controls.Add(buttonOpenLogs);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Yuki PC - Remote Control Client";
            groupBoxServer.ResumeLayout(false);
            groupBoxServer.PerformLayout();
            groupBoxCapabilities.ResumeLayout(false);
            groupExtended.ResumeLayout(false);
            groupD2D.ResumeLayout(false);
            groupD2D.PerformLayout();
            ResumeLayout(false);
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
        private Button btnThemeToggle;

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
    }
}