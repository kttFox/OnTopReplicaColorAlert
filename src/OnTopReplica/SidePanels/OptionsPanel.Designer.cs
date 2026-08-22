namespace OnTopReplica.SidePanels {
    partial class OptionsPanel {
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
			this.btnClose = new System.Windows.Forms.Button();
			this.panelMain = new System.Windows.Forms.Panel();
			this.groupLanguage = new System.Windows.Forms.Label();
			this.comboLanguage = new OnTopReplica.ImageComboBox();
			this.lblLanguage = new System.Windows.Forms.Label();
			this.groupIndicator = new System.Windows.Forms.Label();
			this.checkIndicator = new System.Windows.Forms.CheckBox();
			this.lblIndicatorSize = new System.Windows.Forms.Label();
			this.numIndicatorSize = new System.Windows.Forms.NumericUpDown();
			this.lblIndicatorRunColor = new System.Windows.Forms.Label();
			this.panelIndicatorRunColor = new System.Windows.Forms.Panel();
			this.lblIndicatorPauseColor = new System.Windows.Forms.Label();
			this.panelIndicatorPauseColor = new System.Windows.Forms.Panel();
			this.checkPauseColorAlertOnLoss = new System.Windows.Forms.CheckBox();
			this.groupAutoHide = new System.Windows.Forms.Label();
			this.checkAutoHide = new System.Windows.Forms.CheckBox();
			this.panelMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numIndicatorSize)).BeginInit();
			this.SuspendLayout();
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.Location = new System.Drawing.Point(220, 317);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(87, 27);
			this.btnClose.TabIndex = 20;
			this.btnClose.Text = global::OnTopReplica.Strings.MenuClose;
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.Close_click);
			// 
			// panelMain
			// 
			this.panelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelMain.AutoScroll = true;
			this.panelMain.Controls.Add(this.groupLanguage);
			this.panelMain.Controls.Add(this.comboLanguage);
			this.panelMain.Controls.Add(this.lblLanguage);
			this.panelMain.Controls.Add(this.groupIndicator);
			this.panelMain.Controls.Add(this.checkIndicator);
			this.panelMain.Controls.Add(this.lblIndicatorSize);
			this.panelMain.Controls.Add(this.numIndicatorSize);
			this.panelMain.Controls.Add(this.lblIndicatorRunColor);
			this.panelMain.Controls.Add(this.panelIndicatorRunColor);
			this.panelMain.Controls.Add(this.lblIndicatorPauseColor);
			this.panelMain.Controls.Add(this.panelIndicatorPauseColor);
			this.panelMain.Controls.Add(this.checkPauseColorAlertOnLoss);
			this.panelMain.Controls.Add(this.groupAutoHide);
			this.panelMain.Controls.Add(this.checkAutoHide);
			this.panelMain.Location = new System.Drawing.Point(7, 7);
			this.panelMain.Name = "panelMain";
			this.panelMain.Size = new System.Drawing.Size(301, 304);
			this.panelMain.TabIndex = 1;
			// 
			// groupLanguage
			// 
			this.groupLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupLanguage.AutoSize = true;
			this.groupLanguage.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.groupLanguage.Location = new System.Drawing.Point(3, 5);
			this.groupLanguage.Name = "groupLanguage";
			this.groupLanguage.Size = new System.Drawing.Size(62, 15);
			this.groupLanguage.TabIndex = 0;
			this.groupLanguage.Text = "Language:";
			// 
			// comboLanguage
			// 
			this.comboLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboLanguage.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.comboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboLanguage.FormattingEnabled = true;
			this.comboLanguage.IconList = null;
			this.comboLanguage.Location = new System.Drawing.Point(16, 26);
			this.comboLanguage.Name = "comboLanguage";
			this.comboLanguage.Size = new System.Drawing.Size(268, 24);
			this.comboLanguage.TabIndex = 1;
			this.comboLanguage.SelectedIndexChanged += new System.EventHandler(this.LanguageBox_IndexChange);
			// 
			// lblLanguage
			// 
			this.lblLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblLanguage.Location = new System.Drawing.Point(16, 53);
			this.lblLanguage.Name = "lblLanguage";
			this.lblLanguage.Size = new System.Drawing.Size(268, 22);
			this.lblLanguage.TabIndex = 2;
			this.lblLanguage.Text = "Requires a restart.";
			// 
			// groupIndicator
			// 
			this.groupIndicator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupIndicator.AutoSize = true;
			this.groupIndicator.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.groupIndicator.Location = new System.Drawing.Point(3, 85);
			this.groupIndicator.Name = "groupIndicator";
			this.groupIndicator.Size = new System.Drawing.Size(65, 15);
			this.groupIndicator.TabIndex = 9;
			this.groupIndicator.Text = "Color alert:";
			// 
			// checkIndicator
			// 
			this.checkIndicator.AutoSize = true;
			this.checkIndicator.Checked = true;
			this.checkIndicator.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkIndicator.Location = new System.Drawing.Point(16, 109);
			this.checkIndicator.Name = "checkIndicator";
			this.checkIndicator.Size = new System.Drawing.Size(126, 19);
			this.checkIndicator.TabIndex = 10;
			this.checkIndicator.Text = "Show indicator dot";
			this.checkIndicator.UseVisualStyleBackColor = true;
			this.checkIndicator.CheckedChanged += new System.EventHandler(this.Indicator_SettingChanged);
			// 
			// lblIndicatorSize
			// 
			this.lblIndicatorSize.AutoSize = true;
			this.lblIndicatorSize.Location = new System.Drawing.Point(23, 136);
			this.lblIndicatorSize.Name = "lblIndicatorSize";
			this.lblIndicatorSize.Size = new System.Drawing.Size(75, 15);
			this.lblIndicatorSize.TabIndex = 11;
			this.lblIndicatorSize.Text = "Dot size (px):";
			this.lblIndicatorSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numIndicatorSize
			// 
			this.numIndicatorSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.numIndicatorSize.Location = new System.Drawing.Point(224, 134);
			this.numIndicatorSize.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numIndicatorSize.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.numIndicatorSize.Name = "numIndicatorSize";
			this.numIndicatorSize.Size = new System.Drawing.Size(60, 23);
			this.numIndicatorSize.TabIndex = 12;
			this.numIndicatorSize.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
			this.numIndicatorSize.ValueChanged += new System.EventHandler(this.Indicator_SettingChanged);
			// 
			// lblIndicatorRunColor
			// 
			this.lblIndicatorRunColor.AutoSize = true;
			this.lblIndicatorRunColor.Location = new System.Drawing.Point(23, 166);
			this.lblIndicatorRunColor.Name = "lblIndicatorRunColor";
			this.lblIndicatorRunColor.Size = new System.Drawing.Size(85, 15);
			this.lblIndicatorRunColor.TabIndex = 13;
			this.lblIndicatorRunColor.Text = "Running color:";
			this.lblIndicatorRunColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelIndicatorRunColor
			// 
			this.panelIndicatorRunColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelIndicatorRunColor.BackColor = System.Drawing.Color.Red;
			this.panelIndicatorRunColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelIndicatorRunColor.Cursor = System.Windows.Forms.Cursors.Hand;
			this.panelIndicatorRunColor.Location = new System.Drawing.Point(224, 166);
			this.panelIndicatorRunColor.Name = "panelIndicatorRunColor";
			this.panelIndicatorRunColor.Size = new System.Drawing.Size(60, 20);
			this.panelIndicatorRunColor.TabIndex = 14;
			this.panelIndicatorRunColor.Click += new System.EventHandler(this.PanelIndicatorRunColor_Click);
			// 
			// lblIndicatorPauseColor
			// 
			this.lblIndicatorPauseColor.AutoSize = true;
			this.lblIndicatorPauseColor.Location = new System.Drawing.Point(23, 196);
			this.lblIndicatorPauseColor.Name = "lblIndicatorPauseColor";
			this.lblIndicatorPauseColor.Size = new System.Drawing.Size(78, 15);
			this.lblIndicatorPauseColor.TabIndex = 15;
			this.lblIndicatorPauseColor.Text = "Stopped color:";
			this.lblIndicatorPauseColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelIndicatorPauseColor
			// 
			this.panelIndicatorPauseColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelIndicatorPauseColor.BackColor = System.Drawing.Color.LimeGreen;
			this.panelIndicatorPauseColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelIndicatorPauseColor.Cursor = System.Windows.Forms.Cursors.Hand;
			this.panelIndicatorPauseColor.Location = new System.Drawing.Point(224, 196);
			this.panelIndicatorPauseColor.Name = "panelIndicatorPauseColor";
			this.panelIndicatorPauseColor.Size = new System.Drawing.Size(60, 20);
			this.panelIndicatorPauseColor.TabIndex = 16;
			this.panelIndicatorPauseColor.Click += new System.EventHandler(this.PanelIndicatorPauseColor_Click);
			// 
			// checkPauseColorAlertOnLoss
			// 
			this.checkPauseColorAlertOnLoss.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPauseColorAlertOnLoss.AutoSize = true;
			this.checkPauseColorAlertOnLoss.Location = new System.Drawing.Point(16, 225);
			this.checkPauseColorAlertOnLoss.Name = "checkPauseColorAlertOnLoss";
			this.checkPauseColorAlertOnLoss.Size = new System.Drawing.Size(248, 19);
			this.checkPauseColorAlertOnLoss.TabIndex = 17;
			this.checkPauseColorAlertOnLoss.Text = "Stop color alerts when the window is lost";
			this.checkPauseColorAlertOnLoss.UseVisualStyleBackColor = true;
			this.checkPauseColorAlertOnLoss.CheckedChanged += new System.EventHandler(this.PauseColorAlertOnLoss_CheckedChanged);
			// 
			// groupAutoHide
			// 
			this.groupAutoHide.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupAutoHide.AutoSize = true;
			this.groupAutoHide.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.groupAutoHide.Location = new System.Drawing.Point(3, 255);
			this.groupAutoHide.Name = "groupAutoHide";
			this.groupAutoHide.Size = new System.Drawing.Size(62, 15);
			this.groupAutoHide.TabIndex = 18;
			this.groupAutoHide.Text = "Auto hide:";
			// 
			// checkAutoHide
			// 
			this.checkAutoHide.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAutoHide.AutoSize = true;
			this.checkAutoHide.Location = new System.Drawing.Point(16, 276);
			this.checkAutoHide.Name = "checkAutoHide";
			this.checkAutoHide.Size = new System.Drawing.Size(253, 19);
			this.checkAutoHide.TabIndex = 19;
			this.checkAutoHide.Text = "Show/hide in sync with the cloned window";
			this.checkAutoHide.UseVisualStyleBackColor = true;
			this.checkAutoHide.CheckedChanged += new System.EventHandler(this.AutoHide_CheckedChanged);
			// 
			// OptionsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.Controls.Add(this.panelMain);
			this.Controls.Add(this.btnClose);
			this.MinimumSize = new System.Drawing.Size(315, 279);
			this.Name = "OptionsPanel";
			this.Padding = new System.Windows.Forms.Padding(7);
			this.Size = new System.Drawing.Size(315, 351);
			this.panelMain.ResumeLayout(false);
			this.panelMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numIndicatorSize)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label groupLanguage;
        private System.Windows.Forms.Label lblLanguage;
        private ImageComboBox comboLanguage;
        private System.Windows.Forms.Label groupIndicator;
        private System.Windows.Forms.CheckBox checkIndicator;
        private System.Windows.Forms.Label lblIndicatorSize;
        private System.Windows.Forms.NumericUpDown numIndicatorSize;
        private System.Windows.Forms.Label lblIndicatorRunColor;
        private System.Windows.Forms.Panel panelIndicatorRunColor;
        private System.Windows.Forms.Label lblIndicatorPauseColor;
        private System.Windows.Forms.Panel panelIndicatorPauseColor;
        private System.Windows.Forms.Label groupAutoHide;
        private System.Windows.Forms.CheckBox checkAutoHide;
        private System.Windows.Forms.CheckBox checkPauseColorAlertOnLoss;
    }
}
