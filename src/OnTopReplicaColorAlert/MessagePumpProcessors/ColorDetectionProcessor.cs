using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OnTopReplicaColorAlert.Native;
using OnTopReplicaColorAlert.Properties;

namespace OnTopReplicaColorAlert.MessagePumpProcessors {

    /// <summary>
    /// 検出対象として定義済みの色カテゴリ。
    /// </summary>
    public enum ColorCategory {
        None,
        Red,    // 赤
        Orange, // オレンジ
        Gray    // グレー
    }

    /// <summary>
    /// クローン対象ウィンドウを定義済みの色カテゴリについて監視し、検出時にアラームを発報する。
    /// UI スレッドをブロックしないよう、LockBits による高速なピクセル走査を使用する。
    /// </summary>
    class ColorDetectionProcessor : BaseMessagePumpProcessor {

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private const uint SRCCOPY = 0x00CC0020;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;

        // PW_RENDERFULLCONTENT = 2: DirectComposition コンテンツを含む完全なレンダリング
        private const uint PW_RENDERFULLCONTENT = 2;
        // PW_CLIENTONLY = 1: クライアント領域のみ
        private const uint PW_CLIENTONLY = 1;

        private bool _enabled = false;
        private HashSet<ColorCategory> _enabledCategories = new HashSet<ColorCategory>();
        private int _sampleInterval = 500; // サンプリング間隔(ミリ秒)
        private float _alarmVolume = 1.0f; // 0.0 - 1.0
        private volatile bool _alarmActive = false;
        private System.Threading.Timer _alarmStopTimer = null; // アラーム開始からちょうど AlarmDuration ミリ秒後に発火する
        private System.Threading.Thread _detectionThread = null; // バックグラウンド検出スレッド(message pump から独立)
        private volatile bool _detectionRunning = false;
        private System.Windows.Threading.Dispatcher _uiDispatcher = null; // Initialize() 内で UI スレッド上にてキャプチャする
        private const int AlarmDuration = 3000; // 3秒(ミリ秒単位)
        // 平均色検出: 平均 HSV をターゲット色と比較する際のしきい値
        // 多数のピクセルを平均すると混ざってくすんだ色になるため、意図的に緩めに設定している
        // --- 赤 ---
        // (参考のため残しているが、ピクセル単位モードでは使用していない)
        // --- グレー密度しきい値 ---
        // グレーは grayPixels / totalPixels がこの割合以上のときのみ発報し、スクロールバーや境界線による誤報を防ぐ。
        // 赤/オレンジは一致ピクセルが1つでもあれば発報する(有色の背景はほぼ存在しないため)。
        private const int GrayMinDensityPct = 8; // グレーのピクセルが領域全体の 8% 以上であること
        private const int CustomColorTolerance = 32;
        // 検出処理を続行するために必要な、背景以外のピクセルの最小絶対数。
        // 1 に設定: 有効なピクセルが1つでもある領域は評価対象とする。
        private const int MinNonBgPixels = 1; // 絶対ピクセル数
        private System.Windows.Media.MediaPlayer _mediaPlayer;
        private bool _selfTestRun = false; // 初回有効化サイクル時に分類セルフテストを一度だけ実行する

        public bool Enabled {
            get { return _enabled; }
            set {
                bool wasDisabled = !_enabled;
                _enabled = value;
                // 監視の開始時は消失検知の状態をリセットする
                if (value && wasDisabled) {
                    _lossArmed = false;
                    _lossOngoing = false;
                    _lossMissCount = 0;
                }
                // 初回有効化時に分類セルフテストを一度だけ実行する(ウィンドウ/領域の状態にかかわらず)
                if (value && wasDisabled && !_selfTestRun) {
                    _selfTestRun = true;
                    RunClassificationSelfTest();
                }
                // バックグラウンド検出スレッドの開始または停止
                // (カウント監視中はスレッドを維持し、検出数の表示更新を続ける)
                if (value || _countMonitoring) {
                    StartDetectionThread();
                } else {
                    StopDetectionThread();
                }
                NotifyIndicatorUpdate();
            }
        }

        /// <summary>
        /// 実行状態(Enabled/Paused)の変化をフォームへ通知し、
        /// パネル右上の●インジケーターを更新させる。どのスレッドからでも呼び出し可能。
        /// </summary>
        private void NotifyIndicatorUpdate() {
            var form = Form;
            if (form == null || form.IsDisposed || !form.IsHandleCreated)
                return;
            try {
                form.BeginInvoke((Action)(() => {
                    if (!form.IsDisposed) form.UpdateColorAlertIndicator();
                }));
            }
            catch { } //シャットダウン中のハンドル破棄と競合した場合は無視する
        }

        /// <summary>
        /// 暗転(黒画面)を消失判定から除外するかどうか。
        /// true(既定)の場合、ほぼ黒一色のサンプルでは消失判定を保留する。
        /// </summary>
        public bool IgnoreDarkFrames { get; set; } = true;

        private volatile bool _paused = false;

        /// <summary>
        /// 停止フラグ。true の間は検出・発報を行わない(Enabled 設定は保持される)。
        /// メニューの「全パネル停止/開始」から使用され、PanelLayoutManager によって永続化される。
        /// </summary>
        public bool Paused {
            get { return _paused; }
            set {
                if (_paused == value) return;
                _paused = value;
                if (value) {
                    // 停止時: 鳴動中のアラームを止め、消失検知の状態をリセットする
                    if (_alarmActive) StopAlarm();
                    _lossArmed = false;
                    _lossOngoing = false;
                    _lossMissCount = 0;
                }
                Log.Write("ColorDetection: Paused={0}", value);
                NotifyIndicatorUpdate();
            }
        }

        /// <summary>
        /// 検出時にアラームを発報させる色カテゴリの集合。
        /// </summary>
        public HashSet<ColorCategory> EnabledCategories {
            get { return _enabledCategories; }
            set { _enabledCategories = value ?? new HashSet<ColorCategory>(); }
        }

        public Color? CustomTargetColor { get; set; }

