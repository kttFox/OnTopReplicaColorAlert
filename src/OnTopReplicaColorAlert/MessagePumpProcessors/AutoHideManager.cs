using System;
using System.Windows.Forms;
using OnTopReplicaColorAlert.Native;

namespace OnTopReplicaColorAlert.MessagePumpProcessors {

    /// <summary>
    /// 対象(クローン元)ウィンドウが非アクティブになったときにパネルセットを
    /// 自動的に非表示にし、再度アクティブになったら再表示するプロセッサ。
    /// シェルフックのウィンドウアクティブ化通知を監視する。
    /// 設定 <see cref="Properties.Settings.HideWhenSourceDeactivated"/> で有効/無効を切り替える。
    /// </summary>
    class AutoHideManager : BaseMessagePumpProcessor {

        static readonly uint NativeProcessId =
            (uint)System.Diagnostics.Process.GetCurrentProcess().Id;

        public override bool Process(ref Message msg) {
            if (msg.Msg == HookMethods.WM_SHELLHOOKMESSAGE) {
                int hookCode = msg.WParam.ToInt32();
                if (hookCode == HookMethods.HSHELL_WINDOWACTIVATED ||
                    hookCode == HookMethods.HSHELL_RUDEAPPACTIVATED) {
                    HandleActivation(msg.LParam);
                }
            }

            return false; //never swallow the message (other processors use it too)
        }

        private void HandleActivation(IntPtr activatedWindow) {
            //パネルセット全体の制御はプライマリパネルのみが行う
            if (Form.IsSecondaryPanel || Form.IsDisposed)
                return;

            //機能が無効な場合: 自動非表示状態が残っていれば復帰させる
            if (!Properties.Settings.Default.HideWhenSourceDeactivated) {
                Form.AutoShowAllPanels();
                return;
            }

            //クローン中でなければ対象外(非表示状態は解除する)
            var source = Form.CurrentThumbnailWindowHandle;
            if (source == null) {
                Form.AutoShowAllPanels();
                return;
            }

            //TopMost ウィンドウのアクティブ化時、シェルフックは lParam=0 を
            //通知することがある。その場合は実際のフォアグラウンドウィンドウで判定する。
            //(パネル自身のクリック/ドラッグで誤って非表示になるのを防ぐ)
            if (activatedWindow == IntPtr.Zero) {
                activatedWindow = WindowManagerMethods.GetForegroundWindow();
            }

            if (IsSourceActive(activatedWindow, source.Handle)) {
                Form.AutoShowAllPanels();
            }
            else {
                //自アプリのフォームがアクティブな間(パネルのドラッグ中など)は隠さない。
                //※このガードは「隠す」側のみに適用する。復帰側にも適用すると、
                //  自アプリのウィンドウがアクティブなタイミングで対象が再アクティブ化
                //  された場合に再表示の機会を失い、パネルが隠れたままになる。
                if (System.Windows.Forms.Form.ActiveForm != null) {
                    return;
                }
                Form.AutoHideAllPanels();
            }
        }

        /// <summary>
        /// アクティブになったウィンドウが「対象ウィンドウがアクティブ」と
        /// みなせるかどうかを判定する。対象自身、対象がオーナーのウィンドウ
        /// (ダイアログ等)、および自パネル群を含む。
        /// </summary>
        private static bool IsSourceActive(IntPtr activatedWindow, IntPtr sourceWindow) {
            if (activatedWindow == IntPtr.Zero)
                return false;

            //自分のパネルをクリックした場合は非表示にしない
            if (MainForm.IsPanelHandle(activatedWindow))
                return true;

            //自プロセスのウィンドウ(サイドパネル、コンテキストメニュー、
            //ダイアログ等)がアクティブになった場合も非表示にしない
            uint processId;
            WindowManagerMethods.GetWindowThreadProcessId(activatedWindow, out processId);
            if (processId == NativeProcessId)
                return true;

            //オーナーチェーンを辿り、対象ウィンドウに行き着けばアクティブ扱い
            IntPtr hwnd = activatedWindow;
            for (int guard = 0; hwnd != IntPtr.Zero && guard < 32; ++guard) {
                if (hwnd == sourceWindow)
                    return true;
                hwnd = WindowManagerMethods.GetWindow(hwnd, WindowManagerMethods.GetWindowMode.GW_OWNER);
            }

            return false;
        }

        protected override void Shutdown() {
        }
    }

}
