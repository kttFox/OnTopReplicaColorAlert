namespace OnTopReplica
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
			this.resizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuResize = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.doubleToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.fitToWindowToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.halfToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.quarterToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.dockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.disabledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.topLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.topRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.centerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bottomLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bottomRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
			this.menuResize.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuContext
			// 
			this.menuContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuContextWindows,
            this.switchToWindowToolStripMenuItem,
            this.toolStripSeparator4,
            this.addPanelToolStripMenuItem,
            this.menuContextClose,
            this.toolStripSeparator2,
            this.selectRegionToolStripMenuItem,
            this.chromeToolStripMenuItem,
            this.menuContextOpacity,
            this.resizeToolStripMenuItem,
            this.dockToolStripMenuItem,
            this.colorAlertToolStripMenuItem,
            this.advancedToolStripMenuItem,
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
			this.menuContextWindows.Image = global::OnTopReplica.Properties.Resources.list;
			this.menuContextWindows.Name = "menuContextWindows";
			this.menuContextWindows.Size = new System.Drawing.Size(186, 22);
			this.menuContextWindows.Text = global::OnTopReplica.Strings.MenuWindows;
			this.menuContextWindows.ToolTipText = global::OnTopReplica.Strings.MenuWindowsTT;
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
			this.noneToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuWindowsNone;
			// 
			// switchToWindowToolStripMenuItem
			// 
			this.switchToWindowToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.xiao_arrow;
			this.switchToWindowToolStripMenuItem.Name = "switchToWindowToolStripMenuItem";
			this.switchToWindowToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.switchToWindowToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuSwitch;
			this.switchToWindowToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuSwitchTT;
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
			this.addPanelToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuAddPanel;
			this.addPanelToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuAddPanelTT;
			this.addPanelToolStripMenuItem.Click += new System.EventHandler(this.Menu_AddPanel_click);
			// 
			// menuContextClose
			// 
			this.menuContextClose.Name = "menuContextClose";
			this.menuContextClose.Size = new System.Drawing.Size(186, 22);
			this.menuContextClose.Text = global::OnTopReplica.Strings.MenuClosePanel;
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
			this.selectRegionToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.regions;
			this.selectRegionToolStripMenuItem.Name = "selectRegionToolStripMenuItem";
			this.selectRegionToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.selectRegionToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuRegion;
			this.selectRegionToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuRegionTT;
			this.selectRegionToolStripMenuItem.Click += new System.EventHandler(this.Menu_Region_click);
			// 
			// chromeToolStripMenuItem
			// 
			this.chromeToolStripMenuItem.Name = "chromeToolStripMenuItem";
			this.chromeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.chromeToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuChrome;
			this.chromeToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuChromeTT;
			this.chromeToolStripMenuItem.Click += new System.EventHandler(this.Menu_Chrome_click);
			// 
			// menuContextOpacity
			// 
			this.menuContextOpacity.DropDown = this.menuOpacity;
			this.menuContextOpacity.Image = global::OnTopReplica.Properties.Resources.window_opacity;
			this.menuContextOpacity.Name = "menuContextOpacity";
			this.menuContextOpacity.Size = new System.Drawing.Size(186, 22);
			this.menuContextOpacity.Text = global::OnTopReplica.Strings.MenuOpacity;
			// 
			// menuOpacity
			// 
			this.menuOpacity.Name = "menuOpacity";
			this.menuOpacity.ShowCheckMargin = true;
			this.menuOpacity.ShowImageMargin = false;
			this.menuOpacity.Size = new System.Drawing.Size(61, 4);
			this.menuOpacity.Opening += new System.ComponentModel.CancelEventHandler(this.Menu_Opacity_opening);
			// 
			// resizeToolStripMenuItem
			// 
			this.resizeToolStripMenuItem.DropDown = this.menuResize;
			this.resizeToolStripMenuItem.Enabled = false;
			this.resizeToolStripMenuItem.Name = "resizeToolStripMenuItem";
			this.resizeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.resizeToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuResize;
			// 
			// menuResize
			// 
			this.menuResize.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.doubleToolStripMenuItem1,
            this.fitToWindowToolStripMenuItem1,
            this.halfToolStripMenuItem1,
            this.quarterToolStripMenuItem1});
			this.menuResize.Name = "menuResize";
			this.menuResize.OwnerItem = this.resizeToolStripMenuItem;
			this.menuResize.Size = new System.Drawing.Size(165, 120);
			this.menuResize.Opening += new System.ComponentModel.CancelEventHandler(this.Menu_Resize_opening);
			// 
			// doubleToolStripMenuItem1
			// 
			this.doubleToolStripMenuItem1.Name = "doubleToolStripMenuItem1";
			this.doubleToolStripMenuItem1.Size = new System.Drawing.Size(164, 22);
			this.doubleToolStripMenuItem1.Text = global::OnTopReplica.Strings.MenuFitDouble;
			this.doubleToolStripMenuItem1.Click += new System.EventHandler(this.Menu_Resize_Double);
			// 
			// fitToWindowToolStripMenuItem1
			// 
			this.fitToWindowToolStripMenuItem1.Name = "fitToWindowToolStripMenuItem1";
			this.fitToWindowToolStripMenuItem1.Size = new System.Drawing.Size(164, 22);
			this.fitToWindowToolStripMenuItem1.Text = global::OnTopReplica.Strings.MenuFitOriginal;
			this.fitToWindowToolStripMenuItem1.Click += new System.EventHandler(this.Menu_Resize_FitToWindow);
			// 
			// halfToolStripMenuItem1
			// 
			this.halfToolStripMenuItem1.Name = "halfToolStripMenuItem1";
			this.halfToolStripMenuItem1.Size = new System.Drawing.Size(164, 22);
			this.halfToolStripMenuItem1.Text = global::OnTopReplica.Strings.MenuFitHalf;
			this.halfToolStripMenuItem1.Click += new System.EventHandler(this.Menu_Resize_Half);
			// 
			// quarterToolStripMenuItem1
			// 
			this.quarterToolStripMenuItem1.Name = "quarterToolStripMenuItem1";
			this.quarterToolStripMenuItem1.Size = new System.Drawing.Size(164, 22);
			this.quarterToolStripMenuItem1.Text = global::OnTopReplica.Strings.MenuFitQuarter;
			this.quarterToolStripMenuItem1.Click += new System.EventHandler(this.Menu_Resize_Quarter);
			// 
			// dockToolStripMenuItem
			// 
			this.dockToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disabledToolStripMenuItem,
            this.topLeftToolStripMenuItem,
            this.topRightToolStripMenuItem,
            this.centerToolStripMenuItem,
            this.bottomLeftToolStripMenuItem,
            this.bottomRightToolStripMenuItem});
			this.dockToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.pos_null;
			this.dockToolStripMenuItem.Name = "dockToolStripMenuItem";
			this.dockToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.dockToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuPosition;
			this.dockToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuPositionTT;
			this.dockToolStripMenuItem.DropDownOpening += new System.EventHandler(this.Menu_Position_Opening);
			// 
			// disabledToolStripMenuItem
			// 
			this.disabledToolStripMenuItem.Checked = true;
			this.disabledToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.disabledToolStripMenuItem.Name = "disabledToolStripMenuItem";
			this.disabledToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.disabledToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuPosDisabled;
			this.disabledToolStripMenuItem.Click += new System.EventHandler(this.Menu_Position_Disable);
			// 
			// topLeftToolStripMenuItem
			// 
			this.topLeftToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.pos_topleft;
			this.topLeftToolStripMenuItem.Name = "topLeftToolStripMenuItem";
			this.topLeftToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.topLeftToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuPosTopLeft;
			this.topLeftToolStripMenuItem.Click += new System.EventHandler(this.Menu_Position_TopLeft);
			// 
			// topRightToolStripMenuItem
			// 
			this.topRightToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.pos_topright;
			this.topRightToolStripMenuItem.Name = "topRightToolStripMenuItem";
			this.topRightToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.topRightToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuPosTopRight;
			this.topRightToolStripMenuItem.Click += new System.EventHandler(this.Menu_Position_TopRight);
			// 
			// centerToolStripMenuItem
			// 
			this.centerToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.pos_center;
			this.centerToolStripMenuItem.Name = "centerToolStripMenuItem";
			this.centerToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.centerToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuPosCenter;
			this.centerToolStripMenuItem.Click += new System.EventHandler(this.Menu_Position_Center);
			// 
			// bottomLeftToolStripMenuItem
			// 
			this.bottomLeftToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.pos_bottomleft;
			this.bottomLeftToolStripMenuItem.Name = "bottomLeftToolStripMenuItem";
			this.bottomLeftToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.bottomLeftToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuPosBottomLeft;
			this.bottomLeftToolStripMenuItem.Click += new System.EventHandler(this.Menu_Position_BottomLeft);
			// 
			// bottomRightToolStripMenuItem
			// 
			this.bottomRightToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.pos_bottomright;
			this.bottomRightToolStripMenuItem.Name = "bottomRightToolStripMenuItem";
			this.bottomRightToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.bottomRightToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuPosBottomRight;
			this.bottomRightToolStripMenuItem.Click += new System.EventHandler(this.Menu_Position_BottomRight);
			// 
			// colorAlertToolStripMenuItem
			// 
			this.colorAlertToolStripMenuItem.Name = "colorAlertToolStripMenuItem";
			this.colorAlertToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.colorAlertToolStripMenuItem.Text = global::OnTopReplica.Strings.ColorAlert_MenuTitle;
			this.colorAlertToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.ColorAlert_MenuTooltip;
			this.colorAlertToolStripMenuItem.Click += new System.EventHandler(this.Menu_ColorAlert_click);
			// 
			// advancedToolStripMenuItem
			// 
			this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clickThroughToolStripMenuItem,
            this.enableClickThroughallPanelsToolStripMenuItem,
            this.disableClickThroughallPanelsToolStripMenuItem});
			this.advancedToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.xiao_wrench;
			this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
			this.advancedToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.advancedToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuAdvanced;
			// 
			// clickThroughToolStripMenuItem
			// 
			this.clickThroughToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.window_opacity;
			this.clickThroughToolStripMenuItem.Name = "clickThroughToolStripMenuItem";
			this.clickThroughToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.clickThroughToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuClickThrough;
			this.clickThroughToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuClickThroughTT;
			this.clickThroughToolStripMenuItem.Click += new System.EventHandler(this.Menu_ClickThrough_click);
			// 
			// enableClickThroughallPanelsToolStripMenuItem
			// 
			this.enableClickThroughallPanelsToolStripMenuItem.Name = "enableClickThroughallPanelsToolStripMenuItem";
			this.enableClickThroughallPanelsToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.enableClickThroughallPanelsToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuEnableClickThroughAll;
			this.enableClickThroughallPanelsToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuEnableClickThroughAllTT;
			this.enableClickThroughallPanelsToolStripMenuItem.Click += new System.EventHandler(this.Menu_EnableClickThroughAll_click);
			// 
			// disableClickThroughallPanelsToolStripMenuItem
			// 
			this.disableClickThroughallPanelsToolStripMenuItem.Name = "disableClickThroughallPanelsToolStripMenuItem";
			this.disableClickThroughallPanelsToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.disableClickThroughallPanelsToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuDisableClickThroughAll;
			this.disableClickThroughallPanelsToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuDisableClickThroughAllTT;
			this.disableClickThroughallPanelsToolStripMenuItem.Click += new System.EventHandler(this.Menu_DisableClickThroughAll_click);
			//
			// pauseColorAlertAllPanelsToolStripMenuItem
			//
			this.pauseColorAlertAllPanelsToolStripMenuItem.Name = "pauseColorAlertAllPanelsToolStripMenuItem";
			this.pauseColorAlertAllPanelsToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.pauseColorAlertAllPanelsToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuPauseColorAlertAll;
			this.pauseColorAlertAllPanelsToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuPauseColorAlertAllTT;
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
			this.settingsToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuSettings;
			this.settingsToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuSettingsTT;
			this.settingsToolStripMenuItem.Click += new System.EventHandler(this.Menu_Settings_click);
			// 
			// reduceToIconToolStripMenuItem
			// 
			this.reduceToIconToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.minimize;
			this.reduceToIconToolStripMenuItem.Name = "reduceToIconToolStripMenuItem";
			this.reduceToIconToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.reduceToIconToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuReduce;
			this.reduceToIconToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuReduceTT;
			this.reduceToIconToolStripMenuItem.Click += new System.EventHandler(this.Menu_Reduce_click);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Image = global::OnTopReplica.Properties.Resources.help;
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.aboutToolStripMenuItem.Text = global::OnTopReplica.Strings.MenuAbout;
			this.aboutToolStripMenuItem.ToolTipText = global::OnTopReplica.Strings.MenuAboutTT;
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.Menu_About_click);
			// 
			// menuContextExit
			// 
			this.menuContextExit.Image = global::OnTopReplica.Properties.Resources.close_new;
			this.menuContextExit.Name = "menuContextExit";
			this.menuContextExit.Size = new System.Drawing.Size(186, 22);
			this.menuContextExit.Text = global::OnTopReplica.Strings.MenuExit;
			this.menuContextExit.ToolTipText = global::OnTopReplica.Strings.MenuExitTT;
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
			this.Text = "OnTopReplica";
			this.TopMost = true;
			this.menuContext.ResumeLayout(false);
			this.MenuWindows.ResumeLayout(false);
			this.menuResize.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem resizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem switchToWindowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem dockToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem topLeftToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem topRightToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bottomLeftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bottomRightToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip menuResize;
		private System.Windows.Forms.ToolStripMenuItem doubleToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem fitToWindowToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem halfToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem quarterToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem chromeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clickThroughToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem centerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disabledToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem enableClickThroughallPanelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableClickThroughallPanelsToolStripMenuItem;
    }
}

