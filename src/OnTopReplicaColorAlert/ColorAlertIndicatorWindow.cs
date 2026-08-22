using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OnTopReplicaColorAlert {

    /// <summary>
    /// カラーアラート実行中を示す小さな●インジケーター。
    /// DWM サムネイルの上には同一ウィンドウ内で描画できないため、
    /// オーナー付き(TopMost ではない)の極小ウィンドウとしてメインウィンドウの
    /// 右上に重ねて表示する。クリック透過・非アクティブ化。
    /// ピクセル単位アルファのレイヤードウィンドウ(UpdateLayeredWindow)で描画するため、
    /// アンチエイリアスされた縁がフチ色を出さずに背景へ滑らかに合成される。
    /// </summary>
    class ColorAlertIndicatorWindow : Form {

        const int EdgeMargin = 4; // パネル右上角からの内側マージン

        int _dotSize = 6; // ●の直径(ピクセル)

        /// <summary>
        /// ●の直径(ピクセル)。設定パネルから変更可能。
        /// </summary>
        public int DotSize {
            get { return _dotSize; }
            set {
                value = Math.Max(2, Math.Min(64, value));
                if (_dotSize == value) return;
                _dotSize = value;
                Size = new Size(_dotSize, _dotSize);
                RenderDot();
            }
        }

        public ColorAlertIndicatorWindow() {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(_dotSize, _dotSize);
        }

        /// <summary>
        /// 表示してもフォーカスを奪わない。
        /// </summary>
        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        protected override CreateParams CreateParams {
            get {
                const int WS_EX_LAYERED = 0x00080000;     //ピクセル単位アルファ描画
                const int WS_EX_TRANSPARENT = 0x00000020; //クリック透過
                const int WS_EX_TOOLWINDOW = 0x00000080;  //Alt+Tab に出さない
                const int WS_EX_NOACTIVATE = 0x08000000;  //アクティブ化しない
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                return cp;
            }
        }

        Color _dotColor = Color.Red;

        /// <summary>
        /// ●の色。実行中は赤、停止中はグリーン。
        /// </summary>
        public Color DotColor {
            get { return _dotColor; }
            set {
                if (_dotColor == value) return;
                _dotColor = value;
                RenderDot();
            }
        }

        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            RenderDot();
        }

        /// <summary>
        /// アンチエイリアスした●を 32bpp ARGB ビットマップに描画し、
        /// UpdateLayeredWindow でウィンドウ表面へ転送する。
        /// </summary>
        private void RenderDot() {
            if (!IsHandleCreated || IsDisposed)
                return;

            using (var bmp = new Bitmap(_dotSize, _dotSize, PixelFormat.Format32bppArgb)) {
                using (var g = Graphics.FromImage(bmp)) {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var brush = new SolidBrush(_dotColor)) {
                        g.FillEllipse(brush, 0, 0, _dotSize - 1, _dotSize - 1);
                    }
                }
                SetLayeredBitmap(bmp);
            }
        }

        private void SetLayeredBitmap(Bitmap bmp) {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;
            try {
                hBitmap = bmp.GetHbitmap(Color.FromArgb(0)); //事前乗算アルファ付き HBITMAP
                oldBitmap = SelectObject(memDc, hBitmap);

                var size = new NativeSize(bmp.Width, bmp.Height);
                var srcPos = new NativePoint(0, 0);
                var winPos = new NativePoint(Left, Top);
                var blend = new BLENDFUNCTION {
                    BlendOp = 0,             //AC_SRC_OVER
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = 1          //AC_SRC_ALPHA (ピクセル単位アルファ)
                };

                UpdateLayeredWindow(Handle, screenDc, ref winPos, ref size, memDc, ref srcPos, 0, ref blend, 2 /* ULW_ALPHA */);
            }
            finally {
                if (oldBitmap != IntPtr.Zero) SelectObject(memDc, oldBitmap);
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                DeleteDC(memDc);
                ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        /// <summary>
        /// Z オーダー上でこのウィンドウをオーナーの直上に再配置する。
        /// オーナーが TopMost の場合などに、このウィンドウがオーナーの下へ
        /// 潜ってしまい●が隠れるのを防ぐ。
        /// </summary>
        public void EnsureAbove(Form owner) {
            if (owner == null || owner.IsDisposed || !owner.IsHandleCreated || !IsHandleCreated)
                return;

            //「オーナーの1つ上のウィンドウ」の後ろ = オーナーの直上 に挿入する
            IntPtr insertAfter = GetWindow(owner.Handle, GW_HWNDPREV);
            if (insertAfter == Handle)
                return; //既にオーナーの直上にいる
            if (insertAfter == IntPtr.Zero)
                insertAfter = HWND_TOP; //オーナーが最上位: 同じバンドの先頭へ

            SetWindowPos(Handle, insertAfter, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        /// <summary>
        /// オーナーウィンドウのクライアント領域右上に位置を合わせる。
        /// </summary>
        public void UpdatePosition(Form owner) {
            if (owner == null || owner.IsDisposed || !owner.IsHandleCreated)
                return;

            var topRight = owner.PointToScreen(new Point(owner.ClientSize.Width, 0));
            Location = new Point(topRight.X - _dotSize - EdgeMargin, topRight.Y + EdgeMargin);
        }

        #region Win32 interop

        const int GW_HWNDPREV = 3;
        static readonly IntPtr HWND_TOP = IntPtr.Zero;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        struct NativePoint { public int X, Y; public NativePoint(int x, int y) { X = x; Y = y; } }

        [StructLayout(LayoutKind.Sequential)]
        struct NativeSize { public int Width, Height; public NativeSize(int w, int h) { Width = w; Height = h; } }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BLENDFUNCTION {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref NativePoint pptDst, ref NativeSize psize,
            IntPtr hdcSrc, ref NativePoint pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr hObject);

        #endregion
    }
}
