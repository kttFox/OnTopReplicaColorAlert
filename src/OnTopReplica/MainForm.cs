using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OnTopReplica.Native;
using OnTopReplica.Properties;
using OnTopReplica.StartupOptions;
using OnTopReplica.WindowSeekers;
using WindowsFormsAero.Dwm;
using WindowsFormsAero.TaskDialog;

namespace OnTopReplica {

    partial class MainForm : AspectRatioForm {

        //GUI elements

        //Multi-panel support: secondary panels always clone the primary's source window
        MainForm _primaryPanel; //null on the primary panel itself
        readonly List<MainForm> _childPanels = new List<MainForm>();
        //True while restoring or shutting down (primary only). Starts true so that
        //startup option side effects (e.g. chrome changes) cannot overwrite the
        //layout file before it has been restored; cleared at the end of OnShown.
        bool _suppressLayoutSave = true;
        internal static bool ApplicationExiting; //set when "Exit program" terminates all panels

        //Registry of all open panel windows. Do NOT use Application.OpenForms for
        //lifetime tracking: forms silently disappear from it when their handle is
        //recreated (e.g. on ShowInTaskbar changes during primary promotion).
        static readonly List<MainForm> _openPanels = new List<MainForm>();

        /// <summary>
        /// Saves the panel layout immediately. Call whenever a layout-relevant
        /// setting changes (panels added/removed/moved, region, source window,
        /// color alert settings). No-op while restoring or shutting down.
        /// </summary>
        public void NotifyPanelLayoutChanged() {
            var primary = _primaryPanel ?? this;
            if (primary._suppressLayoutSave || primary.IsDisposed)
                return;
            PanelLayoutManager.Save(primary, primary._childPanels);
        }

        /// <summary>
        /// Gets whether this window is a secondary panel added via "Add panel".
        /// Secondary panels follow the primary panel's cloned window.
        /// </summary>
        public bool IsSecondaryPanel => _primaryPanel != null;

        /// <summary>
        /// Gets the secondary panels bound to this (primary) panel.
        /// </summary>
        internal IEnumerable<MainForm> ChildPanels => _childPanels;

        /// <summary>
        /// Gets the primary panel of this window's panel set (itself if primary).
        /// </summary>
        internal MainForm PrimaryPanel => _primaryPanel ?? this;

        /// <summary>
        /// Gets the current primary panel, resolving promotions. Null when no
        /// panel is open (i.e. during shutdown).
        /// </summary>
        internal static MainForm CurrentPrimary => _openPanels.FirstOrDefault(panel => !panel.IsDisposed && !panel.IsSecondaryPanel);

        /// <summary>
        /// Enables click-through mode on the whole panel set.
        /// </summary>
        public void EnableClickThroughAllPanels() {
            var primary = _primaryPanel ?? this;

            //Confirm (at most) once for the whole panel set, instead of once per panel
            if (!primary.ConfirmFirstTimeClickThrough())
                return;

            primary.ClickThroughEnabled = true;
            foreach (var child in primary._childPanels.Where(child => !child.IsDisposed)) {
                child.ClickThroughEnabled = true;
            }
        }

        /// <summary>
        /// Disables click-through mode on the whole panel set.
        /// </summary>
        public void DisableClickThroughAllPanels() {
            var primary = _primaryPanel ?? this;
            primary.ClickThroughEnabled = false;
            foreach (var child in primary._childPanels.Where(child => !child.IsDisposed)) {
                child.ClickThroughEnabled = false;
            }
        }

        /// <summary>
        /// Pauses or resumes color alert detection on the whole panel set.
        /// The Enabled setting of each panel is preserved.
        /// </summary>
        public void SetColorAlertPausedAllPanels(bool paused) {
            var primary = _primaryPanel ?? this;
            SetColorAlertPaused(primary, paused);
            foreach (var child in primary._childPanels.Where(child => !child.IsDisposed)) {
                SetColorAlertPaused(child, paused);
            }
            //停止状態もレイアウトファイルに永続化する
            NotifyPanelLayoutChanged();
        }

