using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace OnTopReplicaColorAlert {
	public class CloseRequestEventArgs : EventArgs {

		public WindowHandle LastWindowHandle { get; set; }

        public Rectangle? LastRegion { get; set; }

	}
}