        private bool _alertOnLoss = false;
        // 消失検知モードで「一度対象色を検出済み」であることを示すフラグ。
        // true の状態で対象色が見つからなくなったサンプリングでアラームを発報する。
        private volatile bool _lossArmed = false;
        // 消失状態が継続中であることを示すフラグ。true の間はアラームを繰り返し発報し、
        // 対象色が再出現した時点で false に戻してアラームを停止する。
        private volatile bool _lossOngoing = false;
        // 直近のサンプリング画像がほぼ黒一色だったことを示すフラグ。
        private volatile bool _lastSampleMostlyBlack = false;
        private const int MostlyBlackPct = 95; // 黒スキップ画素がこの割合以上なら黒画面とみなす
        private int _lossMissCount = 0; // 連続で対象色が見つからなかった回数(検出スレッドのみが使用)

        private int _lossMissThreshold = 3;

        /// <summary>
        /// 消失アラームの発報に必要な連続消失サンプリング回数(1以上)。
        /// フェード等の一時的な消失を吸収し、誤報を防ぐ。
        /// </summary>
        public int LossMissThreshold {
            get { return _lossMissThreshold; }
            set { _lossMissThreshold = Math.Max(1, value); }
        }

        /// <summary>
        /// 消失検知モード。true の場合、対象色を一度検出した後に
        /// 「検出されなくなった」時点でアラームを発報する(検出時には発報しない)。
        /// </summary>
        public bool AlertOnLoss {
            get { return _alertOnLoss; }
            set {
                if (_alertOnLoss != value) {
                    _alertOnLoss = value;
                    _lossArmed = false; // モード切替時は状態をリセット
                    _lossOngoing = false;
                    _lossMissCount = 0;
                    Log.Write("ColorDetection: AlertOnLoss={0}", value);
                }
            }
        }

        private volatile bool _countMonitoring = false;

        /// <summary>
        /// カウント監視モード。true の間は検出が無効・停止中でもサンプリングを継続し、
        /// 検出ピクセル数(LastXxxCount)を更新し続ける。アラームは発報しない。
        /// カラーアラートパネルの表示中に有効化される。
        /// </summary>
        public bool CountMonitoringEnabled {
            get { return _countMonitoring; }
            set {
                if (_countMonitoring == value) return;
                _countMonitoring = value;
                if (value) {
                    StartDetectionThread();
                } else if (!_enabled) {
                    StopDetectionThread();
                }
                Log.Write("ColorDetection: CountMonitoring={0}", value);
            }
        }

        // 直近のサンプリングで検出されたカテゴリ別ピクセル数(UI 表示用)。
        // int の読み書きはアトミックなので、検出スレッドが書き込み UI スレッドが読み取っても安全。
        public int LastRedCount { get; private set; }
        public int LastOrangeCount { get; private set; }
        public int LastGrayCount { get; private set; }
        public int LastCustomCount { get; private set; }
        /// <summary>直近サンプリングの時刻(UTC)。一度も実行されていなければ DateTime.MinValue。</summary>
        public DateTime LastSampleTimeUtc { get; private set; } = DateTime.MinValue;

        private int _minDetectionPixels = 1;

        /// <summary>
        /// アラーム発報に必要な、ターゲット色に一致するピクセルの最小数(1以上)。
        /// 赤/オレンジ/カスタム色に適用される。グレーは密度条件に加えてこの条件も満たす必要がある。
        /// </summary>
        public int MinDetectionPixels {
            get { return _minDetectionPixels; }
            set { _minDetectionPixels = Math.Max(1, value); }
        }

        public int SampleInterval {
            get { return _sampleInterval; }
            set { _sampleInterval = Math.Max(100, value); }
        }

        public bool IsAlarmActive {
            get { return _alarmActive; }
        }

        public float AlarmVolume {
            get { return _alarmVolume; }
            set { _alarmVolume = Math.Max(0, Math.Min(1, value)); }
        }

        public string AlarmSoundFile { get; set; } = string.Empty;

        /// <summary>
        /// アラーム発報時に監視対象ウィンドウへキーを送信するかどうか。
        /// </summary>
        public bool KeyPressEnabled { get; set; } = false;

        /// <summary>
        /// アラーム発報時に送信するキー(修飾キーなしの単一キー)。
        /// </summary>
        public Keys KeyPressKey { get; set; } = Keys.None;

