namespace OnTopReplicaColorAlert.SidePanels {
    partial class AboutPanelContents {
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
			this.labelTranslators = new System.Windows.Forms.Label();
			this.linkCredits = new System.Windows.Forms.LinkLabel();
			this.labeledDivider2 = new WindowsFormsAero.LabeledDivider();
			this.linkHomepage = new System.Windows.Forms.LinkLabel();
			this.linkAuthor = new System.Windows.Forms.LinkLabel();
			this.lblSlogan = new System.Windows.Forms.Label();
			this.labeledDivider3 = new WindowsFormsAero.LabeledDivider();
			this.linkLicense = new System.Windows.Forms.LinkLabel();
			this.labeledDivider4 = new WindowsFormsAero.LabeledDivider();
			this.linkContribute = new System.Windows.Forms.LinkLabel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// labelTranslators
			// 
			this.labelTranslators.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelTranslators.AutoEllipsis = true;
			this.labelTranslators.Location = new System.Drawing.Point(0, 175);
			this.labelTranslators.Name = "labelTranslators";
			this.labelTranslators.Size = new System.Drawing.Size(376, 93);
			this.labelTranslators.TabIndex = 31;
			this.labelTranslators.Text = "Translators:";
			// 
			// linkCredits
			// 
			this.linkCredits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.linkCredits.AutoEllipsis = true;
			this.linkCredits.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkCredits.Location = new System.Drawing.Point(0, 114);
			this.linkCredits.Name = "linkCredits";
			this.linkCredits.Size = new System.Drawing.Size(376, 53);
			this.linkCredits.TabIndex = 30;
			this.linkCredits.TabStop = true;
			this.linkCredits.Text = "%CREDITS%";
			this.linkCredits.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkCredits_click);
			// 
			// labeledDivider2
			// 
			this.labeledDivider2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labeledDivider2.DividerColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(191)))), ((int)(((byte)(222)))));
			this.labeledDivider2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.labeledDivider2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(170)))));
			this.labeledDivider2.Location = new System.Drawing.Point(0, 90);
			this.labeledDivider2.Name = "labeledDivider2";
			this.labeledDivider2.Size = new System.Drawing.Size(391, 21);
			this.labeledDivider2.TabIndex = 26;
			this.labeledDivider2.Text = global::OnTopReplicaColorAlert.Strings.AboutDividerCredits;
			// 
			// linkHomepage
			// 
			this.linkHomepage.AutoSize = true;
			this.linkHomepage.BackColor = System.Drawing.Color.Transparent;
			this.linkHomepage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkHomepage.Location = new System.Drawing.Point(67, 46);
			this.linkHomepage.Name = "linkHomepage";
			this.linkHomepage.Size = new System.Drawing.Size(175, 17);
			this.linkHomepage.TabIndex = 23;
			this.linkHomepage.TabStop = true;
			this.linkHomepage.Text = "http://ontopreplica.codeplex.com";
			this.linkHomepage.UseCompatibleTextRendering = true;
			this.linkHomepage.VisitedLinkColor = System.Drawing.Color.Blue;
			this.linkHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkHomepage_clicked);
			// 
			// linkAuthor
			// 
			this.linkAuthor.AutoSize = true;
			this.linkAuthor.BackColor = System.Drawing.Color.Transparent;
			this.linkAuthor.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkAuthor.LinkColor = System.Drawing.Color.Blue;
			this.linkAuthor.Location = new System.Drawing.Point(67, 30);
			this.linkAuthor.Name = "linkAuthor";
			this.linkAuthor.Size = new System.Drawing.Size(64, 17);
			this.linkAuthor.TabIndex = 22;
			this.linkAuthor.TabStop = true;
			this.linkAuthor.Text = "%AUTHOR%";
			this.linkAuthor.UseCompatibleTextRendering = true;
			this.linkAuthor.VisitedLinkColor = System.Drawing.Color.Blue;
			this.linkAuthor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkAuthor_clicked);
			// 
			// lblSlogan
			// 
			this.lblSlogan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSlogan.AutoEllipsis = true;
			this.lblSlogan.BackColor = System.Drawing.Color.Transparent;
			this.lblSlogan.Location = new System.Drawing.Point(67, 0);
			this.lblSlogan.Name = "lblSlogan";
			this.lblSlogan.Size = new System.Drawing.Size(324, 30);
			this.lblSlogan.TabIndex = 24;
			this.lblSlogan.Text = "A lightweight, real-time, always on top thumbnail of a window of your choice.";
			// 
			// labeledDivider3
			// 
			this.labeledDivider3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labeledDivider3.DividerColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(191)))), ((int)(((byte)(222)))));
			this.labeledDivider3.Font = new System.Drawing.Font("MS UI Gothic", 9F);
			this.labeledDivider3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(170)))));
			this.labeledDivider3.Location = new System.Drawing.Point(0, 271);
			this.labeledDivider3.Name = "labeledDivider3";
			this.labeledDivider3.Size = new System.Drawing.Size(391, 21);
			this.labeledDivider3.TabIndex = 32;
			this.labeledDivider3.Text = "License";
			// 
			// linkLicense
			// 
			this.linkLicense.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.linkLicense.AutoEllipsis = true;
			this.linkLicense.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkLicense.Location = new System.Drawing.Point(0, 295);
			this.linkLicense.Name = "linkLicense";
			this.linkLicense.Size = new System.Drawing.Size(376, 75);
			this.linkLicense.TabIndex = 33;
			this.linkLicense.TabStop = true;
			this.linkLicense.Text = "%LICENSE%";
			this.linkLicense.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLicense_click);
			// 
			// labeledDivider4
			// 
			this.labeledDivider4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labeledDivider4.DividerColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(191)))), ((int)(((byte)(222)))));
			this.labeledDivider4.Font = new System.Drawing.Font("MS UI Gothic", 9F);
			this.labeledDivider4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(170)))));
			this.labeledDivider4.Location = new System.Drawing.Point(0, 372);
			this.labeledDivider4.Name = "labeledDivider4";
			this.labeledDivider4.Size = new System.Drawing.Size(391, 21);
			this.labeledDivider4.TabIndex = 34;
			this.labeledDivider4.Text = "Contribute";
			// 
			// linkContribute
			// 
			this.linkContribute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.linkContribute.AutoEllipsis = true;
			this.linkContribute.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkContribute.Location = new System.Drawing.Point(0, 396);
			this.linkContribute.Name = "linkContribute";
			this.linkContribute.Size = new System.Drawing.Size(376, 78);
			this.linkContribute.TabIndex = 35;
			this.linkContribute.TabStop = true;
			this.linkContribute.Text = "%CONTRIBUTE%";
			this.linkContribute.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkContribute_clicked);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::OnTopReplicaColorAlert.Properties.Resources.flat_logo_64;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(64, 59);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 36;
			this.pictureBox1.TabStop = false;
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
			this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkLabel1.Location = new System.Drawing.Point(67, 63);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(219, 17);
			this.linkLabel1.TabIndex = 23;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "https://github.com/kttFox/OnTopReplicaColorAlert";
			this.linkLabel1.UseCompatibleTextRendering = true;
			this.linkLabel1.VisitedLinkColor = System.Drawing.Color.Blue;
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// AboutPanelContents
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.linkContribute);
			this.Controls.Add(this.labeledDivider4);
			this.Controls.Add(this.linkLicense);
			this.Controls.Add(this.labeledDivider3);
			this.Controls.Add(this.labelTranslators);
			this.Controls.Add(this.linkCredits);
			this.Controls.Add(this.labeledDivider2);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.linkHomepage);
			this.Controls.Add(this.linkAuthor);
			this.Controls.Add(this.lblSlogan);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "AboutPanelContents";
			this.Size = new System.Drawing.Size(379, 429);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTranslators;
        private System.Windows.Forms.LinkLabel linkCredits;
        private WindowsFormsAero.LabeledDivider labeledDivider2;
        private System.Windows.Forms.LinkLabel linkHomepage;
        private System.Windows.Forms.LinkLabel linkAuthor;
        private System.Windows.Forms.Label lblSlogan;
        private WindowsFormsAero.LabeledDivider labeledDivider3;
        private System.Windows.Forms.LinkLabel linkLicense;
        private WindowsFormsAero.LabeledDivider labeledDivider4;
        private System.Windows.Forms.LinkLabel linkContribute;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}
