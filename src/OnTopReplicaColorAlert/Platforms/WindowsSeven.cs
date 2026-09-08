using System;
using System.Windows.Forms;
using OnTopReplicaColorAlert.Native;
using WindowsFormsAero.Dwm;

namespace OnTopReplicaColorAlert.Platforms {

    class WindowsSeven : WindowsVista {

        public override void PreHandleFormInit() {
            //Set Application ID
            WindowsSevenMethods.SetCurrentProcessExplicitAppUserModelID("LorenzCunoKlopfenstein.OnTopReplicaColorAlert.MainForm");
        }

        public override void PostHandleFormInit(MainForm form) {
            DwmManager.SetWindowFlip3dPolicy(form, WindowsFormsAero.Flip3DPolicy.ExcludeAbove);
            DwmManager.SetExcludeFromPeek(form, true);
            DwmManager.SetDisallowPeek(form, true);
        }

        public override void HideForm(MainForm form) {
            //表示時の透明度はフォーム側 (MainForm.VisibleOpacity) が保持するため、
            //ここでは見た目だけを消す。
            form.Opacity = 0;
        }

        public override bool IsHidden(MainForm form) {
            return (form.Opacity == 0.0);
        }

        public override void RestoreForm(MainForm form) {
            if (form.Opacity == 0.0) {
                form.Opacity = form.VisibleOpacity;
            }

            form.Show();
        }

    }

}