        /// <summary>
        /// 監視対象ウィンドウへ設定されたキーを PostMessage で送信する。
        /// 非アクティブなウィンドウにも届く。バックグラウンドスレッドから呼び出し可能。
        /// </summary>
        private void SendKeyToTargetWindow() {
            if (!KeyPressEnabled || KeyPressKey == Keys.None)
                return;

            var handle = Form != null ? Form.CurrentThumbnailWindowHandle : null;
            if (handle == null)
                return;

            try {
                uint vk = (uint)(KeyPressKey & Keys.KeyCode);
                uint scanCode = MapVirtualKey(vk, 0 /* MAPVK_VK_TO_VSC */);
                IntPtr wParam = new IntPtr((int)vk);
                IntPtr lParamDown = new IntPtr(unchecked((int)(1u | (scanCode << 16))));
                IntPtr lParamUp = new IntPtr(unchecked((int)(1u | (scanCode << 16) | (1u << 30) | (1u << 31))));
                PostMessage(handle.Handle, WM_KEYDOWN, wParam, lParamDown);
                PostMessage(handle.Handle, WM_KEYUP, wParam, lParamUp);
                Log.Write("ColorDetection: key {0} sent to target window", KeyPressKey);
            }
            catch (Exception ex) {
                Log.Write("ColorDetection: failed to send key: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 音声ファイルの代わりに <see cref="System.Media.SystemSounds"/> のサウンドを
        /// 選択する疑似パスのプレフィックス(例: "system:Asterisk")。
        /// </summary>
        public const string SystemSoundPrefix = "system:";

        /// <summary>
        /// 指定された疑似パスに符号化されたシステムサウンドを再生する。
        /// 値が "system:" の疑似パスでない場合は false を返す。
        /// </summary>
        public static bool TryPlaySystemSound(string sound) {
            if (string.IsNullOrEmpty(sound) || !sound.StartsWith(SystemSoundPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            switch (sound.Substring(SystemSoundPrefix.Length).ToLowerInvariant()) {
                case "asterisk": System.Media.SystemSounds.Asterisk.Play(); break;
                case "exclamation": System.Media.SystemSounds.Exclamation.Play(); break;
                case "hand": System.Media.SystemSounds.Hand.Play(); break;
                case "question": System.Media.SystemSounds.Question.Play(); break;
                default: System.Media.SystemSounds.Beep.Play(); break;
            }
            return true;
        }

        public override void Initialize(MainForm form) {
            base.Initialize(form);
            // バックグラウンドスレッドから MediaPlayer 呼び出しを正しいスレッドへマーシャリングできるよう、
            // UI スレッド上で WPF の Dispatcher をキャプチャする(WinForms では Application.Current は null)。
            _uiDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        public override bool Process(ref Message msg) {
            // 検出はバックグラウンドスレッド(_detectionThread)が担当する。
            // 色検出のタイミング制御に message pump はもはや必要ない。
            return false;
        }

        /// <summary>
        /// バックグラウンド検出スレッドが未起動であれば開始する。
        /// </summary>
        private void StartDetectionThread() {
            if (_detectionThread != null && _detectionThread.IsAlive)
                return;
            _detectionRunning = true;
            _detectionThread = new System.Threading.Thread(DetectionThreadLoop) {
                IsBackground = true,
                Name = "ColorDetection"
            };
            _detectionThread.Start();
            Log.Write("ColorDetection: background thread started (interval={0}ms)", _sampleInterval);
        }

        /// <summary>
        /// バックグラウンド検出スレッドに停止を通知する。
        /// </summary>
        private void StopDetectionThread() {
            _detectionRunning = false;
            // スレッドは次のスリープサイクルで終了する — Join は不要(IsBackground=true)
            Log.Write("ColorDetection: background thread stop requested");
        }

        /// <summary>
        /// バックグラウンド検出ループ。WinForms の message pump とは独立に _sampleInterval ミリ秒ごとに実行されるため、
        /// フルスクリーンのゲーム(EVE Online など)によって message pump の頻度が約6秒に1回まで低下しても、
        /// 検出レイテンシは約500ミリ秒に保たれる。
        /// </summary>
        private void DetectionThreadLoop() {
            while (_detectionRunning) {
                System.Threading.Thread.Sleep(_sampleInterval);
                if (!_detectionRunning) break;
                if (!_enabled && !_countMonitoring) break;
                // アラーム発報が許可されるのは、検出有効かつ停止中でない場合のみ。
                // カウント監視中はそれ以外でもサンプリングを継続し、検出数の表示だけ更新する。
                bool alarmAllowed = _enabled && !_paused;
                if (!alarmAllowed && !_countMonitoring) continue;
                if (Form == null || Form.CurrentThumbnailWindowHandle == null) continue;
                // 対象ウィンドウが既に破棄されている場合は検出をスキップする。
                // 破棄されたハンドルに対する検出は失敗(色なし)となり、消失検知モードで
                // 誤ってアラームを発報してしまう。喪失時の停止処理(UnsetThumbnail)が
                // 走る前のこの隙間を塞ぐ。
                if (!WindowManagerMethods.IsWindow(Form.CurrentThumbnailWindowHandle.Handle)) {
                    Log.Write("ColorDetection: target window no longer exists — skipping detection");
                    continue;
                }
                // 非アクティブ抑制: 対象ウィンドウが前面(フォアグラウンド)でない場合はアラームを発報しない(常に有効)。
                // カウント監視はそのまま継続する(alarmAllowed のみ無効化する)。
                if (alarmAllowed && GetForegroundWindow() != Form.CurrentThumbnailWindowHandle.Handle) {
                    alarmAllowed = false;
                    if (_alarmActive) StopAlarm();
                    // 消失検知の状態をリセットし、再アクティブ化直後の誤報を防ぐ
                    _lossArmed = false;
                    _lossOngoing = false;
                    _lossMissCount = 0;
                    if (!_countMonitoring) continue;
                }

                if (_enabledCategories.Count == 0 && !CustomTargetColor.HasValue) continue;
                // 消失検知モードでは、アラーム中も検出を継続する
                // (色の再出現でアラームを止め、消失が続く限り鳴らし続けるため)
                if (_alarmActive && !_alertOnLoss && !_countMonitoring) continue;

                var catList = string.Join(",", _enabledCategories);
                Log.Write("Performing color detection (categories={0})", catList);
                try {
                    _lastSampleMostlyBlack = false; // 前回サンプルの黒画面フラグをクリア
                    bool detected = DetectColorInWindow(Form.CurrentThumbnailWindowHandle.Handle);
                    if (!alarmAllowed) {
                        // カウント監視のみ: 検出数は更新済み。発報や消失判定は行わない。
                    }
                    else if (_alertOnLoss) {
                        // 消失検知モード: 一度検出(armed)した後、見つからなくなったら発報し、
                        // 色が再出現するまでアラームを繰り返し鳴らし続ける
                        if (!detected && _lastSampleMostlyBlack && IgnoreDarkFrames) {
                            // 黒画面は消失判定の対象外: 状態と連続消失カウントを変えず判定を保留する
                            Log.Write("ColorDetection: sample mostly black — loss judgment skipped");
                        }
                        else if (detected) {
                            _lossMissCount = 0;
                            if (_lossOngoing) {
                                _lossOngoing = false;
                                Log.Write("ColorDetection: target color reappeared — stopping alarm");
                                if (_alarmActive) StopAlarm();
                            }
                            if (!_lossArmed) {
                                _lossArmed = true;
                                Log.Write("ColorDetection: loss-mode armed (color detected)");
                            }
                        }
                        else if (_lossArmed) {
                            // 連続 LossMissThreshold 回消失したら発報(フェード等の一時的な消失を吸収)
                            _lossMissCount++;
                            Log.Write("ColorDetection: target color missing ({0}/{1})", _lossMissCount, _lossMissThreshold);
                            if (_lossMissCount >= _lossMissThreshold) {
                                _lossMissCount = 0;
                                _lossArmed = false;
                                _lossOngoing = true;
                                Log.Write("ColorDetection: target color lost — triggering alarm");
                                StartAlarm();
                            }
                        }
                        else if (_lossOngoing && !_alarmActive) {
                            // 消失が継続中: 前回のアラームが終わったら再発報する
                            Log.Write("ColorDetection: target color still lost — re-triggering alarm");
                            StartAlarm();
                        }
                    }
                    else if (detected) {
                        StartAlarm();
                    }
                } catch (Exception ex) {
                    Log.Write("ColorDetection thread error: {0}", ex.Message);
                }
            }
            Log.Write("ColorDetection: background thread exited");
        }

        /// <summary>
        /// 監視対象ウィンドウ内にターゲット色が存在するかを検出する。
        /// PrintWindow API でソースウィンドウを直接キャプチャする(遮蔽の問題を回避)。
        /// </summary>
        private bool DetectColorInWindow(IntPtr windowHandle)
        {
            try
            {
                // ウィンドウのクライアント領域サイズを取得
                if( !WindowManagerMethods.GetClientRect(windowHandle, out var clientRect) )
                {
                    Log.Write("ColorDetection: Failed to get client rect");
                    return false;
                }

                var clientSize = new Size(clientRect.Width, clientRect.Height);
                if (clientSize.Width <= 0 || clientSize.Height <= 0)
                {
                    Log.Write("ColorDetection: Invalid client size {0}x{1}", clientSize.Width, clientSize.Height);
                    return false;
                }

                // このプロセッサ自身のフォームを使用する: 複数のパネルウィンドウが開いている場合、
                // 各プロセッサは自分が属するウィンドウの領域を読み取らなければならない。
                MainForm mainForm = Form;

                Rectangle regionRect;

                // 選択領域があれば優先的に使用する
                if (mainForm?.SelectedThumbnailRegion != null)
                {
                    var selectedRegion = mainForm.SelectedThumbnailRegion;
                    regionRect = selectedRegion.ComputeRegionRectangle(clientSize);
                    Log.Write("ColorDetection: Region={0},{1} {2}x{3} (relative={4})",
                        regionRect.X, regionRect.Y, regionRect.Width, regionRect.Height, selectedRegion.Relative);
                }
                else
                {
                    regionRect = clientRect;
                    Log.Write("ColorDetection: No region, full window {0}x{1}", clientSize.Width, clientSize.Height);
                }

                if (regionRect.Width <= 0 || regionRect.Height <= 0)
                {
                    Log.Write("ColorDetection: Invalid region {0}x{1}", regionRect.Width, regionRect.Height);
                    return false;
                }

                // PrintWindow でウィンドウ内容をキャプチャする(遮蔽の影響を受けない)
                Bitmap windowBmp = null;
                Bitmap regionBmp = null;
                try
                {
                    // ウィンドウ全体のサイズを取得(非クライアント領域を含む)
                    // 注意: GetWindowRect は RECT (left,top,right,bottom) を Rectangle (X,Y,Width=right,Height=bottom) にマッピングして返す
                    WindowManagerMethods.GetWindowRect(windowHandle, out var windowRect);
                    int windowWidth = windowRect.Width - windowRect.X;
                    int windowHeight = windowRect.Height - windowRect.Y;

                    if (windowWidth <= 0 || windowHeight <= 0)
                    {
                        Log.Write("ColorDetection: Invalid window size {0}x{1} (rect={2},{3},{4},{5})",
                            windowWidth, windowHeight, windowRect.X, windowRect.Y, windowRect.Width, windowRect.Height);
                        return false;
                    }

                    // ウィンドウサイズのビットマップを作成し、PrintWindow でキャプチャする
                    windowBmp = new Bitmap(windowWidth, windowHeight, PixelFormat.Format32bppArgb);
                    bool printSuccess = false;

                    // 複数の PrintWindow flags を順に試す
                    var flags = new uint[] { PW_RENDERFULLCONTENT, PW_CLIENTONLY, 0 };
                    foreach (uint flag in flags)
                    {
                        using (var g = Graphics.FromImage(windowBmp))
                        {
                            var hdc = g.GetHdc();
                            try
                            {
                                printSuccess = PrintWindow(windowHandle, hdc, flag);
                            }
                            finally
                            {
                                g.ReleaseHdc(hdc);
                            }
                        }
                        if (printSuccess)
                        {
                            Log.Write("ColorDetection: PrintWindow OK with flag={0}, winSize={1}x{2}", flag, windowWidth, windowHeight);
                            break;
                        }
                    }

                    if (!printSuccess)
                    {
                        Log.Write("ColorDetection: PrintWindow failed (all flags), falling back to CopyFromScreen");
                        windowBmp.Dispose();
                        windowBmp = null;
                        return DetectColorFallback(windowHandle, regionRect);
                    }

                    // ウィンドウビットマップ内におけるクライアント領域のオフセットを計算
                    var clientOriginScreen = WindowManagerMethods.ClientToScreen(windowHandle, new NPoint(0, 0));
                    int clientOffsetX = clientOriginScreen.X - windowRect.X;
                    int clientOffsetY = clientOriginScreen.Y - windowRect.Y;

                    // 領域座標をウィンドウビットマップ座標へオフセットする
                    int cropX = clientOffsetX + regionRect.X;
                    int cropY = clientOffsetY + regionRect.Y;
                    int cropW = Math.Min(regionRect.Width, windowWidth - cropX);
                    int cropH = Math.Min(regionRect.Height, windowHeight - cropY);

                    Log.Write("ColorDetection: PrintWindow OK, winSize={0}x{1}, clientOffset=({2},{3}), crop=({4},{5} {6}x{7})",
                        windowWidth, windowHeight, clientOffsetX, clientOffsetY, cropX, cropY, cropW, cropH);

                    if (cropW <= 0 || cropH <= 0)
                    {
                        Log.Write("ColorDetection: crop region empty");
                        return false;
                    }

                    // 選択領域を切り出す
                    regionBmp = windowBmp.Clone(new Rectangle(cropX, cropY, cropW, cropH), PixelFormat.Format32bppArgb);

                    // デバッグ用スクリーンショットを保存(IO 過多を避けるため20回に1回のみ保存)
                    _debugCounter++;
                    if (_debugCounter % 20 == 1)
                    {
                        SaveDebugBitmap(regionBmp, "region_capture");
                    }

                    return SampleBitmapForColor(regionBmp);
                }
                finally
                {
                    regionBmp?.Dispose();
                    windowBmp?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ColorDetection Error: {0}\n{1}", ex.Message, ex.StackTrace);
                return false;
            }
        }

        private int _debugCounter = 0;

        /// <summary>
        /// デバッグ用にビットマップを AppData フォルダーへ保存する。
        /// </summary>
        [Conditional("DEBUG")]
        private void SaveDebugBitmap(Bitmap bmp, string prefix)
        {
            try
            {
                string dir = AppPaths.PrivateRoamingFolderPath;
                if (string.IsNullOrEmpty(dir)) return;
                string path = Path.Combine(dir, prefix + "_debug.png");
                bmp.Save(path, ImageFormat.Png);
                Log.Write("ColorDetection: Debug bitmap saved to {0}", path);
            }
            catch (Exception ex)
            {
                Log.Write("ColorDetection: Failed to save debug bitmap: {0}", ex.Message);
            }
        }

        /// <summary>
        /// フォールバック: ソースウィンドウの DC から BitBlt でクライアント領域を直接キャプチャする。
        /// これにより OnTopReplicaColorAlert のオーバーレイまで写り込む CopyFromScreen を回避できる。
        /// BitBlt が全面黒の画像を返した場合は、opacity トリック付きの CopyFromScreen にフォールバックする。
        /// </summary>
        private bool DetectColorFallback(IntPtr windowHandle, Rectangle regionRect)
        {
            Log.Write("ColorDetection Fallback(BitBlt): region={0},{1} {2}x{3}",
                regionRect.X, regionRect.Y, regionRect.Width, regionRect.Height);

            if (regionRect.Width <= 0 || regionRect.Height <= 0)
                return false;

            int cropW = Math.Min(regionRect.Width, 800);
            int cropH = Math.Min(regionRect.Height, 600);

            // 方式1: BitBlt + GetDC でソースウィンドウのクライアント領域から直接キャプチャする(ちらつきなし)
            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr hdcMem = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr hOldBmp = IntPtr.Zero;
            Bitmap bmp = null;
            try
            {
                hdcSrc = GetDC(windowHandle);
                if (hdcSrc != IntPtr.Zero)
                {
                    hdcMem = CreateCompatibleDC(hdcSrc);
                    hBitmap = CreateCompatibleBitmap(hdcSrc, cropW, cropH);
                    hOldBmp = SelectObject(hdcMem, hBitmap);

                    bool ok = BitBlt(hdcMem, 0, 0, cropW, cropH, hdcSrc, regionRect.X, regionRect.Y, SRCCOPY);
                    SelectObject(hdcMem, hOldBmp);

                    if (ok)
                    {
                        bmp = Bitmap.FromHbitmap(hBitmap);

                        // 全面黒かどうかを確認(ハードウェアレンダリングのウィンドウは黒い画像を返すことがある)
                        if (!IsBitmapAllBlack(bmp))
                        {
                            Log.Write("ColorDetection Fallback: BitBlt OK, size={0}x{1}", cropW, cropH);
                            _debugCounter++;
                            if (_debugCounter % 20 == 1)
                                SaveDebugBitmap(bmp, "bitblt_capture");
                            return SampleBitmapForColor(bmp);
                        }
                        else
                        {
                            Log.Write("ColorDetection Fallback: BitBlt returned all-black, trying CopyFromScreen");
                            bmp.Dispose();
                            bmp = null;
                        }
                    }
                    else
                    {
                        Log.Write("ColorDetection Fallback: BitBlt failed");
                    }
                }
                else
                {
                    Log.Write("ColorDetection Fallback: GetDC failed");
                }
            }
            catch (Exception ex)
            {
                Log.Write("ColorDetection Fallback BitBlt error: {0}", ex.Message);
                bmp?.Dispose();
                bmp = null;
            }
            finally
            {
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                if (hdcMem != IntPtr.Zero) DeleteDC(hdcMem);
                if (hdcSrc != IntPtr.Zero) ReleaseDC(windowHandle, hdcSrc);
            }

            // 方式2: CopyFromScreen(最終手段。オーバーレイが写り込む可能性はあるが、ちらつきは発生しない)
            return DetectColorScreenCapture(windowHandle, regionRect);
        }

        /// <summary>
        /// 最終手段: CopyFromScreen。オーバーレイが写り込む可能性はあるが、ちらつきは発生しない。
        /// </summary>
        private bool DetectColorScreenCapture(IntPtr windowHandle, Rectangle regionRect)
        {
            var scrRect = WindowManagerMethods.ClientToScreenRect(windowHandle, regionRect);
            Log.Write("ColorDetection ScreenCapture: screen={0},{1} {2}x{3}", scrRect.X, scrRect.Y, scrRect.Width, scrRect.Height);

            if (scrRect.Width <= 0 || scrRect.Height <= 0)
                return false;

            int maxW = Math.Min(scrRect.Width, 800);
            int maxH = Math.Min(scrRect.Height, 600);

            Bitmap bmp = null;
            try
            {
                bmp = new Bitmap(maxW, maxH, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(scrRect.X, scrRect.Y, 0, 0, new Size(maxW, maxH));
                }
                // デバッグ用にスクリーンショットを常時保存(頻度制限: 5回に1回保存)
                _debugCounter++;
                if (_debugCounter % 5 == 1)
                    SaveDebugBitmap(bmp, "screen_capture");
                return SampleBitmapForColor(bmp);
            }
            catch (Exception ex)
            {
                Log.Write("ColorDetection ScreenCapture error: {0}", ex.Message);
                return false;
            }
            finally
            {
                bmp?.Dispose();
                bmp = null;
            }
        }

        /// <summary>
        /// ビットマップがほぼ黒一色かどうかを判定する(ハードウェアレンダリングのウィンドウからのキャプチャ失敗を示す)。
        /// 全チャンネルが threshold 以下のピクセルを「黒」とみなす。
        /// サンプリングしたピクセルの50%超が黒に近い場合に true を返す。
        /// ハードウェアレンダリングのウィンドウは、ノイズが散在するほぼ黒一色の画像を返すことが多い。
        /// </summary>
        private bool IsBitmapAllBlack(Bitmap bmp)
        {
            if (bmp == null) return true;
            const int threshold = 15; // ハードウェアレンダリングのウィンドウでは BitBlt が (3,3,3) のような黒に近いピクセルを返すことがある
            int stepX = Math.Max(1, bmp.Width / 10);
            int stepY = Math.Max(1, bmp.Height / 10);
            int totalSamples = 0;
            int blackSamples = 0;
            for (int y = 0; y < bmp.Height; y += stepY)
            {
                for (int x = 0; x < bmp.Width; x += stepX)
                {
                    Color c = bmp.GetPixel(x, y);
                    totalSamples++;
                    if (c.R <= threshold && c.G <= threshold && c.B <= threshold)
                        blackSamples++;
                }
            }
            bool result = totalSamples > 0 && (blackSamples * 100 / totalSamples) >= 50;
            if (result)
                Log.Write("ColorDetection: IsBitmapAllBlack=true ({0}/{1} samples near-black={2}%, threshold={3})", blackSamples, totalSamples, blackSamples * 100 / totalSamples, threshold);
            return result;
        }

        /// <summary>
        /// ピクセル単位の色検出アルゴリズム:
        /// 1. 全ピクセルを走査し、白(S&lt;15 かつ V&gt;85)、黒/黒に近い色(V&lt;15)、
        ///    および高彩度の非ターゲット色(青/緑のアイコン、S&gt;=40 かつ赤/オレンジ/グレー以外)をスキップする。
        /// 2. 残った各ピクセルを Red / Orange / Gray / None に分類する。
        /// 3a. 赤またはオレンジ: 有効なカテゴリに一致するピクセルが minPixels 以上あれば → 即時アラーム。
        /// 3b. グレーのみ(赤/オレンジが見つからない場合): grayPixels/totalPixels &gt;= GrayMinDensityPct% かつ grayPixels &gt;= minPixels → アラーム。
        ///     この密度ガードにより、スクロールバーや境界線(純グレー背景、約6%)による発報を防ぐ。
        /// </summary>
        private bool SampleBitmapForColor(Bitmap bmp) {
            if (bmp == null || bmp.Width <= 0 || bmp.Height <= 0)
                return false;

            Log.Write("ColorDetection Scan: enabledCategories=[{0}], customColor={1}, bmpSize={2}x{3}",
                string.Join(",", _enabledCategories), CustomTargetColor.HasValue ? CustomTargetColor.Value.ToString() : "none", bmp.Width, bmp.Height);

            BitmapData data = null;
            try {
                data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int stride = data.Stride;
                int byteCount = stride * bmp.Height;
                byte[] pixels = new byte[byteCount];
                Marshal.Copy(data.Scan0, pixels, 0, byteCount);

                bmp.UnlockBits(data);
                data = null;

                int totalPixels = bmp.Width * bmp.Height;
                int whiteSkipped    = 0;
                int blackSkipped    = 0;
                int coloredBgSkipped = 0; // 高彩度の非ターゲット色(青/緑のアイコン)
                int redCount        = 0;
                int orangeCount     = 0;
                int grayCount       = 0;
                int customCount     = 0;
                int noneCount       = 0;

                for (int y = 0; y < bmp.Height; y++) {
                    int rowOffset = y * stride;
                    for (int x = 0; x < bmp.Width; x++) {
                        int idx = rowOffset + x * 4;
                        byte pb = pixels[idx];      // B
                        byte pg = pixels[idx + 1];  // G
                        byte pr = pixels[idx + 2];  // R

                        float h, s, v;
                        RgbToHsv(pr, pg, pb, out h, out s, out v);

                        // カスタム色は白/黒スキップより先に判定する
                        // (白っぽい/黒っぽいカスタム色も検出できるようにするため)
                        if (CustomTargetColor.HasValue && IsNearCustomColor(pr, pg, pb, CustomTargetColor.Value)) {
                            customCount++;
                        }

                        // 白をスキップ: 低彩度 + 高輝度(アイコンのテキスト、境界線)
                        if (s < 15 && v > 85) { whiteSkipped++; continue; }

                        // 黒/黒に近い色をスキップ: 暗い UI 背景
                        if (v < 15) { blackSkipped++; continue; }

                        // 分類
                        ColorCategory cat = ClassifyPixelColor(pr, pg, pb);

                        // 高彩度の非ターゲット色(青/緑のアイコン)を解析対象から除外する
                        if (cat == ColorCategory.None && s >= 40) { coloredBgSkipped++; continue; }

                        switch (cat) {
                            case ColorCategory.Red:    redCount++;    break;
                            case ColorCategory.Orange: orangeCount++; break;
                            case ColorCategory.Gray:   grayCount++;   break;
                            default:                   noneCount++;   break;
                        }
                    }
                }

                // UI 表示用に直近カウントを公開する
                LastRedCount = redCount;
                LastOrangeCount = orangeCount;
                LastGrayCount = grayCount;
                LastCustomCount = customCount;
                LastSampleTimeUtc = DateTime.UtcNow;

                int validPixels = redCount + orangeCount + grayCount + noneCount;
                int grayDensityPct = totalPixels > 0 ? grayCount * 100 / totalPixels : 0;

                // 黒画面判定: 黒スキップ画素がほぼ全体を占める場合、消失検知モードでは判定を保留する
                _lastSampleMostlyBlack = totalPixels > 0 && (blackSkipped * 100 / totalPixels) >= MostlyBlackPct;

                Log.Write("ColorDetection counts: red={0} orange={1} gray={2} custom={3} none={4}  total={5} white={6} black={7} coloredBg={8}",
                    redCount, orangeCount, grayCount, customCount, noneCount, totalPixels, whiteSkipped, blackSkipped, coloredBgSkipped);
                Log.Write("  grayDensity={0}%(need \u2265{1}% for gray alarm)", grayDensityPct, GrayMinDensityPct);

                // アラーム発報に必要な最小一致ピクセル数(設定可能、既定1)
                int minPixels = _minDetectionPixels;

                // --- ルール1: 赤またはオレンジ — 一致ピクセルが minPixels 以上であればアラーム発報 ---
                if (_enabledCategories.Contains(ColorCategory.Red) && redCount >= minPixels) {
                    Log.Write("ColorDetection MATCH: Red, {0} pixel(s) (min={1})", redCount, minPixels);
                    SaveDebugBitmap(bmp, "alarm_trigger");
                    return true;
                }
                if (_enabledCategories.Contains(ColorCategory.Orange) && orangeCount >= minPixels) {
                    Log.Write("ColorDetection MATCH: Orange, {0} pixel(s) (min={1})", orangeCount, minPixels);
                    SaveDebugBitmap(bmp, "alarm_trigger");
                    return true;
                }

                // --- ルール2: グレー — 密度が GrayMinDensityPct% 以上かつ minPixels 以上の場合のみ ---
                if (_enabledCategories.Contains(ColorCategory.Gray)) {
                    if (grayDensityPct >= GrayMinDensityPct && grayCount >= minPixels) {
                        Log.Write("ColorDetection MATCH: Gray, {0}px density={1}% (min={2})", grayCount, grayDensityPct, minPixels);
                        SaveDebugBitmap(bmp, "alarm_trigger");
                        return true;
                    } else {
                        Log.Write("ColorDetection no-Gray: {0}px density={1}% < {2}% or count < min({3})", grayCount, grayDensityPct, GrayMinDensityPct, minPixels);
                    }
                }

                // --- ルール3: カスタム色 ---
                if (CustomTargetColor.HasValue && customCount >= minPixels) {
                    Log.Write("ColorDetection MATCH: Custom, {0} pixel(s), target={1}", customCount, CustomTargetColor.Value);
                    SaveDebugBitmap(bmp, "alarm_trigger");
                    return true;
                }

                Log.Write("ColorDetection NO MATCH");
                return false;
            }
            finally {
                if (data != null) bmp.UnlockBits(data);
            }
        }

        /// <summary>
        /// ピクセルの RGB 色を HSV 範囲に基づいて定義済みの色カテゴリに分類する。
        /// 赤:     H が [0,15] または [345,360], S >= 40%, V >= 25%  (暗い縁を持つ赤アイコン)
        /// オレンジ: H が (15,55],               S >= 40%, V >= 25%  (暗い縁を持つオレンジアイコン)
        /// グレー:  S < 20%, V が [15,75%]   (グレーのアイコン、暗いグレー→中間グレーをカバーする広い範囲)
        /// 白/黒のピクセルは、このメソッドを呼び出す前にフィルタリング済みであること。
        /// </summary>
        private static ColorCategory ClassifyPixelColor(byte r, byte g, byte b) {
            float h, s, v;
            RgbToHsv(r, g, b, out h, out s, out v);

            // 赤: 色相が 0° 付近で十分な彩度があるもの
            if (s >= 40 && v >= 25) {
                if (h <= 15 || h >= 345) {
                    return ColorCategory.Red;
                }
                // オレンジ: 赤みがかったオレンジから黄色寄りのオレンジまでの色相
                if (h > 15 && h <= 55) {
                    return ColorCategory.Orange;
                }
            }

            // グレー: 低彩度。暗いグレー(V≈15%)から明るいグレー(V≈83%)までをカバーする
            // 白(V>85, S<15)と黒(V<15)は走査ループ内で事前にフィルタリング済み。
            // S<25 とすることで、グレーのアイコン背景に生じるわずかなレンダリング色味を許容しつつ、ベージュ/暖色系(S≥25)を除外する
            if (s < 25 && v >= 15 && v <= 83) {
                return ColorCategory.Gray;
            }

            return ColorCategory.None;
        }

        private static bool IsNearCustomColor(byte r, byte g, byte b, Color target) {
            int dr = r - target.R;
            int dg = g - target.G;
            int db = b - target.B;
            int distanceSquared = dr * dr + dg * dg + db * db;
            int toleranceSquared = CustomColorTolerance * CustomColorTolerance;
            return distanceSquared <= toleranceSquared;
        }

        /// <summary>
        /// RGB (0-255) を HSV に変換する。H は度(0-360)、S と V はパーセント(0-100)。
        /// </summary>
        private static void RgbToHsv(int r, int g, int b, out float h, out float s, out float v) {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float delta = max - min;

            // 明度 (Value)
            v = max * 100f;

            // 彩度 (Saturation)
            if (max < 0.0001f) { h = 0; s = 0; return; }
            s = (delta / max) * 100f;

            // 色相 (Hue)
            if (delta < 0.0001f) { h = 0; return; }
            if (Math.Abs(max - rf) < 0.0001f)
                h = 60f * (((gf - bf) / delta) % 6f);
            else if (Math.Abs(max - gf) < 0.0001f)
                h = 60f * (((bf - rf) / delta) + 2f);
            else
                h = 60f * (((rf - gf) / delta) + 4f);

            if (h < 0) h += 360f;
        }

        /// <summary>
        /// アラームを開始する。バックグラウンド検出スレッドから呼び出される — スレッドセーフ性を確保するため、
        /// UI/WPF に関わる操作はすべて WPF の dispatcher へディスパッチされる。
        /// </summary>
        private void StartAlarm() {
            if (_alarmActive)
                return;

            _alarmActive = true;

            // AlarmDuration ミリ秒後の自動停止をスケジュールする。
            // message pump と検出スレッドのどちらからも独立している。
            _alarmStopTimer?.Dispose();
            _alarmStopTimer = new System.Threading.Timer(_ => StopAlarm(), null, AlarmDuration, System.Threading.Timeout.Infinite);

            Log.Write("Color alarm triggered! volume={0}, file={1}", _alarmVolume, AlarmSoundFile);

            // 設定されていれば、監視対象ウィンドウへキーを1回送信する
            SendKeyToTargetWindow();

            // システムサウンドの疑似パスは直接再生する(スレッドセーフのため dispatcher は不要)
            if (TryPlaySystemSound(AlarmSoundFile)) {
                Log.Write("System sound played: {0}", AlarmSoundFile);
                return;
            }

            // MediaPlayer は WPF の Dispatcher を持つスレッド上で生成・使用しなければならない。
            // _uiDispatcher は Initialize() 内で UI スレッド上にてキャプチャ済み。
            if (!string.IsNullOrEmpty(AlarmSoundFile) && File.Exists(AlarmSoundFile)) {
                var soundFile = AlarmSoundFile;
                var volume = _alarmVolume;
                if (_uiDispatcher != null) {
                    _uiDispatcher.BeginInvoke((Action)(() => {
                        try {
                            if (_mediaPlayer == null)
                                _mediaPlayer = new System.Windows.Media.MediaPlayer();
                            _mediaPlayer.Open(new Uri(soundFile));
                            _mediaPlayer.Volume = volume;
                            _mediaPlayer.Play();
                            Log.Write("MediaPlayer.Play() called for {0}", soundFile);
                        } catch (Exception ex) {
                            Log.Write("Error playing alarm: {0}", ex.Message);
                        }
                    }));
                } else {
                    Log.Write("Warning: _uiDispatcher is null, cannot play sound");
                    System.Media.SystemSounds.Beep.Play();
                }
            } else {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        /// <summary>
        /// アラームを停止する。
        /// </summary>
        private void StopAlarm() {
            _alarmActive = false;
            _alarmStopTimer?.Dispose();
            _alarmStopTimer = null;
            Log.Write("Color alarm stopped");
            try {
                if (_mediaPlayer != null && _uiDispatcher != null) {
                    _uiDispatcher.BeginInvoke((Action)(() => {
                        try { _mediaPlayer?.Stop(); } catch { }
                    }));
                }
            }
            catch { }
        }

        /// <summary>
        /// 起動時に分類セルフテストを実行し、色検出ロジックを検証する。
        /// 結果は確認用にログへ出力される。
        /// </summary>
        private void RunClassificationSelfTest() {
            Log.Write("=== ColorDetection Self-Test BEGIN ===");
            Log.Write("  Algorithm: per-pixel  Red/Orange>=1px→alarm  Gray>=density{0}%→alarm", GrayMinDensityPct);
            Log.Write("  White skip: S<15 && V>85, Black skip: V<15, MinNonBgPixels={0}", MinNonBgPixels);
            // 形式: (ラベル, R, G, B, 期待するカテゴリ)
            var tests = new[] {
                // --- 赤 (アイコン本体。暗い縁を含むさまざまな色合い) ---
                new { Label="Red icon body",  R=(byte)180, G=(byte)30,  B=(byte)30,  Expect=ColorCategory.Red    },
                new { Label="Deep red edge",  R=(byte)140, G=(byte)20,  B=(byte)20,  Expect=ColorCategory.Red    },
                new { Label="Bright red",     R=(byte)255, G=(byte)20,  B=(byte)10,  Expect=ColorCategory.Red    },
                new { Label="Dark red V=25",  R=(byte)64,  G=(byte)10,  B=(byte)10,  Expect=ColorCategory.Red    },
                // --- オレンジ (アイコン本体。暗い縁取りの色合いを含む) ---
                new { Label="Orange body",    R=(byte)200, G=(byte)120, B=(byte)40,  Expect=ColorCategory.Orange },
                new { Label="Bright orange",  R=(byte)255, G=(byte)140, B=(byte)0,   Expect=ColorCategory.Orange },
                new { Label="Dark orange",    R=(byte)160, G=(byte)80,  B=(byte)20,  Expect=ColorCategory.Orange },
                new { Label="Yellow-orange",  R=(byte)255, G=(byte)180, B=(byte)0,   Expect=ColorCategory.Orange },
                // --- グレー (アイコン本体。暗いグレーから中間グレーまで) ---
                new { Label="Dark gray V=24", R=(byte)60,  G=(byte)60,  B=(byte)60,  Expect=ColorCategory.Gray   },
                new { Label="Med gray V=50",  R=(byte)128, G=(byte)128, B=(byte)128, Expect=ColorCategory.Gray   },
                new { Label="Gray V=70",      R=(byte)178, G=(byte)178, B=(byte)178, Expect=ColorCategory.Gray   },
                new { Label="Gray V=15",      R=(byte)39,  G=(byte)39,  B=(byte)39,  Expect=ColorCategory.Gray   },
                new { Label="Gray V=80 light",R=(byte)204, G=(byte)204, B=(byte)204, Expect=ColorCategory.Gray   },
                // --- 一致してはならないもの (None) ---
                new { Label="Green H=120",    R=(byte)0,   G=(byte)200, B=(byte)0,   Expect=ColorCategory.None   },
                new { Label="Blue H=240",     R=(byte)0,   G=(byte)0,   B=(byte)200, Expect=ColorCategory.None   },
                new { Label="Yellow H=60",    R=(byte)255, G=(byte)255, B=(byte)0,   Expect=ColorCategory.None   },
                // --- 白/黒: 走査時には事前にフィルタリングされるが、分類上は None となる ---
                new { Label="White (skip)",   R=(byte)255, G=(byte)255, B=(byte)255, Expect=ColorCategory.None   },
                new { Label="Black (skip)",   R=(byte)0,   G=(byte)0,   B=(byte)0,   Expect=ColorCategory.None   },
                // --- 境界値 / エッジケース ---
                new { Label="Gray V=76 now ok",R=(byte)194, G=(byte)194, B=(byte)194, Expect=ColorCategory.Gray   },
                new { Label="Gray V=84 above",R=(byte)215, G=(byte)215, B=(byte)215, Expect=ColorCategory.None   },
                new { Label="Gray V=14 below",R=(byte)35,  G=(byte)35,  B=(byte)35,  Expect=ColorCategory.None   },
                new { Label="Low-sat orange", R=(byte)200, G=(byte)180, B=(byte)150, Expect=ColorCategory.None   },
            };

            int pass = 0, fail = 0;
            foreach (var t in tests) {
                var got = ClassifyPixelColor(t.R, t.G, t.B);
                float h, s, v;
                RgbToHsv(t.R, t.G, t.B, out h, out s, out v);
                bool ok = got == t.Expect;
                if (ok) pass++;
                else fail++;
                Log.Write("  [{0}] {1}: rgb=({2},{3},{4}) H={5:F0} S={6:F0}% V={7:F1}% → got={8} expect={9}",
                    ok ? "PASS" : "FAIL", t.Label, t.R, t.G, t.B, h, s, v, got, t.Expect);
            }
            Log.Write("=== ColorDetection Self-Test END: {0} passed, {1} FAILED ===", pass, fail);
        }

        protected override void Shutdown() {
            _countMonitoring = false; // Enabled=false 設定時にスレッドが再起動しないよう先にクリアする
            StopDetectionThread();
            Enabled = false;
            if (_alarmActive)
                StopAlarm();
            if (_mediaPlayer != null && _uiDispatcher != null) {
                try {
                    _uiDispatcher.Invoke((Action)(() => {
                        try { _mediaPlayer?.Close(); } catch { }
                    }));
                } catch { }
                _mediaPlayer = null;
            }
        }
    }
}
