using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace OnTopReplicaColorAlert.Platforms {
    class Other : PlatformSupport {
        
        public override bool CheckCompatibility() {
            MessageBox.Show(Strings.ErrorNoDwm, Strings.ErrorNoDwmTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

    }
}
