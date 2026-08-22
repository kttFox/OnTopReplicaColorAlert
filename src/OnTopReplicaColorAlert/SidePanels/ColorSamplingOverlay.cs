using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplicaColorAlert.SidePanels {

    /// <summary>
    /// Invisible, full-screen, always-on-top overlay used to capture a single
    /// mouse click anywhere on the virtual screen. Used by <see cref="ColorAlertPanel"/>
    /// to let the user pick a custom color by clicking any point on screen instead
    /// of always sampling the color underneath the "Sample" button itself.
    /// </summary>
    /// <remarks>
    /// The whole virtual screen is captured once when the overlay loads (before any
    /// overlay pixel is painted on screen), so the reported color is not affected by
    /// the overlay's own (near-transparent) surface or by timing of the click.
    /// </remarks>
    class ColorSamplingOverlay : Form {

        Bitmap _screenshot;
        readonly Rectangle _virtualScreen;
        ColorPreviewWindow _preview;

        public ColorSamplingOverlay() {
            _virtualScreen = SystemInformation.VirtualScreen;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Bounds = _virtualScreen;
            TopMost = true;
            BackColor = Color.Black;
            Opacity = 0.01;
            Cursor = Cursors.Cross;
            KeyPreview = true;
        }

        /// <summary>
        /// Raised when the user left-clicks on the overlay to pick a screen location.
        /// </summary>
        public event EventHandler<ColorPickedEventArgs> Picked;

        /// <summary>
        /// Raised when sampling is cancelled (right-click, Escape key, or screen capture failure).
        /// </summary>
        public event EventHandler Cancelled;

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            if (!TryCaptureScreen(out _screenshot)) {
                OnCancelled();
                return;
            }

            //Live preview of the color under the cursor, following the mouse
            _preview = new ColorPreviewWindow();
            _preview.Show();
            UpdatePreview(PointToClient(Cursor.Position));
        }

        /// <summary>
        /// Updates the follow-cursor preview with the color at the given overlay
        /// client location (which maps 1:1 onto the captured screenshot).
        /// </summary>
        private void UpdatePreview(Point clientLocation) {
            if (_preview == null || _screenshot == null)
                return;
            if (clientLocation.X < 0 || clientLocation.Y < 0 ||
                clientLocation.X >= _screenshot.Width || clientLocation.Y >= _screenshot.Height)
                return;

            var color = _screenshot.GetPixel(clientLocation.X, clientLocation.Y);
            _preview.UpdatePreview(color, PointToScreen(clientLocation), _virtualScreen);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            UpdatePreview(e.Location);
        }

        private bool TryCaptureScreen(out Bitmap screenshot) {
            var bitmap = new Bitmap(_virtualScreen.Width, _virtualScreen.Height);
            try {
                using (var graphics = Graphics.FromImage(bitmap)) {
                    graphics.CopyFromScreen(_virtualScreen.Location, Point.Empty, _virtualScreen.Size);
                }
            }
            catch (ArgumentException) {
                bitmap.Dispose();
                screenshot = null;
                return false;
            }
            catch (Win32Exception) {
                bitmap.Dispose();
                screenshot = null;
                return false;
            }

            screenshot = bitmap;
            return true;
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left) {
                OnCancelled();
                return;
            }

            if (_screenshot == null || e.X < 0 || e.Y < 0 || e.X >= _screenshot.Width || e.Y >= _screenshot.Height) {
                OnCancelled();
                return;
            }

            var color = _screenshot.GetPixel(e.X, e.Y);
            var evt = Picked;
            if (evt != null) evt(this, new ColorPickedEventArgs(color));
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape) {
                OnCancelled();
            }
        }

        private void OnCancelled() {
            var evt = Cancelled;
            if (evt != null) evt(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (_screenshot != null) {
                    _screenshot.Dispose();
                    _screenshot = null;
                }
                if (_preview != null) {
                    _preview.Dispose();
                    _preview = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Small always-on-top window following the cursor that previews the color
        /// currently under it (swatch + hex/RGB values). Click-through and
        /// non-activating, so it never interferes with the pick click.
        /// </summary>
        class ColorPreviewWindow : Form {

            const int CursorOffset = 24;

            readonly Panel _swatch;
            readonly Label _label;

            public ColorPreviewWindow() {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                TopMost = true;
                Size = new Size(150, 40);
                BackColor = SystemColors.Info;

                _swatch = new Panel {
                    Location = new Point(6, 6),
                    Size = new Size(28, 28),
                    BorderStyle = BorderStyle.FixedSingle
                };
                _label = new Label {
                    Location = new Point(40, 4),
                    Size = new Size(106, 32),
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = SystemColors.InfoText
                };
                Controls.Add(_swatch);
                Controls.Add(_label);
            }

            protected override bool ShowWithoutActivation {
                get { return true; }
            }

            protected override CreateParams CreateParams {
                get {
                    const int WS_EX_TRANSPARENT = 0x00000020;
                    const int WS_EX_TOOLWINDOW = 0x00000080;
                    const int WS_EX_NOACTIVATE = 0x08000000;
                    var cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);
                //Thin border so the tooltip-like window stands out on any background
                e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, 0, 0, Width - 1, Height - 1);
            }

            /// <summary>
            /// Shows the given color and moves the window next to the cursor,
            /// flipping to the other side near the screen edges.
            /// </summary>
            public void UpdatePreview(Color color, Point cursorScreenPos, Rectangle screenBounds) {
                _swatch.BackColor = color;
                _label.Text = string.Format("#{0:X2}{1:X2}{2:X2}\nR:{0} G:{1} B:{2}", color.R, color.G, color.B);

                int x = cursorScreenPos.X + CursorOffset;
                int y = cursorScreenPos.Y + CursorOffset;
                if (x + Width > screenBounds.Right)
                    x = cursorScreenPos.X - CursorOffset - Width;
                if (y + Height > screenBounds.Bottom)
                    y = cursorScreenPos.Y - CursorOffset - Height;
                Location = new Point(x, y);
            }
        }

    }
}
