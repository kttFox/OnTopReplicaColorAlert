namespace OnTopReplicaColorAlert.SidePanels {
    partial class ColorAlertPanel {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.groupColor = new System.Windows.Forms.GroupBox();
			this.checkEnabled = new System.Windows.Forms.CheckBox();
			this.checkAlertOnLoss = new System.Windows.Forms.CheckBox();
			this.checkIgnoreDark = new System.Windows.Forms.CheckBox();
			this.labelLossMiss = new System.Windows.Forms.Label();
			this.numLossMiss = new System.Windows.Forms.NumericUpDown();
			this.labelMinPixels = new System.Windows.Forms.Label();
			this.numMinPixels = new System.Windows.Forms.NumericUpDown();
			this.labelDetectedPixels = new System.Windows.Forms.Label();
			this.labelColorSelection = new System.Windows.Forms.Label();
			this.checkRed = new System.Windows.Forms.CheckBox();
			this.checkOrange = new System.Windows.Forms.CheckBox();
			this.checkGray = new System.Windows.Forms.CheckBox();
			this.checkCustomColor = new System.Windows.Forms.CheckBox();
			this.panelCustomColor = new System.Windows.Forms.Panel();
			this.btnPickCustomColor = new System.Windows.Forms.Button();
			this.btnSampleCursorColor = new System.Windows.Forms.Button();
			this.labelInterval = new System.Windows.Forms.Label();
			this.numInterval = new System.Windows.Forms.NumericUpDown();
			this.labelIntervalUnit = new System.Windows.Forms.Label();
			this.labelVolume = new System.Windows.Forms.Label();
			this.trackBarVolume = new System.Windows.Forms.TrackBar();
			this.labelSoundFile = new System.Windows.Forms.Label();
			this.comboSound = new System.Windows.Forms.ComboBox();
			this.btnTestAlarm = new System.Windows.Forms.Button();
			this.checkKeyPress = new System.Windows.Forms.CheckBox();
			this.textKeyCapture = new System.Windows.Forms.TextBox();
			this.btnClose = new System.Windows.Forms.Button();
			this.tooltipInfo = new System.Windows.Forms.ToolTip(this.components);
			this.groupColor.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numLossMiss)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numMinPixels)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).BeginInit();
			this.SuspendLayout();
			// 
			// groupColor
			// 
			this.groupColor.Controls.Add(this.checkEnabled);
			this.groupColor.Controls.Add(this.checkAlertOnLoss);
			this.groupColor.Controls.Add(this.checkIgnoreDark);
			this.groupColor.Controls.Add(this.labelLossMiss);
			this.groupColor.Controls.Add(this.numLossMiss);
			this.groupColor.Controls.Add(this.labelMinPixels);
			this.groupColor.Controls.Add(this.numMinPixels);
			this.groupColor.Controls.Add(this.labelDetectedPixels);
			this.groupColor.Controls.Add(this.labelColorSelection);
			this.groupColor.Controls.Add(this.checkRed);
			this.groupColor.Controls.Add(this.checkOrange);
			this.groupColor.Controls.Add(this.checkGray);
			this.groupColor.Controls.Add(this.checkCustomColor);
			this.groupColor.Controls.Add(this.panelCustomColor);
			this.groupColor.Controls.Add(this.btnPickCustomColor);
			this.groupColor.Controls.Add(this.btnSampleCursorColor);
			this.groupColor.Controls.Add(this.labelInterval);
			this.groupColor.Controls.Add(this.numInterval);
			this.groupColor.Controls.Add(this.labelIntervalUnit);
			this.groupColor.Controls.Add(this.labelVolume);
			this.groupColor.Controls.Add(this.trackBarVolume);
			this.groupColor.Controls.Add(this.labelSoundFile);
			this.groupColor.Controls.Add(this.comboSound);
			this.groupColor.Controls.Add(this.btnTestAlarm);
			this.groupColor.Controls.Add(this.checkKeyPress);
			this.groupColor.Controls.Add(this.textKeyCapture);
			this.groupColor.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupColor.Location = new System.Drawing.Point(10, 5);
			this.groupColor.Name = "groupColor";
			this.groupColor.Padding = new System.Windows.Forms.Padding(12);
			this.groupColor.Size = new System.Drawing.Size(388, 422);
			this.groupColor.TabIndex = 0;
			this.groupColor.TabStop = false;
			this.groupColor.Text = "Color Alert";
			// 
			// checkEnabled
			// 
			this.checkEnabled.AutoSize = true;
			this.checkEnabled.Location = new System.Drawing.Point(15, 25);
			this.checkEnabled.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkEnabled.Name = "checkEnabled";
			this.checkEnabled.Size = new System.Drawing.Size(146, 19);
			this.checkEnabled.TabIndex = 0;
			this.checkEnabled.Text = "Enable Color Detection";
			this.checkEnabled.UseVisualStyleBackColor = true;
			this.checkEnabled.CheckedChanged += new System.EventHandler(this.CheckEnabled_CheckedChanged);
			//
			// checkAlertOnLoss
			//
			this.checkAlertOnLoss.AutoSize = true;
			this.checkAlertOnLoss.Location = new System.Drawing.Point(185, 25);
			this.checkAlertOnLoss.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkAlertOnLoss.Name = "checkAlertOnLoss";
			this.checkAlertOnLoss.Size = new System.Drawing.Size(180, 19);
			this.checkAlertOnLoss.TabIndex = 1;
			this.checkAlertOnLoss.Text = "Alarm when color disappears";
			this.checkAlertOnLoss.UseVisualStyleBackColor = true;
			this.checkAlertOnLoss.CheckedChanged += new System.EventHandler(this.CheckAlertOnLoss_CheckedChanged);
			//
			// checkIgnoreDark
			//
			this.checkIgnoreDark.AutoSize = true;
			this.checkIgnoreDark.Checked = true;
			this.checkIgnoreDark.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkIgnoreDark.Location = new System.Drawing.Point(203, 47);
			this.checkIgnoreDark.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkIgnoreDark.Name = "checkIgnoreDark";
			this.checkIgnoreDark.Size = new System.Drawing.Size(130, 19);
			this.checkIgnoreDark.TabIndex = 2;
			this.checkIgnoreDark.Text = "Ignore dark frames";
			this.checkIgnoreDark.UseVisualStyleBackColor = true;
			this.checkIgnoreDark.CheckedChanged += new System.EventHandler(this.CheckIgnoreDark_CheckedChanged);
			//
			// labelLossMiss
			//
			this.labelLossMiss.AutoSize = true;
			this.labelLossMiss.Location = new System.Drawing.Point(185, 76);
			this.labelLossMiss.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelLossMiss.Name = "labelLossMiss";
			this.labelLossMiss.Size = new System.Drawing.Size(110, 15);
			this.labelLossMiss.TabIndex = 3;
			this.labelLossMiss.Text = "Consecutive misses:";
			//
			// numLossMiss
			//
			this.numLossMiss.Location = new System.Drawing.Point(310, 72);
			this.numLossMiss.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.numLossMiss.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numLossMiss.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numLossMiss.Name = "numLossMiss";
			this.numLossMiss.Size = new System.Drawing.Size(55, 23);
			this.numLossMiss.TabIndex = 4;
			this.numLossMiss.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			this.numLossMiss.ValueChanged += new System.EventHandler(this.NumLossMiss_ValueChanged);
			//
			// labelMinPixels
			//
			this.labelMinPixels.AutoSize = true;
			this.labelMinPixels.Location = new System.Drawing.Point(185, 108);
			this.labelMinPixels.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelMinPixels.Name = "labelMinPixels";
			this.labelMinPixels.Size = new System.Drawing.Size(110, 15);
			this.labelMinPixels.TabIndex = 5;
			this.labelMinPixels.Text = "Min pixels:";
			//
			// numMinPixels
			//
			this.numMinPixels.Location = new System.Drawing.Point(310, 104);
			this.numMinPixels.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.numMinPixels.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numMinPixels.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numMinPixels.Name = "numMinPixels";
			this.numMinPixels.Size = new System.Drawing.Size(55, 23);
			this.numMinPixels.TabIndex = 6;
			this.numMinPixels.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numMinPixels.ValueChanged += new System.EventHandler(this.NumMinPixels_ValueChanged);
			//
			// labelDetectedPixels
			//
			this.labelDetectedPixels.AutoEllipsis = true;
			this.labelDetectedPixels.ForeColor = System.Drawing.SystemColors.GrayText;
			this.labelDetectedPixels.Location = new System.Drawing.Point(15, 194);
			this.labelDetectedPixels.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelDetectedPixels.Name = "labelDetectedPixels";
			this.labelDetectedPixels.Size = new System.Drawing.Size(360, 17);
			this.labelDetectedPixels.TabIndex = 7;
			this.labelDetectedPixels.Text = "Detected: --";
			//
			// labelColorSelection
			// 
			this.labelColorSelection.AutoSize = true;
			this.labelColorSelection.Location = new System.Drawing.Point(15, 58);
			this.labelColorSelection.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelColorSelection.Name = "labelColorSelection";
			this.labelColorSelection.Size = new System.Drawing.Size(93, 15);
			this.labelColorSelection.TabIndex = 1;
			this.labelColorSelection.Text = "Colors to detect:";
			// 
			// checkRed
			// 
			this.checkRed.AutoSize = true;
			this.checkRed.Checked = true;
			this.checkRed.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkRed.ForeColor = System.Drawing.Color.Red;
			this.checkRed.Location = new System.Drawing.Point(35, 81);
			this.checkRed.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkRed.Name = "checkRed";
			this.checkRed.Size = new System.Drawing.Size(46, 19);
			this.checkRed.TabIndex = 2;
			this.checkRed.Text = "Red";
			this.checkRed.UseVisualStyleBackColor = true;
			this.checkRed.CheckedChanged += new System.EventHandler(this.CheckColor_CheckedChanged);
			// 
			// checkOrange
			// 
			this.checkOrange.AutoSize = true;
			this.checkOrange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.checkOrange.Location = new System.Drawing.Point(35, 107);
			this.checkOrange.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkOrange.Name = "checkOrange";
			this.checkOrange.Size = new System.Drawing.Size(65, 19);
			this.checkOrange.TabIndex = 3;
			this.checkOrange.Text = "Orange";
			this.checkOrange.UseVisualStyleBackColor = true;
			this.checkOrange.CheckedChanged += new System.EventHandler(this.CheckColor_CheckedChanged);
			// 
			// checkGray
			// 
			this.checkGray.AutoSize = true;
			this.checkGray.ForeColor = System.Drawing.Color.Gray;
			this.checkGray.Location = new System.Drawing.Point(35, 134);
			this.checkGray.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkGray.Name = "checkGray";
			this.checkGray.Size = new System.Drawing.Size(50, 19);
			this.checkGray.TabIndex = 4;
			this.checkGray.Text = "Gray";
			this.checkGray.UseVisualStyleBackColor = true;
			this.checkGray.CheckedChanged += new System.EventHandler(this.CheckColor_CheckedChanged);
			// 
			// checkCustomColor
			// 
			this.checkCustomColor.AutoSize = true;
			this.checkCustomColor.Location = new System.Drawing.Point(35, 161);
			this.checkCustomColor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkCustomColor.Name = "checkCustomColor";
			this.checkCustomColor.Size = new System.Drawing.Size(97, 19);
			this.checkCustomColor.TabIndex = 5;
			this.checkCustomColor.Text = "Custom Color";
			this.checkCustomColor.UseVisualStyleBackColor = true;
			this.checkCustomColor.CheckedChanged += new System.EventHandler(this.CheckCustomColor_CheckedChanged);
			// 
			// panelCustomColor
			// 
			this.panelCustomColor.BackColor = System.Drawing.Color.White;
			this.panelCustomColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelCustomColor.Location = new System.Drawing.Point(143, 159);
			this.panelCustomColor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.panelCustomColor.Name = "panelCustomColor";
			this.panelCustomColor.Size = new System.Drawing.Size(24, 24);
			this.panelCustomColor.TabIndex = 6;
			// 
			// btnPickCustomColor
			// 
			this.btnPickCustomColor.Location = new System.Drawing.Point(175, 156);
			this.btnPickCustomColor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btnPickCustomColor.Name = "btnPickCustomColor";
			this.btnPickCustomColor.Size = new System.Drawing.Size(91, 29);
			this.btnPickCustomColor.TabIndex = 7;
			this.btnPickCustomColor.Text = "Choose...";
			this.btnPickCustomColor.UseVisualStyleBackColor = true;
			this.btnPickCustomColor.Click += new System.EventHandler(this.BtnPickCustomColor_Click);
			// 
			// btnSampleCursorColor
			// 
			this.btnSampleCursorColor.Location = new System.Drawing.Point(274, 156);
			this.btnSampleCursorColor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btnSampleCursorColor.Name = "btnSampleCursorColor";
			this.btnSampleCursorColor.Size = new System.Drawing.Size(102, 29);
			this.btnSampleCursorColor.TabIndex = 8;
			this.btnSampleCursorColor.Text = "Sample";
			this.btnSampleCursorColor.UseVisualStyleBackColor = true;
			this.btnSampleCursorColor.Click += new System.EventHandler(this.BtnSampleCursorColor_Click);
			// 
			// labelInterval
			// 
			this.labelInterval.AutoSize = true;
			this.labelInterval.Location = new System.Drawing.Point(15, 226);
			this.labelInterval.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelInterval.Name = "labelInterval";
			this.labelInterval.Size = new System.Drawing.Size(116, 15);
			this.labelInterval.TabIndex = 9;
			this.labelInterval.Text = "Sample Interval (ms):";
			// 
			// numInterval
			// 
			this.numInterval.Location = new System.Drawing.Point(175, 222);
			this.numInterval.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.numInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.numInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numInterval.Name = "numInterval";
			this.numInterval.Size = new System.Drawing.Size(93, 23);
			this.numInterval.TabIndex = 10;
			this.numInterval.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			this.numInterval.ValueChanged += new System.EventHandler(this.NumInterval_ValueChanged);
			// 
			// labelIntervalUnit
			// 
			this.labelIntervalUnit.AutoSize = true;
			this.labelIntervalUnit.Location = new System.Drawing.Point(274, 226);
			this.labelIntervalUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelIntervalUnit.Name = "labelIntervalUnit";
			this.labelIntervalUnit.Size = new System.Drawing.Size(101, 15);
			this.labelIntervalUnit.TabIndex = 11;
			this.labelIntervalUnit.Text = "(100-10000 range)";
			// 
			// labelVolume
			// 
			this.labelVolume.AutoSize = true;
			this.labelVolume.Location = new System.Drawing.Point(15, 263);
			this.labelVolume.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelVolume.Name = "labelVolume";
			this.labelVolume.Size = new System.Drawing.Size(83, 15);
			this.labelVolume.TabIndex = 12;
			this.labelVolume.Text = "Alarm Volume:";
			// 
			// trackBarVolume
			// 
			this.trackBarVolume.Location = new System.Drawing.Point(117, 257);
			this.trackBarVolume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.trackBarVolume.Maximum = 100;
			this.trackBarVolume.Name = "trackBarVolume";
			this.trackBarVolume.Size = new System.Drawing.Size(260, 45);
			this.trackBarVolume.TabIndex = 13;
			this.trackBarVolume.Value = 100;
			this.trackBarVolume.Scroll += new System.EventHandler(this.TrackBarVolume_Scroll);
			// 
			// labelSoundFile
			// 
			this.labelSoundFile.AutoSize = true;
			this.labelSoundFile.Location = new System.Drawing.Point(15, 309);
			this.labelSoundFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelSoundFile.Name = "labelSoundFile";
			this.labelSoundFile.Size = new System.Drawing.Size(62, 15);
			this.labelSoundFile.TabIndex = 14;
			this.labelSoundFile.Text = "Sound file:";
			// 
			// comboSound
			// 
			this.comboSound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboSound.FormattingEnabled = true;
			this.comboSound.Location = new System.Drawing.Point(117, 304);
			this.comboSound.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.comboSound.Name = "comboSound";
			this.comboSound.Size = new System.Drawing.Size(259, 23);
			this.comboSound.TabIndex = 15;
			this.comboSound.SelectedIndexChanged += new System.EventHandler(this.ComboSound_SelectedIndexChanged);
			// 
			// btnTestAlarm
			// 
			this.btnTestAlarm.Location = new System.Drawing.Point(117, 343);
			this.btnTestAlarm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btnTestAlarm.Name = "btnTestAlarm";
			this.btnTestAlarm.Size = new System.Drawing.Size(117, 29);
			this.btnTestAlarm.TabIndex = 16;
			this.btnTestAlarm.Text = "Test alarm";
			this.btnTestAlarm.UseVisualStyleBackColor = true;
			this.btnTestAlarm.Click += new System.EventHandler(this.BtnTestAlarm_Click);
			//
			// checkKeyPress
			//
			this.checkKeyPress.AutoSize = true;
			this.checkKeyPress.Location = new System.Drawing.Point(15, 384);
			this.checkKeyPress.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkKeyPress.Name = "checkKeyPress";
			this.checkKeyPress.Size = new System.Drawing.Size(120, 19);
			this.checkKeyPress.TabIndex = 17;
			this.checkKeyPress.Text = "Press key on alarm:";
			this.checkKeyPress.UseVisualStyleBackColor = true;
			this.checkKeyPress.CheckedChanged += new System.EventHandler(this.CheckKeyPress_CheckedChanged);
			//
			// textKeyCapture
			//
			this.textKeyCapture.Location = new System.Drawing.Point(175, 381);
			this.textKeyCapture.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.textKeyCapture.Name = "textKeyCapture";
			this.textKeyCapture.ReadOnly = true;
			this.textKeyCapture.Size = new System.Drawing.Size(140, 23);
			this.textKeyCapture.TabIndex = 18;
			this.textKeyCapture.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textKeyCapture.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextKeyCapture_KeyDown);
			//
			// btnClose
			// 
			this.btnClose.Location = new System.Drawing.Point(158, 433);
			this.btnClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(93, 29);
			this.btnClose.TabIndex = 13;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
			// 
			// ColorAlertPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.groupColor);
			this.Controls.Add(this.btnClose);
			this.MinimumSize = new System.Drawing.Size(408, 474);
			this.Name = "ColorAlertPanel";
			this.Padding = new System.Windows.Forms.Padding(10, 5, 10, 0);
			this.Size = new System.Drawing.Size(408, 474);
			this.groupColor.ResumeLayout(false);
			this.groupColor.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numLossMiss)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numMinPixels)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupColor;
        private System.Windows.Forms.CheckBox checkEnabled;
        private System.Windows.Forms.CheckBox checkAlertOnLoss;
        private System.Windows.Forms.CheckBox checkIgnoreDark;
        private System.Windows.Forms.Label labelLossMiss;
        private System.Windows.Forms.NumericUpDown numLossMiss;
        private System.Windows.Forms.Label labelMinPixels;
        private System.Windows.Forms.NumericUpDown numMinPixels;
        private System.Windows.Forms.Label labelDetectedPixels;
        private System.Windows.Forms.Label labelColorSelection;
        private System.Windows.Forms.CheckBox checkRed;
        private System.Windows.Forms.CheckBox checkOrange;
        private System.Windows.Forms.CheckBox checkGray;
        private System.Windows.Forms.CheckBox checkCustomColor;
        private System.Windows.Forms.Panel panelCustomColor;
        private System.Windows.Forms.Button btnPickCustomColor;
        private System.Windows.Forms.Button btnSampleCursorColor;
        private System.Windows.Forms.Label labelInterval;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.Label labelIntervalUnit;
        private System.Windows.Forms.Label labelVolume;
        private System.Windows.Forms.TrackBar trackBarVolume;
        private System.Windows.Forms.Label labelSoundFile;
        private System.Windows.Forms.ComboBox comboSound;
        private System.Windows.Forms.Button btnTestAlarm;
        private System.Windows.Forms.CheckBox checkKeyPress;
        private System.Windows.Forms.TextBox textKeyCapture;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ToolTip tooltipInfo;
    }
}
