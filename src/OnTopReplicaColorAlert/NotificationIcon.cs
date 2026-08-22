using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using OnTopReplicaColorAlert.Properties;

namespace OnTopReplicaColorAlert {
    /// <summary>
    /// Notification icon that installs itself in the "tray" and manipulates the
    /// OnTopReplicaColorAlert panels. The target form is resolved dynamically so the icon
    /// keeps working when the primary panel changes (promotion on close).
    /// </summary>
    class NotificationIcon : IDisposable {

        public NotificationIcon() {
            Install();
        }

        /// <summary>
        /// Gets the current primary panel (may change over the icon's lifetime).
        /// </summary>
        static MainForm Form => MainForm.CurrentPrimary;

        NotifyIcon _taskIcon;
        ContextMenuStrip _contextMenu;
        ToolStripMenuItem _windowsItem;
        ToolStripMenuItem _colorAlertPauseItem;

        private void Install() {
            _windowsItem = new ToolStripMenuItem(Strings.MenuWindows, Resources.list) {
                ToolTipText = Strings.MenuWindowsTT
            };

            _colorAlertPauseItem = new ToolStripMenuItem(Strings.MenuPauseColorAlertAll, null, TaskIconColorAlertPauseResume_click) {
                ToolTipText = Strings.MenuPauseColorAlertAllTT
            };

            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem(Strings.MenuOpen, Resources.icon, TaskIconOpen_click) {
                    ToolTipText = Strings.MenuOpenTT,
                },
                _windowsItem,
                new ToolStripMenuItem(Strings.MenuDisableClickThroughAll, null, TaskIconDisableClickThrough_click) {
                    ToolTipText = Strings.MenuDisableClickThroughAllTT
                },
                _colorAlertPauseItem,
                new ToolStripMenuItem(Strings.MenuExit, Resources.close_new, TaskIconExit_click){
                    ToolTipText = Strings.MenuExitTT
                }
            });
            //The window list menu belongs to the primary panel: rebind on every
            //opening since the primary may have changed
            _contextMenu.Opening += (sender, e) => {
                _windowsItem.DropDown = Form?.MenuWindows;
                _windowsItem.Enabled = Form != null;

                //カラーアラートの停止状態に応じて表記を切り替える
                bool alertPaused = Form != null && Form.IsColorAlertPausedAllPanels;
                _colorAlertPauseItem.Text = alertPaused
                    ? Strings.MenuResumeColorAlertAll : Strings.MenuPauseColorAlertAll;
                _colorAlertPauseItem.ToolTipText = alertPaused
                    ? Strings.MenuResumeColorAlertAllTT : Strings.MenuPauseColorAlertAllTT;
                _colorAlertPauseItem.Enabled = Form != null;
            };
            Asztal.Szótár.NativeToolStripRenderer.SetToolStripRenderer(_contextMenu);

            _taskIcon = new NotifyIcon {
                Text = Strings.ApplicationName,
                Icon = Resources.new_flat_icon,
                Visible = true,
                ContextMenuStrip = _contextMenu
            };
            _taskIcon.DoubleClick += new EventHandler(TaskIcon_doubleclick);
        }

        #region IDisposable Members

        public void Dispose() {
            //Destroy NotifyIcon
            if (_taskIcon != null) {
                _taskIcon.Visible = false;
                _taskIcon.Dispose();
                _taskIcon = null;
            }
        }

        #endregion

        #region Task Icon events

        void TaskIcon_doubleclick(object sender, EventArgs e) {
            Form?.EnsureMainFormVisible();
        }

        private void TaskIconOpen_click(object sender, EventArgs e) {
            Form?.EnsureMainFormVisible();
        }

        private void TaskIconDisableClickThrough_click(object sender, EventArgs e) {
            Form?.DisableClickThroughAllPanels();
        }

        private void TaskIconColorAlertPauseResume_click(object sender, EventArgs e) {
            var form = Form;
            if (form == null) return;
            form.SetColorAlertPausedAllPanels(!form.IsColorAlertPausedAllPanels);
        }

        private void TaskIconExit_click(object sender, EventArgs e) {
            Form?.ExitApplication();
        }

        #endregion

    }
}