        /// <summary>
        /// Gets whether color alert detection is currently paused for the panel set
        /// (the primary panel's state is used as representative).
        /// </summary>
        public bool IsColorAlertPausedAllPanels {
            get {
                var primary = _primaryPanel ?? this;
                try {
                    return primary.MessagePumpManager.Get<MessagePumpProcessors.ColorDetectionProcessor>().Paused;
                }
                catch {
                    return false;
                }
            }
        }

        //対象ウィンドウ喪失によりカラーアラートを停止済みかどうか(プライマリのみ)。
        //喪失検出は複数経路(WindowKeeper のシェルフック、PanelLayoutManager のウォッチャ、
        //DWM エラー)から起こり得るため、多重停止・レイアウト保存の連打を防ぐ一度きりのガード。
        //対象ウィンドウが再アタッチされた時点でリセットする(自動開始はしない)。
        bool _colorAlertStoppedForLoss;

        /// <summary>
        /// 対象ウィンドウが失われたとき、設定が有効ならカラーアラート検出を停止する。
        /// 検出経路に依らず一元的にここで処理する。停止は一度きりで、
        /// 再アタッチ(<see cref="ResetColorAlertSourceLossState"/>)までは繰り返さない。
        /// </summary>
        public void StopColorAlertForSourceLoss() {
            var primary = _primaryPanel ?? this;
            if (primary.IsSecondaryPanel) return;
            if (!Properties.Settings.Default.PauseColorAlertWhenSourceLost) return;
            if (primary._colorAlertStoppedForLoss) return;
            primary._colorAlertStoppedForLoss = true;
            primary.SetColorAlertPausedAllPanels(true);
        }

        /// <summary>
        /// 対象ウィンドウ喪失の停止ガードをリセットする(再アタッチ時に呼ぶ)。
        /// 停止状態(Paused)自体は変更しない — 自動開始は行わない。
        /// </summary>
        public void ResetColorAlertSourceLossState() {
            (_primaryPanel ?? this)._colorAlertStoppedForLoss = false;
        }

        static void SetColorAlertPaused(MainForm panel, bool paused) {
            try {
                var proc = panel.MessagePumpManager.Get<MessagePumpProcessors.ColorDetectionProcessor>();
                proc.Paused = paused;
            }
            catch { }
            panel.UpdateColorAlertIndicator();
        }

        ColorAlertIndicatorWindow _colorAlertIndicator;

        /// <summary>
        /// 開いている全パネルの●インジケーターを更新する。
        /// 設定パネルでインジケーター関連の設定が変更されたときに呼び出す。
        /// </summary>
        public static void UpdateAllColorAlertIndicators() {
            foreach( var panel in _openPanels ) {
                panel.UpdateColorAlertIndicator();
            }
        }

        /// <summary>
        /// カラーアラートの実行状態に応じて、パネル右上の●インジケーターの
        /// 表示/非表示と位置を更新する。UI スレッドから呼び出すこと。
        /// </summary>
        public void UpdateColorAlertIndicator() {
            if (IsDisposed || !IsHandleCreated) return;
            if (!_managersInitialized) return; //ハンドル作成前の OnLocationChanged 等に備えるガード

            bool show;
            bool paused = false;
            try {
                var proc = MessagePumpManager.Get<MessagePumpProcessors.ColorDetectionProcessor>();
                show = proc.Enabled;
                paused = proc.Paused;
            }
            catch {
                show = false;
            }
            show &= Properties.Settings.Default.ColorAlertIndicatorEnabled;
            show &= Visible && WindowState != FormWindowState.Minimized;
            //Windows 7 以降の HideForm は Opacity=0 で隠すため Visible は true のまま。
            //プラットフォーム側の「非表示」状態も判定に含める。
            show &= !Program.Platform.IsHidden(this);

            if (show) {
                if (_colorAlertIndicator == null || _colorAlertIndicator.IsDisposed) {
                    _colorAlertIndicator = new ColorAlertIndicatorWindow {
                        Owner = this //オーナー付きにすることで TopMost なしで常にこのパネルの直上に保たれる
                    };
                }
                //実行中/停止中の色とサイズは設定から反映する
                _colorAlertIndicator.DotSize = Properties.Settings.Default.ColorAlertIndicatorSize;
                _colorAlertIndicator.DotColor = paused
                    ? Properties.Settings.Default.ColorAlertIndicatorPausedColor
                    : Properties.Settings.Default.ColorAlertIndicatorRunningColor;
                _colorAlertIndicator.UpdatePosition(this);
                if (!_colorAlertIndicator.Visible) {
                    _colorAlertIndicator.Show();
                }
                //オーナー(このパネル)が TopMost の場合、オーナー付きウィンドウが
                //TopMost バンドに入らず下に潜ることがあるため、常に状態を同期して
                //パネルの直上に保つ。
                if (_colorAlertIndicator.TopMost != TopMost) {
                    _colorAlertIndicator.TopMost = TopMost;
                }
                _colorAlertIndicator.EnsureAbove(this);
            }
            else if (_colorAlertIndicator != null && !_colorAlertIndicator.IsDisposed && _colorAlertIndicator.Visible) {
                _colorAlertIndicator.Hide();
            }
        }

