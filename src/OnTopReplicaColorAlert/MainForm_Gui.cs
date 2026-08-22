using System.Drawing;
using System.Windows.Forms;
using WindowsFormsAero.TaskDialog;

namespace OnTopReplicaColorAlert {
    partial class MainForm {

        /// <summary>
        /// Opens the context menu.
        /// </summary>
        /// <param name="position">Optional position of the mouse, relative to which the menu is shown.</param>
        public void OpenContextMenu(Point? position) {
            Point menuPosition = MousePosition;
            if (position.HasValue)
                menuPosition = position.Value;

            var menu = menuContext;
            //メニューを閉じて別ウィンドウがアクティブになると、パネルが最前面バンドの
            //背面に落ちることがあるため、閉じたタイミングで TopMost を再主張する。
            //(重複登録を避けるため一旦解除してから登録)
            menu.Closed -= ContextMenu_Closed;
            menu.Closed += ContextMenu_Closed;
            menu.Show(menuPosition);
        }

        private void ContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ReassertTopMost();
        }

        /// <summary>
        /// フォーカスを奪わずに最前面(TOPMOST バンドの先頭)へ復帰させる。
        /// </summary>
        internal void ReassertTopMost() {
            if (IsDisposed || !IsHandleCreated || !TopMost)
                return;

            Native.WindowManagerMethods.SetWindowPos(Handle,
                Native.WindowManagerMethods.HWND_TOPMOST, 0, 0, 0, 0,
                Native.WindowManagerMethods.SWP_NOMOVE | Native.WindowManagerMethods.SWP_NOSIZE |
                Native.WindowManagerMethods.SWP_NOACTIVATE);
            UpdateColorAlertIndicator();
        }

        /// <summary>
        /// Gets the window's vertical chrome size.
        /// </summary>
        public int ChromeBorderVertical {
            get {
                if (IsChromeVisible)
                    return SystemInformation.FrameBorderSize.Height;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the window's horizontal chrome size.
        /// </summary>
        public int ChromeBorderHorizontal {
            get {
                if (IsChromeVisible)
                    return SystemInformation.FrameBorderSize.Width;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Displays an error task dialog.
        /// </summary>
        /// <param name="mainInstruction">Main instruction of the error dialog.</param>
        /// <param name="explanation">Detailed informations about the error.</param>
        /// <param name="errorMessage">Expanded error codes/messages.</param>
        private void ShowErrorDialog(string mainInstruction, string explanation, string errorMessage) {
            TaskDialog dlg = new TaskDialog(mainInstruction, Strings.ErrorGenericTitle, explanation) {
                CommonIcon = CommonIcon.Stop,
                IsExpanded = false
            };

            if (!string.IsNullOrEmpty(errorMessage)) {
                dlg.ExpandedInformation = Strings.ErrorGenericInfoText + errorMessage;
                dlg.ExpandedControlText = Strings.ErrorGenericInfoButton;
            }

            dlg.Show(this);
        }

        /// <summary>
        /// Ensures that the main form is visible (reactivating from task icon).
        /// </summary>
        public void EnsureMainFormVisible() {
            //Restore all panels in a platform-dependent method
            RestoreAllPanels();
        }

    }
}
