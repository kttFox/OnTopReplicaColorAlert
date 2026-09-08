using OnTopReplicaColorAlert.Native;
using OnTopReplicaColorAlert.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsAero.TaskDialog;

namespace OnTopReplicaColorAlert {
    //Contains some feature implementations of MainForm
    partial class MainForm {

        #region Opacity

        /// <summary>
        /// 表示時の透明度の下限。0 まで下げると操作不能になるためクランプする。
        /// </summary>
        public const double MinVisibleOpacity = 0.1;

        double _visibleOpacity = 1.0;

        /// <summary>
        /// パネルが表示されているときの透明度。Windows 7 以降の非表示実装は
        /// Opacity=0 でパネルを隠すため、Form.Opacity だけでは「隠れている 0」と
        /// ユーザーが選んだ値を区別できない(終了時などに opacity=0 が保存され、
        /// 次回起動で 10% に復元されてしまう)。保存・復元・復帰にはこの値を使う。
        /// 非表示中の代入は見た目を変えず、復帰時に反映される。
        /// </summary>
        public double VisibleOpacity {
            get { return _visibleOpacity; }
            set {
                _visibleOpacity = Math.Max(MinVisibleOpacity, Math.Min(1.0, value));
                if (!Program.Platform.IsHidden(this)) {
                    Opacity = _visibleOpacity;
                }
            }
        }

        #endregion

        #region Visible geometry

        //最小化中の Location は (-32000, -32000)、ClientSize も最小化枠の値になる。
        //レイアウトには「通常表示だったときの位置・サイズ」を保存したいので、
        //通常表示中の値をここで追跡しておく。
        Point _visibleLocation;
        Size _visibleClientSize;
        bool _visibleGeometryValid;

        /// <summary>
        /// 通常表示中(最小化・最大化していない状態)の位置。
        /// 最小化中は追跡済みの値を返す。
        /// </summary>
        public Point VisibleLocation {
            get { return _visibleGeometryValid ? _visibleLocation : Location; }
        }

        /// <summary>
        /// 通常表示中(最小化・最大化していない状態)のクライアントサイズ。
        /// 最小化中は追跡済みの値を返す。
        /// </summary>
        public Size VisibleClientSize {
            get { return _visibleGeometryValid ? _visibleClientSize : ClientSize; }
        }

        /// <summary>
        /// 通常表示中なら現在の位置・サイズを「表示時の値」として記録する。
        /// 位置・サイズが変わるたびに呼ぶこと。
        /// </summary>
        void TrackVisibleGeometry() {
            if (IsDisposed || Disposing)
                return;
            if (WindowState != FormWindowState.Normal)
                return;
            //最小化アニメーション中などに現れる画面外座標は記録しない
            if (Location.X <= -30000 || Location.Y <= -30000)
                return;
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            _visibleLocation = Location;
            _visibleClientSize = ClientSize;
            _visibleGeometryValid = true;
        }

        /// <summary>
        /// レイアウト復元時に適用した位置・サイズを「表示時の値」として直接記録する。
        /// 復元直後に非表示・最小化された場合でも、正しい値を保存し続けられるようにする。
        /// </summary>
        internal void SeedVisibleGeometry(Point location, Size clientSize) {
            _visibleLocation = location;
            _visibleClientSize = clientSize;
            _visibleGeometryValid = true;
        }

        #endregion

        #region Click-through

        bool _clickThrough = false;

        readonly Color DefaultNonClickTransparencyKey;

        public bool ClickThroughEnabled {
            get {
                return _clickThrough;
            }
            set {
                if (value && Settings.Default.FirstTimeClickThrough) {
                    if (!ConfirmFirstTimeClickThrough())
                        return;
                }

                TransparencyKey = (value) ? Color.Black : DefaultNonClickTransparencyKey;
                if (value && CurrentThumbnailWindowHandle != null) {
                    //Re-force as top most (always helps in some cases)
                    TopMost = false;
                    this.Activate();
                    TopMost = true;
                }

                _clickThrough = value;
            }
        }

        /// <summary>
        /// Shows the one-time click-through confirmation dialog (if not already shown/dismissed)
        /// and persists that it has been shown. Returns false if the user declined.
        /// </summary>
        bool ConfirmFirstTimeClickThrough() {
            if (!Settings.Default.FirstTimeClickThrough)
                return true;

            TaskDialog dlg = new TaskDialog(Strings.InfoClickThrough, Strings.InfoClickThroughTitle, Strings.InfoClickThroughContent) {
                CommonButtons = CommonButton.Yes | CommonButton.No
            };
            if (dlg.Show(this).CommonButton == CommonButtonResult.No)
                return false;

            Settings.Default.FirstTimeClickThrough = false;
            return true;
        }

        #endregion

        #region Chrome

        readonly FormBorderStyle DefaultBorderStyle; // = FormBorderStyle.Sizable; // FormBorderStyle.SizableToolWindow;

        public bool IsChromeVisible {
            get {
                return (FormBorderStyle == DefaultBorderStyle);
            }
            set {
                //No-op when unchanged (the location shift below must not run twice)
                if (value == IsChromeVisible)
                    return;

                //Cancel hiding chrome if no thumbnail is shown
                if (!value && !ThumbnailPanel.IsShowingThumbnail)
                    return;

                if (!value) {
                    Location = new Point {
                        X = Location.X + SystemInformation.FrameBorderSize.Width,
                        Y = Location.Y + SystemInformation.FrameBorderSize.Height
                    };
                    FormBorderStyle = FormBorderStyle.None;
                }
                else if(value) {
                    Location = new Point {
                        X = Location.X - SystemInformation.FrameBorderSize.Width,
                        Y = Location.Y - SystemInformation.FrameBorderSize.Height
                    };
                    FormBorderStyle = DefaultBorderStyle;
                }

                Program.Platform.OnFormStateChange(this);
                Invalidate();

                //Chrome visibility is part of the persisted panel layout
                NotifyPanelLayoutChanged();
            }
        }

        #endregion

    }
}
