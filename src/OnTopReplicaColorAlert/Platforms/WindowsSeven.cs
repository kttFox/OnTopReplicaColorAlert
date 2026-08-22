using System;
using System.Windows.Forms;
using OnTopReplicaColorAlert.Native;
using WindowsFormsAero.Dwm;

namespace OnTopReplicaColorAlert.Platforms {

    class WindowsSeven : WindowsVista {

        //Opacity is stored per form: multiple panel windows can be hidden at once
        private readonly System.Collections.Generic.Dictionary<MainForm, double> _previousOpacity =
            new System.Collections.Generic.Dictionary<MainForm, double>();

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
            if (form.Opacity > 0.0) {
                _previousOpacity[form] = form.Opacity;
            }
            form.Opacity = 0;
        }

        public override bool IsHidden(MainForm form) {
            return (form.Opacity == 0.0);
        }

        public override double GetVisibleOpacity(MainForm form) {
            if (form.Opacity == 0.0) {
                double previous;
                return _previousOpacity.TryGetValue(form, out previous) ? previous : 1.0;
            }
            return form.Opacity;
        }

        public override void RestoreForm(MainForm form) {
            if (form.Opacity == 0.0) {
                double previous;
                form.Opacity = _previousOpacity.TryGetValue(form, out previous) ? previous : 1.0;
                _previousOpacity.Remove(form);
            }

            form.Show();
        }

    }

}
