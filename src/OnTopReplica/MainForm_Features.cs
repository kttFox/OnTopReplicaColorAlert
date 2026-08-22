using OnTopReplica.Native;
using OnTopReplica.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsAero.TaskDialog;

namespace OnTopReplica {
    //Contains some feature implementations of MainForm
    partial class MainForm {

        #region Click-through

        bool _clickThrough = false;

        readonly Color DefaultNonClickTransparencyKey;

        public bool ClickThroughEnabled {
            get {
                return _clickThrough;
            }
            set {
                if (value && Settings.Default.FirstTimeClickThrough) {
                    if (!ConfirmFirstTimeClickThrough())
                        return;
                }

                TransparencyKey = (value) ? Color.Black : DefaultNonClickTransparencyKey;
                if (value) {
                    //Re-force as top most (always helps in some cases)
                    TopMost = false;
                    this.Activate();
                    TopMost = true;
                }

                _clickThrough = value;
            }
        }

        /// <summary>
        /// Shows the one-time click-through confirmation dialog (if not already shown/dismissed)
        /// and persists that it has been shown. Returns false if the user declined.
        /// </summary>
        bool ConfirmFirstTimeClickThrough() {
            if (!Settings.Default.FirstTimeClickThrough)
                return true;

            TaskDialog dlg = new TaskDialog(Strings.InfoClickThrough, Strings.InfoClickThroughTitle, Strings.InfoClickThroughContent) {
                CommonButtons = CommonButton.Yes | CommonButton.No
            };
            if (dlg.Show(this).CommonButton == CommonButtonResult.No)
                return false;

            Settings.Default.FirstTimeClickThrough = false;
            return true;
        }

        #endregion

        #region Chrome

        readonly FormBorderStyle DefaultBorderStyle; // = FormBorderStyle.Sizable; // FormBorderStyle.SizableToolWindow;

        public bool IsChromeVisible {
            get {
                return (FormBorderStyle == DefaultBorderStyle);
            }
            set {
                //No-op when unchanged (the location shift below must not run twice)
                if (value == IsChromeVisible)
                    return;

                //Cancel hiding chrome if no thumbnail is shown
                if (!value && !ThumbnailPanel.IsShowingThumbnail)
                    return;

                if (!value) {
                    Location = new Point {
                        X = Location.X + SystemInformation.FrameBorderSize.Width,
                        Y = Location.Y + SystemInformation.FrameBorderSize.Height
                    };
                    FormBorderStyle = FormBorderStyle.None;
                }
                else if(value) {
                    Location = new Point {
                        X = Location.X - SystemInformation.FrameBorderSize.Width,
                        Y = Location.Y - SystemInformation.FrameBorderSize.Height
                    };
                    FormBorderStyle = DefaultBorderStyle;
                }

                Program.Platform.OnFormStateChange(this);
                Invalidate();

                //Chrome visibility is part of the persisted panel layout
                NotifyPanelLayoutChanged();
            }
        }

        #endregion

    }
}