        /// <summary>
        /// Closes all panels and terminates the application. Layout saving is
        /// suppressed first, so that panels closing during shutdown do not
        /// overwrite the stored layout with a partial state.
        /// </summary>
        public void ExitApplication() {
            ApplicationExiting = true;
            var primary = _primaryPanel ?? this;
            primary._suppressLayoutSave = true;
            Application.Exit();
        }

        /// <summary>
        /// Gets whether the given window handle belongs to one of the open panels.
        /// </summary>
        internal static bool IsPanelHandle(IntPtr handle) {
            foreach (var panel in _openPanels) {
                if (!panel.IsDisposed && panel.IsHandleCreated && panel.Handle == handle)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Hides all panel windows (primary and secondaries) at once.
        /// </summary>
        public void HideAllPanels() {
            var primary = _primaryPanel ?? this;
            primary._autoHidden = false; //manual hide supersedes auto hide
            Program.Platform.HideForm(primary);
            primary.UpdateColorAlertIndicator();
            foreach (var child in primary._childPanels) {
                Program.Platform.HideForm(child);
                //Opacity=0 で隠す実装では Visible 変更イベントが発火しないため明示的に更新する
                child.UpdateColorAlertIndicator();
            }
            //Deactivate: releases focus so the taskbar button is no longer highlighted
            primary.WindowState = FormWindowState.Minimized;
        }

        #region Auto hide on source deactivation

        //True while the panel set is hidden because the cloned window lost focus
        //(primary only). Distinct from manual hiding (taskbar icon).
        bool _autoHidden;

        /// <summary>
        /// 対象ウィンドウの非アクティブ化により、パネルセットが自動非表示中かどうか。
        /// </summary>
        internal bool IsAutoHidden {
            get { return (_primaryPanel ?? this)._autoHidden; }
        }

        /// <summary>
        /// 対象ウィンドウが非アクティブになったときにパネルセット全体を
        /// フォーカスを奪わずに非表示にする(AutoHideManager から呼ばれる)。
        /// 手動で非表示・最小化されている場合は何もしない。
        /// </summary>
        internal void AutoHideAllPanels() {
            var primary = _primaryPanel ?? this;
            if (primary._autoHidden || primary.IsDisposed) return;
            //手動非表示や最小化と競合しないよう、表示中のみ対象とする
            if (!primary.Visible || primary.WindowState == FormWindowState.Minimized) return;

            primary._autoHidden = true;
            foreach (var child in primary._childPanels) {
                SetPanelVisibleNoActivate(child, false);
            }
            //プライマリは SW_HIDE ではなくアクティブ化なしの最小化にする。
            //非表示にするとタスクバーボタンも消えてしまうため、最小化で
            //タスクバーボタンを常に残す(クリックで手動復帰も可能)。
            if (primary.IsHandleCreated) {
                WindowManagerMethods.ShowWindow(primary.Handle,
                    WindowManagerMethods.SW_SHOWMINNOACTIVE);
                primary.UpdateColorAlertIndicator();
            }
        }

        /// <summary>
        /// 対象ウィンドウが再度アクティブになったとき、自動非表示中の
        /// パネルセットをフォーカスを奪わずに再表示する。
        /// </summary>
        internal void AutoShowAllPanels() {
            var primary = _primaryPanel ?? this;
            if (!primary._autoHidden || primary.IsDisposed) return;

            //SW_SHOWNOACTIVATE は最小化状態から元のサイズ・位置に
            //アクティブ化なしで復元する
            SetPanelVisibleNoActivate(primary, true);
            foreach (var child in primary._childPanels) {
                SetPanelVisibleNoActivate(child, true);
            }
            primary._autoHidden = false; //OnSizeChanged のガードのため最後にクリア
        }

        //アクティブ化(フォーカス奪取)を伴わずにパネルの表示/非表示を切り替える
        static void SetPanelVisibleNoActivate(MainForm panel, bool visible) {
            if (panel.IsDisposed || !panel.IsHandleCreated) return;
            WindowManagerMethods.ShowWindow(panel.Handle,
                visible ? WindowManagerMethods.SW_SHOWNOACTIVATE : WindowManagerMethods.SW_HIDE);
            if (visible) {
                //Opacity=0(プラットフォームの非表示実装)のまま表示されると
                //見えないままになるため、必要なら不透明度も復元する
                //(表示済みフォームへの Show() は no-op なのでフォーカスは奪わない)
                if (Program.Platform.IsHidden(panel)) {
                    Program.Platform.RestoreForm(panel);
                }
                //SW_SHOWNOACTIVATE では TOPMOST バンド内の順序が保証されず、
                //クローン元ウィンドウの背面に残る場合があるため明示的に再前面化する
                if (panel.TopMost) {
                    WindowManagerMethods.SetWindowPos(panel.Handle,
                        WindowManagerMethods.HWND_TOPMOST, 0, 0, 0, 0,
                        WindowManagerMethods.SWP_NOMOVE | WindowManagerMethods.SWP_NOSIZE |
                        WindowManagerMethods.SWP_NOACTIVATE);
                }
            }
            panel.UpdateColorAlertIndicator();
        }

        #endregion

        bool _restoringAllPanels; //re-entrancy guard (Show() re-triggers OnActivated)

        /// <summary>
        /// Restores all panel windows (primary and secondaries) at once.
        /// </summary>
        public void RestoreAllPanels() {
            var primary = _primaryPanel ?? this;

            //Guard against re-entrancy and shutdown: RestoreForm calls Show(), which
            //raises OnActivated again and would recurse into this method endlessly
            //(stack overflow), especially while forms are being disposed.
            if (primary._restoringAllPanels || ApplicationExiting ||
                primary.IsDisposed || primary.Disposing)
                return;

            primary._restoringAllPanels = true;
            primary._autoHidden = false; //manual restore supersedes auto hide
            try {
                if (primary.WindowState == FormWindowState.Minimized) {
                    primary.WindowState = FormWindowState.Normal;
                }
                Program.Platform.RestoreForm(primary);
                primary.UpdateColorAlertIndicator();
                foreach (var child in primary._childPanels.ToArray()) {
                    if (!child.IsDisposed && !child.Disposing) {
                        Program.Platform.RestoreForm(child);
                        child.UpdateColorAlertIndicator();
                    }
                }
            }
            finally {
                primary._restoringAllPanels = false;
            }
        }

        /// <summary>
        /// Creates a secondary panel window bound to this panel's primary.
        /// The new panel clones the same source window as the primary and follows
        /// it when the primary switches to another window.
        /// </summary>
        public MainForm CreateChildPanel() {
            var primary = _primaryPanel ?? this;

            var form = new MainForm(new StartupOptions.Options());
            form._primaryPanel = primary;
            primary._childPanels.Add(form);
            form.FormClosed += (s, args) => {
                //Resolve the primary at close time: it may have changed if the
                //original primary was closed and a child was promoted.
                var p = form._primaryPanel;
                if (p != null) {
                    p._childPanels.Remove(form);
                    p.NotifyPanelLayoutChanged();
                }
            };

            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(Location.X + 40, Location.Y + 40);
            form.ShowInTaskbar = false; //only the primary panel appears in the taskbar
            form.Show();

            if (primary.CurrentThumbnailWindowHandle != null) {
                form.SetThumbnail(primary.CurrentThumbnailWindowHandle, null);
            }

            primary.NotifyPanelLayoutChanged();
            return form;
        }

        //Managers
        WindowListMenuManager _windowListManager;

        Options _startupOptions;

        public MainForm(Options startupOptions) {
            _startupOptions = startupOptions;

            _quickRegionDrawingHandler = HandleQuickRegionDrawn;
            
            //WinForms init pass
            InitializeComponent();

            //Store default values
            DefaultNonClickTransparencyKey = this.TransparencyKey;
            DefaultBorderStyle = this.FormBorderStyle;

            //Thumbnail panel
            ThumbnailPanel = new ThumbnailPanel {
                Location = Point.Empty,
                Dock = DockStyle.Fill
            };
            Controls.Add(ThumbnailPanel);

            //Populate opacity menu (100% down to 10%, in 10% steps)
            for (int pct = 100; pct >= 10; pct -= 10) {
                var item = new ToolStripMenuItem(pct + "%") {
                    Tag = pct / 100.0
                };
                item.Click += Menu_Opacity_click;
                menuOpacity.Items.Add(item);
            }

            //Set native renderer on context menus
            Asztal.Szótár.NativeToolStripRenderer.SetToolStripRenderer(
                menuContext, MenuWindows, menuOpacity
            );

            //Set to Key event preview
            this.KeyPreview = true;

            _openPanels.Add(this);

            Log.Write("Main form constructed");
        }

        #region Event override

        bool _managersInitialized;

        protected override void OnHandleCreated(EventArgs e){
 	        base.OnHandleCreated(e);

            //Window init
            KeepAspectRatio = false;
            GlassMargins = new Padding(-1);

            //Managers: initialize once only. OnHandleCreated fires again when the
            //handle is recreated (e.g. ShowInTaskbar change on primary promotion)
            //and re-registering the processors would duplicate them.
            if (!_managersInitialized) {
                _managersInitialized = true;
                MessagePumpManager.Initialize(this);
                _windowListManager = new WindowListMenuManager(this, MenuWindows) {
                    ParentMenus = new[] {
                        menuContext
                    }
                };
            }

            //Platform specific form initialization
            Program.Platform.PostHandleFormInit(this);
        }

        protected override void OnShown(EventArgs e) {
            Log.Write("Main form shown");
            base.OnShown(e);

            //Apply startup options
            _startupOptions.Apply(this);

            //Restore secondary panels from last session (primary only)
            if (!IsSecondaryPanel) {
                _suppressLayoutSave = true;
                try {
                    PanelLayoutManager.Restore(this);
                }
                finally {
                    _suppressLayoutSave = false;
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            Log.Write("Main form closing");
            base.OnClosing(e);

            MainForm promoted = null;
            if (!IsSecondaryPanel) {
                if (!ApplicationExiting && _childPanels.Count > 0) {
                    //The primary is closing but other panels remain: promote a child
                    //to primary so the panel set keeps working (and saving its layout).
                    promoted = PromotePrimaryRole();
                }
                else {
                    //Layout is saved incrementally on every change; suppress further saves so
                    //that child panels closed during shutdown do not erase the stored layout.
                    _suppressLayoutSave = true;
                }
            }

            MessagePumpManager.Dispose();
            Program.Platform.CloseForm(this);
        }

        protected override void OnClosed(EventArgs e) {
            Log.Write("Main form closed");
            base.OnClosed(e);

            if (_colorAlertIndicator != null) {
                _colorAlertIndicator.Dispose();
                _colorAlertIndicator = null;
            }

            //Keep the application alive until the last panel window is closed
            _openPanels.Remove(this);
            if (_openPanels.Count > 0)
                return;
            Log.Write("Last panel closed, exiting application loop");
            Application.ExitThread();
        }

        /// <summary>
        /// Transfers the primary role to the first remaining child panel.
        /// Called when the primary closes while other panels are still open.
        /// </summary>
        private MainForm PromotePrimaryRole() {
            var newPrimary = _childPanels[0];
            _childPanels.RemoveAt(0);
            newPrimary._primaryPanel = null;

            //Changing ShowInTaskbar recreates the window handle, which destroys the
            //DWM thumbnail bound to it. Capture the current clone state and restore
            //it right after taking over the taskbar button.
            var cloneHandle = newPrimary.CurrentThumbnailWindowHandle;
            var cloneRegion = newPrimary.SelectedThumbnailRegion;
            newPrimary.ShowInTaskbar = true; //take over the single taskbar button
            if (cloneHandle != null) {
                newPrimary.SetThumbnail(cloneHandle, cloneRegion);
            }

            foreach (var child in _childPanels) {
                child._primaryPanel = newPrimary;
                newPrimary._childPanels.Add(child);
            }
            _childPanels.Clear();

            //This form is closing: stop saving from here, let the new primary take over
            _suppressLayoutSave = true;
            Log.Write("Primary role promoted to another panel");
            PanelLayoutManager.StartWatcher(newPrimary);
            newPrimary.NotifyPanelLayoutChanged();
            return newPrimary;
        }

        FormWindowState _lastWindowState = FormWindowState.Normal;

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);

            //Minimizing the primary from the taskbar hides all secondary panels;
            //restoring it brings them all back.
            if (WindowState != _lastWindowState) {
                _lastWindowState = WindowState;
                //自動非表示による最小化/復元は AutoHide/AutoShowAllPanels 側で
                //子パネルを処理済みのため、ここでは連動させない
                if (!IsSecondaryPanel && !_autoHidden) {
                    if (WindowState == FormWindowState.Minimized) {
                        foreach (var child in _childPanels) {
                            Program.Platform.HideForm(child);
                            //Opacity=0 で隠す実装では Visible 変更イベントが発火しないため明示的に更新する
                            child.UpdateColorAlertIndicator();
                        }
                    }
                    else {
                        foreach (var child in _childPanels) {
                            Program.Platform.RestoreForm(child);
                            child.UpdateColorAlertIndicator();
                        }
                    }
                }
            }

            UpdateColorAlertIndicator();
        }

        protected override void OnLocationChanged(EventArgs e) {
            base.OnLocationChanged(e);
            UpdateColorAlertIndicator();
        }

        protected override void OnVisibleChanged(EventArgs e) {
            base.OnVisibleChanged(e);
            UpdateColorAlertIndicator();
        }


        protected override void OnResizeEnd(EventArgs e) {
            base.OnResizeEnd(e);

            //Persist layout after a move/resize of any panel
            NotifyPanelLayoutChanged();
        }

        protected override void OnResizing(EventArgs e) {
            //Update aspect ratio from thumbnail while resizing (but do not refresh, resizing does that anyway)
            if (ThumbnailPanel.IsShowingThumbnail) {
                SetAspectRatio(ThumbnailPanel.ThumbnailPixelSize, false);
            }
        }

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);

            //Ignore activation while form is being torn down
            if (IsDisposed || Disposing) {
                return;
            }

            //Restoring any panel brings back the whole panel set
            RestoreAllPanels();

            //前面化でインジケーターが下に潜ることがあるため Z オーダーを再同期する
            UpdateColorAlertIndicator();
        }

