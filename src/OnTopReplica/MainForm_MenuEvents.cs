using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OnTopReplica.Properties;
using WindowsFormsAero.TaskDialog;
using OnTopReplica.SidePanels;

namespace OnTopReplica {
    partial class MainForm {

        private void Menu_opening(object sender, CancelEventArgs e) {
            //Cancel if a side panel is open
            if (IsSidePanelOpen) {
                e.Cancel = true;
                return;
            }

            bool showing = ThumbnailPanel.IsShowingThumbnail;

            selectRegionToolStripMenuItem.Enabled = showing;
            switchToWindowToolStripMenuItem.Enabled = showing;
            chromeToolStripMenuItem.Checked = IsChromeVisible;
            chromeToolStripMenuItem.Enabled = showing;
            clickThroughToolStripMenuItem.Enabled = showing;

            //停止状態に応じてメニュー表記を切り替える
            bool alertPaused = IsColorAlertPausedAllPanels;
            pauseColorAlertAllPanelsToolStripMenuItem.Text = alertPaused
                ? Strings.MenuResumeColorAlertAll : Strings.MenuPauseColorAlertAll;
            pauseColorAlertAllPanelsToolStripMenuItem.ToolTipText = alertPaused
                ? Strings.MenuResumeColorAlertAllTT : Strings.MenuPauseColorAlertAllTT;
        }

        private void Menu_Switch_click(object sender, EventArgs e) {
            if (CurrentThumbnailWindowHandle == null)
                return;

            //Hide the whole panel set while the source window is in the foreground
            HideAllPanels();
            Native.WindowManagerMethods.SetForegroundWindow(CurrentThumbnailWindowHandle.Handle);
        }

        private void Menu_ClickThrough_click(object sender, EventArgs e) {
            ClickThroughEnabled = true;
        }

        private void Menu_EnableClickThroughAll_click(object sender, EventArgs e) {
            EnableClickThroughAllPanels();
        }

        private void Menu_DisableClickThroughAll_click(object sender, EventArgs e) {
            DisableClickThroughAllPanels();
        }

        private void Menu_Opacity_opening(object sender, CancelEventArgs e) {
            foreach (ToolStripItem item in menuOpacity.Items) {
                var menuItem = item as ToolStripMenuItem;
                if (menuItem == null || !(menuItem.Tag is double))
                    continue;

                //Form.Opacity is stored with limited precision: compare with tolerance
                menuItem.Checked = Math.Abs((double)menuItem.Tag - this.Opacity) < 0.005;
            }
        }

        private void Menu_Opacity_click(object sender, EventArgs e) {
            ToolStripMenuItem tsi = (ToolStripMenuItem)sender;

            if (this.Visible) {
                //Target opacity is stored in the item's tag
                this.Opacity = (double)tsi.Tag;
                Program.Platform.OnFormStateChange(this);
                NotifyPanelLayoutChanged();
            }
        }

        private void Menu_Region_click(object sender, EventArgs e) {
            SetSidePanel(new OnTopReplica.SidePanels.RegionPanel());
        }

        private void Menu_Reduce_click(object sender, EventArgs e) {
            //Hide all panels in a platform specific way
            HideAllPanels();
        }

        private void Menu_AddPanel_click(object sender, EventArgs e) {
            //Spawn an additional panel window bound to the primary panel's source window.
            //Region and color alert settings stay independent per panel.
            CreateChildPanel();
        }

        private void Menu_Chrome_click(object sender, EventArgs e) {
            IsChromeVisible = !IsChromeVisible;
        }

        private void Menu_Settings_click(object sender, EventArgs e) {
            this.SetSidePanel(new OptionsPanel());
        }

        private void Menu_About_click(object sender, EventArgs e) {
            this.SetSidePanel(new AboutPanel());
        }

        private void Menu_ColorAlert_click(object sender, EventArgs e) {
            this.SetSidePanel(new ColorAlertPanel());
        }

        private void Menu_ColorAlertPauseResumeAll_click(object sender, EventArgs e) {
            SetColorAlertPausedAllPanels(!IsColorAlertPausedAllPanels);
        }

        private void Menu_Close_click(object sender, EventArgs e) {
            this.Close();
        }

        private void Menu_Exit_click(object sender, EventArgs e) {
            ExitApplication();
        }

    }
}
