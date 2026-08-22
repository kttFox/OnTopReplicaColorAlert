using System;
using System.Collections.Generic;
using System.Text;
using OnTopReplicaColorAlert.Native;
using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplicaColorAlert {
	public static class Win32Helper {

        /// <summary>
        /// Gets a handle to the window that currently is in the foreground.
        /// </summary>
        /// <returns>May return null if call fails or no valid window selected.</returns>
        public static WindowHandle GetCurrentForegroundWindow() {
            IntPtr handle = WindowManagerMethods.GetForegroundWindow();
            if (handle == IntPtr.Zero)
                return null;

            return new WindowHandle(handle);
        }

	}
}