        protected override void OnDeactivate(EventArgs e) {
            base.OnDeactivate(e);

            //HACK: sometimes, even if TopMost is true, the window loses its "always on top" status.
            //  This is a fix attempt that probably won't work...
            TopMost = false;
            TopMost = true;

            //フォーカスを奪わずに最前面バンドの先頭へ確実に復帰させる。
            //(TopMost 再設定だけでは順序が保証されない場合があるため)
            //ここでインジケーターの Z オーダーも再同期される。
            ReassertTopMost();
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            base.OnMouseClick(e);

            //Same story as above (OnMouseDoubleClick)
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                OpenContextMenu(null);
            }
        }

        private ThumbnailPanel.RegionDrawnHandler _quickRegionDrawingHandler;

        protected override void WndProc(ref Message m) {
            if (MessagePumpManager != null) {
                if (MessagePumpManager.PumpMessage(ref m)) {
                    return;
                }
            }

            switch (m.Msg) {
                case WM.NCRBUTTONUP:
                    //Open context menu if right button clicked on caption (i.e. all of the window area because of glass)
                    if (m.WParam.ToInt32() == HT.CAPTION) {
                        OpenContextMenu(null);

                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM.NCLBUTTONDOWN:
                    if ((ModifierKeys & Keys.Control) == Keys.Control &&
                        ThumbnailPanel.IsShowingThumbnail &&
                        !ThumbnailPanel.DrawMouseRegions) {

                        ThumbnailPanel.EnableMouseRegionsDrawingWithMouseDown();
                        ThumbnailPanel.RegionDrawn += _quickRegionDrawingHandler;

                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM.NCHITTEST:
                    //Make transparent to hit-testing if in click through mode
                    if (ClickThroughEnabled) {
                        m.Result = (IntPtr)HT.TRANSPARENT;
                        return;
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        private void HandleQuickRegionDrawn(object sender, ThumbnailRegion region) {
            //Reset region drawing state
            ThumbnailPanel.DrawMouseRegions = false;
            ThumbnailPanel.RegionDrawn -= _quickRegionDrawingHandler;

            SelectedThumbnailRegion = region;
        }

        #endregion

        #region Thumbnail operation

        /// <summary>
        /// Sets a new thumbnail.
        /// </summary>
        /// <param name="handle">Handle to the window to clone.</param>
        /// <param name="region">Region of the window to clone or null.</param>
        public void SetThumbnail(WindowHandle handle, ThumbnailRegion region) {
            //Never clone one of our own panel windows: DWM throws when a window
            //is registered as a thumbnail source of itself.
            if (handle != null && IsPanelHandle(handle.Handle)) {
                Log.Write("Refusing to clone own panel window HWND {0}", handle.Handle);
                return;
            }

            try {
                Log.Write("Cloning window HWND {0} of class {1}", handle.Handle, handle.Class);

                CurrentThumbnailWindowHandle = handle;
                ThumbnailPanel.SetThumbnailHandle(handle, region);

                //The internal thumbnail update can fail on a transient DWM error
                //and silently unset the thumbnail: bail out quietly (no error
                //dialog) and let the watcher retry on its next tick.
                if (!ThumbnailPanel.IsShowingThumbnail) {
                    Log.Write("Thumbnail was unset during setup (transient DWM error), will retry");
                    return;
                }

                //Set aspect ratio (this will resize the form)
                SetAspectRatio(ThumbnailPanel.ThumbnailPixelSize, true);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to set new thumbnail", ex);

                ThumbnailError(ex, false, Strings.ErrorUnableToCreateThumbnail);
                ThumbnailPanel.UnsetThumbnail();
                return;
            }

            //Keep secondary panels on the same source window as the primary
            foreach (var child in _childPanels.ToArray()) {
                if (child.CurrentThumbnailWindowHandle == null ||
                    child.CurrentThumbnailWindowHandle.Handle != handle.Handle) {
                    child.SetThumbnail(handle, null);
                }
            }

            if (!IsSecondaryPanel) {
                //再アタッチできたので喪失停止ガードをリセットする(停止状態自体は保持)。
                ResetColorAlertSourceLossState();
                NotifyPanelLayoutChanged();
            }
        }

        /// <summary>
        /// Disables the cloned thumbnail.
        /// </summary>
        /// <param name="targetLost">
        /// クローン対象のウィンドウが終了・消失したことによる解除の場合は true。
        /// ユーザー操作による手動解除では false。
        /// </param>
        public void UnsetThumbnail(bool targetLost = false) {
            //Unset handle
            CurrentThumbnailWindowHandle = null;
            ThumbnailPanel.UnsetThumbnail();

            //Disable aspect ratio
            KeepAspectRatio = false;

            //Secondary panels follow the primary
            foreach (var child in _childPanels.ToArray()) {
                child.UnsetThumbnail();
            }

            if (!IsSecondaryPanel) {
                NotifyPanelLayoutChanged();
            }

            //対象ウィンドウが終了したら最小化する（自動非表示設定がONの場合）。
            //検知経路に依らず一元的にここで処理する。
            //自動非表示と挙動を揃え、フォーカスを奪わない最小化にする。
            if (targetLost && !IsSecondaryPanel && IsHandleCreated &&
                Properties.Settings.Default.HideWhenSourceDeactivated) {
                WindowManagerMethods.ShowWindow(Handle,
                    WindowManagerMethods.SW_SHOWMINNOACTIVE);
            }

            //対象ウィンドウが失われたら、設定に応じてカラーアラート検出を停止する。
            //検出対象が存在しないため誤検知・無駄な走査を防ぎ、目印を「停止」表示にする。
            //自動開始は行わない(ユーザーがメニュー等で明示的に開始する)。
            if (targetLost && !IsSecondaryPanel) {
                StopColorAlertForSourceLoss();
            }
        }

        /// <summary>
        /// Gets or sets the region displayed of the current thumbnail.
        /// </summary>
        public ThumbnailRegion SelectedThumbnailRegion {
            get {
                if (!ThumbnailPanel.IsShowingThumbnail || !ThumbnailPanel.ConstrainToRegion)
                    return null;

                return ThumbnailPanel.SelectedRegion;
            }
            set {
                if (!ThumbnailPanel.IsShowingThumbnail)
                    return;

                ThumbnailPanel.SelectedRegion = value;

                //Setting the region internally updates the thumbnail, which can
                //fail on a transient DWM error and unset it: re-check before
                //querying the pixel size (would throw InvalidOperationException).
                if (!ThumbnailPanel.IsShowingThumbnail)
                    return;

                SetAspectRatio(ThumbnailPanel.ThumbnailPixelSize, true);

                FixPositionAndSize();

                NotifyPanelLayoutChanged();
            }
        }

        const int FixMargin = 10;

        /// <summary>
        /// Fixes the form's position and size, ensuring it is fully displayed in the current screen.
        /// </summary>
        private void FixPositionAndSize() {
            var screen = Screen.FromControl(this);

            if (Width > screen.WorkingArea.Width) {
                Width = screen.WorkingArea.Width - FixMargin;
            }
            if (Height > screen.WorkingArea.Height) {
                Height = screen.WorkingArea.Height - FixMargin;
            }
            if (Location.X + Width > screen.WorkingArea.Right) {
                Location = new Point(screen.WorkingArea.Right - Width - FixMargin, Location.Y);
            }
            if (Location.Y + Height > screen.WorkingArea.Bottom) {
                Location = new Point(Location.X, screen.WorkingArea.Bottom - Height - FixMargin);
            }
        }

        private void ThumbnailError(Exception ex, bool suppress, string title) {
            if (!suppress) {
                ShowErrorDialog(title, Strings.ErrorGenericThumbnailHandleError, ex.Message);
            }

            UnsetThumbnail();
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Gets the form's thumbnail panel.
        /// </summary>
        public ThumbnailPanel ThumbnailPanel { get; }

        /// <summary>
        /// Gets the form's message pump manager.
        /// </summary>
        public MessagePumpManager MessagePumpManager { get; } = new MessagePumpManager();

        /// <summary>
        /// Gets the form's window list drop down menu.
        /// </summary>
        public ContextMenuStrip MenuWindows { get; private set; }

        /// <summary>
        /// Retrieves the window handle of the currently cloned thumbnail.
        /// </summary>
        public WindowHandle CurrentThumbnailWindowHandle { get; private set;}

        #endregion

    }
}
