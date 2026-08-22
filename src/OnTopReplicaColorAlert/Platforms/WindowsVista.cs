using System;
using System.Windows.Forms;
using WindowsFormsAero.Dwm;

namespace OnTopReplicaColorAlert.Platforms {

    class WindowsVista : PlatformSupport {
        
        public override bool CheckCompatibility() {
            if (!WindowsFormsAero.OsSupport.IsCompositionEnabled) {
                MessageBox.Show(Strings.ErrorDwmOffContent, Strings.ErrorDwmOff, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        public override void PostHandleFormInit(MainForm form) {
            //Do not show in task bar (the notification icon is now installed
            //globally by Program.Main on every platform)
            //NOTE: this effectively makes Windows ignore the Flip 3D policy set above (on Windows 7)
            form.ShowInTaskbar = false;

            DwmManager.SetWindowFlip3dPolicy(form, WindowsFormsAero.Flip3DPolicy.ExcludeAbove);
        }

        public override bool IsHidden(MainForm form) {
            return !form.Visible;
        }

        public override void HideForm(MainForm form) {
            form.Hide();
        }

        public override void RestoreForm(MainForm form) {
            form.Show();
        }

    }

}
