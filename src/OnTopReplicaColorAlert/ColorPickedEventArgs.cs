using System;
using System.Drawing;

namespace OnTopReplicaColorAlert {
    /// <summary>
    /// EventArgs structure for a color picked from the screen (e.g. via <see cref="SidePanels.ColorSamplingOverlay"/>).
    /// </summary>
    public class ColorPickedEventArgs : EventArgs {

        public Color Color { get; private set; }

        public ColorPickedEventArgs(Color color) {
            Color = color;
        }

    }
}
