namespace OnTopReplicaColorAlert
{
    partial class MainForm
    {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.menuContext = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuContextWindows = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuWindows = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.noneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.switchToWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.addPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuContextClose = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.selectRegionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.chromeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuContextOpacity = new System.Windows.Forms.ToolStripMenuItem();
			this.menuOpacity = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.colorAlertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pauseColorAlertAllPanelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clickThroughToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.enableClickThroughallPanelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.disableClickThroughallPanelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.reduceToIconToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuContextExit = new System.Windows.Forms.ToolStripMenuItem();
			this.menuContext.SuspendLayout();
			this.MenuWindows.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuContext
			// 
			this.menuContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectRegionToolStripMenuItem,
            this.chromeToolStripMenuItem,
            this.menuContextOpacity,
            this.colorAlertToolStripMenuItem,
            this.advancedToolStripMenuItem,
            this.toolStripSeparator2,
            this.menuContextWindows,
            this.switchToWindowToolStripMenuItem,
            this.toolStripSeparator4,
            this.addPanelToolStripMenuItem,
            this.menuContextClose,
            this.toolStripSeparator1,
            this.pauseColorAlertAllPanelsToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.reduceToIconToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.menuContextExit});
			this.menuContext.Name = "menuContext";
			this.menuContext.Size = new System.Drawing.Size(187, 374);
			this.menuContext.Opening += new System.ComponentModel.CancelEventHandler(this.Menu_opening);
			// 
			// menuContextWindows
			// 
			this.menuContextWindows.DropDown = this.MenuWindows;
			this.menuContextWindows.Image = global::OnTopReplicaColorAlert.Properties.Resources.list;
			this.menuContextWindows.Name = "menuContextWindows";
			this.menuContextWindows.Size = new System.Drawing.Size(186, 22);
			this.menuContextWindows.Text = global::OnTopReplicaColorAlert.Strings.MenuWindows;
			this.menuContextWindows.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuWindowsTT;
			// 
			// menuWindows
			// 
			this.MenuWindows.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noneToolStripMenuItem});
			this.MenuWindows.Name = "menuWindows";
			this.MenuWindows.OwnerItem = this.menuContextWindows;
			this.MenuWindows.Size = new System.Drawing.Size(118, 26);
			// 
			// noneToolStripMenuItem
			// 
			this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
			this.noneToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
			this.noneToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuWindowsNone;
			// 
			// switchToWindowToolStripMenuItem
			// 
			this.switchToWindowToolStripMenuItem.Image = global::OnTopReplicaColorAlert.Properties.Resources.xiao_arrow;
			this.switchToWindowToolStripMenuItem.Name = "switchToWindowToolStripMenuItem";
			this.switchToWindowToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.switchToWindowToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuSwitch;
			this.switchToWindowToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuSwitchTT;
			this.switchToWindowToolStripMenuItem.Click += new System.EventHandler(this.Menu_Switch_click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(183, 6);
			// 
			// addPanelToolStripMenuItem
			// 
			this.addPanelToolStripMenuItem.Name = "addPanelToolStripMenuItem";
			this.addPanelToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.addPanelToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuAddPanel;
			this.addPanelToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuAddPanelTT;
			this.addPanelToolStripMenuItem.Click += new System.EventHandler(this.Menu_AddPanel_click);
			// 
			// menuContextClose
			// 
			this.menuContextClose.Name = "menuContextClose";
			this.menuContextClose.Size = new System.Drawing.Size(186, 22);
			this.menuContextClose.Text = global::OnTopReplicaColorAlert.Strings.MenuClosePanel;
			this.menuContextClose.Click += new System.EventHandler(this.Menu_Close_click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(183, 6);
			// 
			// selectRegionToolStripMenuItem
			// 
			this.selectRegionToolStripMenuItem.Enabled = false;
			this.selectRegionToolStripMenuItem.Image = global::OnTopReplicaColorAlert.Properties.Resources.regions;
			this.selectRegionToolStripMenuItem.Name = "selectRegionToolStripMenuItem";
			this.selectRegionToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.selectRegionToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuRegion;
			this.selectRegionToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuRegionTT;
			this.selectRegionToolStripMenuItem.Click += new System.EventHandler(this.Menu_Region_click);
			// 
			// chromeToolStripMenuItem
			// 
			this.chromeToolStripMenuItem.Name = "chromeToolStripMenuItem";
			this.chromeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.chromeToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuChrome;
			this.chromeToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuChromeTT;
			this.chromeToolStripMenuItem.Click += new System.EventHandler(this.Menu_Chrome_click);
			// 
			// menuContextOpacity
			// 
			this.menuContextOpacity.DropDown = this.menuOpacity;
			this.menuContextOpacity.Image = global::OnTopReplicaColorAlert.Properties.Resources.window_opacity;
			this.menuContextOpacity.Name = "menuContextOpacity";
			this.menuContextOpacity.Size = new System.Drawing.Size(186, 22);
			this.menuContextOpacity.Text = global::OnTopReplicaColorAlert.Strings.MenuOpacity;
			// 
			// menuOpacity
			// 
			this.menuOpacity.Name = "menuOpacity";
			this.menuOpacity.ShowCheckMargin = true;
			this.menuOpacity.ShowImageMargin = false;
			this.menuOpacity.Size = new System.Drawing.Size(61, 4);
			this.menuOpacity.Opening += new System.ComponentModel.CancelEventHandler(this.Menu_Opacity_opening);
			// 
			// colorAlertToolStripMenuItem
			// 
			this.colorAlertToolStripMenuItem.Name = "colorAlertToolStripMenuItem";
			this.colorAlertToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.colorAlertToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.ColorAlert_MenuTitle;
			this.colorAlertToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.ColorAlert_MenuTooltip;
			this.colorAlertToolStripMenuItem.Click += new System.EventHandler(this.Menu_ColorAlert_click);
			// 
			// advancedToolStripMenuItem
			// 
			this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clickThroughToolStripMenuItem,
            this.enableClickThroughallPanelsToolStripMenuItem,
            this.disableClickThroughallPanelsToolStripMenuItem});
			this.advancedToolStripMenuItem.Image = global::OnTopReplicaColorAlert.Properties.Resources.xiao_wrench;
			this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
			this.advancedToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.advancedToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuAdvanced;
			// 
			// clickThroughToolStripMenuItem
			// 
			this.clickThroughToolStripMenuItem.Image = global::OnTopReplicaColorAlert.Properties.Resources.window_opacity;
			this.clickThroughToolStripMenuItem.Name = "clickThroughToolStripMenuItem";
			this.clickThroughToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.clickThroughToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuClickThrough;
			this.clickThroughToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuClickThroughTT;
			this.clickThroughToolStripMenuItem.Click += new System.EventHandler(this.Menu_ClickThrough_click);
			// 
			// enableClickThroughallPanelsToolStripMenuItem
			// 
			this.enableClickThroughallPanelsToolStripMenuItem.Name = "enableClickThroughallPanelsToolStripMenuItem";
			this.enableClickThroughallPanelsToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.enableClickThroughallPanelsToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuEnableClickThroughAll;
			this.enableClickThroughallPanelsToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuEnableClickThroughAllTT;
			this.enableClickThroughallPanelsToolStripMenuItem.Click += new System.EventHandler(this.Menu_EnableClickThroughAll_click);
			// 
			// disableClickThroughallPanelsToolStripMenuItem
			// 
			this.disableClickThroughallPanelsToolStripMenuItem.Name = "disableClickThroughallPanelsToolStripMenuItem";
			this.disableClickThroughallPanelsToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.disableClickThroughallPanelsToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuDisableClickThroughAll;
			this.disableClickThroughallPanelsToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuDisableClickThroughAllTT;
			this.disableClickThroughallPanelsToolStripMenuItem.Click += new System.EventHandler(this.Menu_DisableClickThroughAll_click);
			//
			// pauseColorAlertAllPanelsToolStripMenuItem
			//
			this.pauseColorAlertAllPanelsToolStripMenuItem.Name = "pauseColorAlertAllPanelsToolStripMenuItem";
			this.pauseColorAlertAllPanelsToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.pauseColorAlertAllPanelsToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuPauseColorAlertAll;
			this.pauseColorAlertAllPanelsToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuPauseColorAlertAllTT;
			this.pauseColorAlertAllPanelsToolStripMenuItem.Click += new System.EventHandler(this.Menu_ColorAlertPauseResumeAll_click);
			//
			// toolStripSeparator1
			//
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(183, 6);
			// 
			// settingsToolStripMenuItem
			// 
			this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			this.settingsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.settingsToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuSettings;
			this.settingsToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuSettingsTT;
			this.settingsToolStripMenuItem.Click += new System.EventHandler(this.Menu_Settings_click);
			// 
			// reduceToIconToolStripMenuItem
			// 
			this.reduceToIconToolStripMenuItem.Image = global::OnTopReplicaColorAlert.Properties.Resources.minimize;
			this.reduceToIconToolStripMenuItem.Name = "reduceToIconToolStripMenuItem";
			this.reduceToIconToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.reduceToIconToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuReduce;
			this.reduceToIconToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuReduceTT;
			this.reduceToIconToolStripMenuItem.Click += new System.EventHandler(this.Menu_Reduce_click);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Image = global::OnTopReplicaColorAlert.Properties.Resources.help;
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.aboutToolStripMenuItem.Text = global::OnTopReplicaColorAlert.Strings.MenuAbout;
			this.aboutToolStripMenuItem.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuAboutTT;
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.Menu_About_click);
			// 
			// menuContextExit
			// 
			this.menuContextExit.Image = global::OnTopReplicaColorAlert.Properties.Resources.close_new;
			this.menuContextExit.Name = "menuContextExit";
			this.menuContextExit.Size = new System.Drawing.Size(186, 22);
			this.menuContextExit.Text = global::OnTopReplicaColorAlert.Strings.MenuExit;
			this.menuContextExit.ToolTipText = global::OnTopReplicaColorAlert.Strings.MenuExitTT;
			this.menuContextExit.Click += new System.EventHandler(this.Menu_Exit_click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(352, 260);
			this.ControlBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.HideCaption = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(44, 44);
			this.Name = "MainForm";
			this.Text = "OnTopReplicaColorAlert";
			//クローン対象を捕捉した時点で UpdateTopMostState() が有効化する
			this.TopMost = false;
			this.menuContext.ResumeLayout(false);
			this.MenuWindows.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip menuContext;
        private System.Windows.Forms.ToolStripMenuItem menuContextWindows;
        private System.Windows.Forms.ToolStripMenuItem menuContextClose;
        private System.Windows.Forms.ToolStripMenuItem menuContextExit;
        private System.Windows.Forms.ToolStripMenuItem menuContextOpacity;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ContextMenuStrip menuOpacity;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorAlertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseColorAlertAllPanelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reduceToIconToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addPanelToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem selectRegionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem switchToWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chromeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clickThroughToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem enableClickThroughallPanelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableClickThroughallPanelsToolStripMenuItem;
    }
}

