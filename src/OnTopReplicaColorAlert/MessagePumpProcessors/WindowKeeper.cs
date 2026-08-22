using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using OnTopReplicaColorAlert.Native;

namespace OnTopReplicaColorAlert.MessagePumpProcessors {
    
    /// <summary>
    /// Listens for shell events and closes the thumbnail if a cloned window is destroyed.
    /// </summary>
    class WindowKeeper : BaseMessagePumpProcessor {

        public override bool Process(ref Message msg) {
            if (Form.CurrentThumbnailWindowHandle != null &&
                msg.Msg == HookMethods.WM_SHELLHOOKMESSAGE) {
                int hookCode = msg.WParam.ToInt32();

                if (hookCode == HookMethods.HSHELL_WINDOWDESTROYED) {
                    //Check whether the destroyed window is the one we were cloning
                    IntPtr destroyedHandle = msg.LParam;
                    if (destroyedHandle == Form.CurrentThumbnailWindowHandle.Handle) {
                        //Disable cloning (対象ウィンドウ終了として通知)
                        Form.UnsetThumbnail(true);
                    }
                }
            }

            return false;
        }

        protected override void Shutdown() {
            
        }
    }

}
