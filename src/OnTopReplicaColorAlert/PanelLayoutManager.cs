using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using OnTopReplicaColorAlert.MessagePumpProcessors;
using OnTopReplicaColorAlert.Properties;

namespace OnTopReplicaColorAlert {

    /// <summary>
    /// Persists the panel layout and restores it on startup. The file consists of
    /// "key=value" tokens separated by '|'. The first line after the header stores
    /// the source window ("window=hwnd,class|title=..."), followed by one line per
    /// panel (the primary first): "pos=x,y|size=w,h|chrome=1|region=x,y,w,h|..."
    /// (plus "regionRel=1" or "regionAnchor=BottomRight" for regions that are not
    /// absolute from the top-left corner) plus the color detection settings. Optional tokens are simply omitted.
    /// Lines starting with '#' (after the header) are metadata and are ignored.
    /// </summary>
    static partial class PanelLayoutManager {

        const string FileName = "PanelLayout.txt";
        const string Header = "#OnTopReplicaColorAlert panels";

        //File names used before the rename: read when the current file is missing,
        //so an existing installation keeps its panel layout. Saves always use FileName.
        static readonly string[] LegacyFileNames = { "PanelLayout v3.txt" };

        //Headers written by earlier versions: keep accepting them on restore
        //(the file is rewritten with the current header on the next save).
        static readonly string[] AcceptedHeaders = {
            Header, "#OnTopReplicaColorAlert panels v3", "#OnTopReplica panels v3"
        };
        const string WindowLinePrefix = "window=";
        const string TitleToken = "|title=";
        const char Sep = '|';
        const char MetadataPrefix = '#';

        static string LayoutFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);

        /// <summary>
        /// Path of the layout file to restore from: the current file, or a legacy
        /// one when the current file does not exist yet. Null if none is present.
        /// </summary>
        static string ExistingLayoutFilePath {
            get {
                if (File.Exists(LayoutFilePath))
                    return LayoutFilePath;

                foreach (var legacy in LegacyFileNames) {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, legacy);
                    if (File.Exists(path)) {
                        Log.Write("Reading panel layout from legacy file {0}", legacy);
                        return path;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Layout of a single panel as it should be: the values read from the
        /// layout file, then kept up to date from the live state while the panel
        /// shows its thumbnail (or from user moves/resizes at any time).
        /// </summary>
        class PanelState {
            public ThumbnailRegion Region; //null = whole window
            public bool Chrome = true;
            public Point? Location;
            public Size? ClientSize;

            /// <summary>
            /// True while this state still has to be (re-)applied to the panel: after
            /// a restore or a re-attachment, until the thumbnail is shown and the
            /// panel is not minimized. While pending, the live state is only an
            /// intermediate one (e.g. chrome forced visible without a thumbnail, size
            /// recomputed from the full source window): it is neither captured into
            /// this state nor saved, the state itself is saved instead.
            /// </summary>
            public bool Pending;
        }

        /// <summary>
        /// Last known source window and panel layouts. While a source window is
        /// attached the watcher keeps this snapshot up to date; when no window is
        /// attached (at startup or after the source application exits) the watcher
        /// uses it to re-attach all panels as soon as the window (re)appears.
        /// </summary>
        class SourceSnapshot {
            public long Hwnd;
            public string ClassName;
            public string Title;
            //Keyed by panel window; the primary is included like any other panel
            public readonly Dictionary<MainForm, PanelState> Panels = new Dictionary<MainForm, PanelState>();

            public bool HasWindow {
                get { return Hwnd != 0 || !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(ClassName); }
            }
        }

        static readonly SourceSnapshot _snapshot = new SourceSnapshot();
        static System.Windows.Forms.Timer _watcher;

        static PanelState GetState(MainForm panel, bool create) {
            PanelState state;
            if (!_snapshot.Panels.TryGetValue(panel, out state) && create) {
                //Start from the live state: the region is only known while shown
                state = new PanelState {
                    Region = panel.SelectedThumbnailRegion,
                    Chrome = panel.IsChromeVisible
                };
                _snapshot.Panels[panel] = state;
            }
            return state;
        }

        /// <summary>
        /// Builds the layout file contents of the primary and its secondary panels,
        /// including the source window cloned by the primary and the selected regions.
        /// </summary>
        public static List<string> BuildLayoutLines(MainForm primary, IEnumerable<MainForm> childPanels) {
            var lines = new List<string> { Header };

            //Title goes last: it may contain any character
            var sourceWindow = primary.CurrentThumbnailWindowHandle;
            if (sourceWindow != null) {
                lines.Add(WindowLinePrefix +
                    sourceWindow.Handle.ToInt64().ToString(CultureInfo.InvariantCulture) + "," +
                    sourceWindow.Class +
                    TitleToken + sourceWindow.Title);
            }
            else if (_snapshot.HasWindow) {
                //No window attached right now (e.g. the source application exited):
                //keep the last known window so it can be restored later
                lines.Add(WindowLinePrefix +
                    _snapshot.Hwnd.ToString(CultureInfo.InvariantCulture) + "," +
                    _snapshot.ClassName +
                    TitleToken + _snapshot.Title);
            }

            //The primary is stored as the first panel line, secondaries follow
            lines.Add(SerializePanel(primary));

            foreach (var panel in childPanels) {
                if (panel.IsDisposed)
                    continue;
                lines.Add(SerializePanel(panel));
            }
            return lines;
        }

        /// <summary>
        /// Saves the layout of the primary and its secondary panels, so that the
        /// whole panel set can be restored on next launch.
        /// </summary>
        public static void Save(MainForm primary, IEnumerable<MainForm> childPanels) {
            try {
                var lines = BuildLayoutLines(primary, childPanels);
                File.WriteAllLines(LayoutFilePath, lines);
                Log.Write("Panel layout saved ({0} panels)", lines.Count(IsPanelLine) - 1);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to save panel layout", ex);
            }
        }

        /// <summary>
        /// Gets whether a layout file line describes a panel (not the header, a
        /// metadata line or the source window line).
        /// </summary>
        internal static bool IsPanelLine(string line) {
            return !string.IsNullOrWhiteSpace(line) &&
                line[0] != MetadataPrefix &&
                !line.StartsWith(WindowLinePrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets whether the given lines start with a recognized layout file header.
        /// </summary>
        internal static bool HasValidHeader(IList<string> lines) {
            return lines != null && lines.Count >= 2 && Array.IndexOf(AcceptedHeaders, lines[0]) >= 0;
        }

        /// <summary>
        /// Copies a layout file written under a pre-rename file name to the current
        /// file name and removes the old one. The old file is only deleted once the
        /// copy exists, so a failure cannot lose the layout.
        /// </summary>
        static void MigrateLegacyFile(string legacyPath) {
            try {
                File.Copy(legacyPath, LayoutFilePath, true);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to migrate legacy panel layout file", ex);
                return;
            }

            try {
                File.Delete(legacyPath);
                Log.Write("Migrated panel layout to {0} and deleted {1}",
                    FileName, Path.GetFileName(legacyPath));
            }
            catch (Exception ex) {
                Log.WriteException("Unable to delete legacy panel layout file", ex);
            }
        }

        /// <summary>
        /// Restores secondary panels bound to the given primary form.
        /// </summary>
        public static void Restore(MainForm primary) {
            try {
                var path = ExistingLayoutFilePath;
                if (path == null)
                    return;

                var lines = File.ReadAllLines(path);
                if (!HasValidHeader(lines))
                    return;

                //Read from a pre-rename file: take it over under the current name
                if (path != LayoutFilePath)
                    MigrateLegacyFile(path);

                //Keep the layout of the previous session in the history, so that it
                //can be brought back if it gets broken during this session
                PanelLayoutHistory.Add(lines, PanelLayoutHistory.EntryKind.Startup, null);

                int count = ApplyLayoutLines(primary, lines);
                Log.Write("Panel layout restored ({0} panels)", count);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to restore panel layout", ex);
            }
            finally {
                //Always watch: attaches the saved source window whenever it appears
                //and keeps the snapshot fresh while a window is attached.
                StartWatcher(primary);
            }
        }

        /// <summary>
        /// Replaces the current panel set with a layout from the history. The current
        /// layout is stored in the history first, so the restoration can be undone.
        /// </summary>
        public static bool RestoreFromHistory(MainForm primary, PanelLayoutHistory.Entry entry) {
            var lines = PanelLayoutHistory.ReadLines(entry);
            if (!HasValidHeader(lines)) {
                Log.Write("Panel layout history: entry {0} is not a valid layout", entry.FilePath);
                return false;
            }

            PanelLayoutHistory.Add(BuildLayoutLines(primary, primary.ChildPanels),
                PanelLayoutHistory.EntryKind.BeforeRestore, null);

            //Show the panel set first: the layout is applied to minimized panels
            //only once they are restored, and new panels would appear alone
            primary.RestoreAllPanels();

            primary.RunWithoutLayoutSave(() => {
                foreach (var child in primary.ChildPanels.ToArray()) {
                    child.Close();
                }
                PruneSnapshotPanels();

                int count = ApplyLayoutLines(primary, lines);
                Log.Write("Panel layout restored from history {0} ({1} panels)", entry.FilePath, count);
                return true;
            });
            return true;
        }

        /// <summary>
        /// Applies the lines of a layout file: the source window, the primary panel
        /// (first panel line) and new secondary panels. Returns the number of
        /// secondary panels created.
        /// </summary>
        static int ApplyLayoutLines(MainForm primary, IList<string> lines) {
            int count = 0;
            bool primaryDone = false;
            for (int i = 1; i < lines.Count; ++i) {
                if (string.IsNullOrWhiteSpace(lines[i]) || lines[i][0] == MetadataPrefix)
                    continue;
                if (lines[i].StartsWith(WindowLinePrefix, StringComparison.Ordinal)) {
                    RestoreSourceWindow(primary, lines[i]);
                    continue;
                }

                //First panel line applies to the primary, the rest spawn secondaries
                if (!primaryDone) {
                    primaryDone = ApplyPanelLine(primary, lines[i], primary) != null;
                }
                else if (ApplyPanelLine(primary, lines[i], null) != null) {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Applies chrome visibility to a panel. Requires a shown thumbnail (the
        /// IsChromeVisible setter refuses to hide the chrome otherwise) and keeps
        /// the stored window position (the setter shifts it by the frame size).
        /// </summary>
        static void ApplyChrome(MainForm form, bool visible) {
            if (form.IsChromeVisible == visible)
                return;
            if (!visible && !form.ThumbnailPanel.IsShowingThumbnail)
                return;
            //The setter moves the window: while minimized WinForms would record the
            //off-screen position (-32000) as the position to restore to
            if (form.IsMinimizedWindow)
                return;

            var location = form.Location;
            var clientSize = form.ClientSize;
            form.IsChromeVisible = visible;
            form.Location = location;
            form.ClientSize = clientSize;
        }

        /// <summary>
        /// Moves and resizes a panel to the given geometry. The panel is moved first
        /// and resized afterwards: moving it to a monitor with a different DPI makes
        /// Windows rescale the window (WM_DPICHANGED), so the size is only final once
        /// the panel is on its target monitor.
        /// </summary>
        static bool ApplyGeometry(MainForm panel, Point location, Size clientSize) {
            bool changed = false;
            if (panel.Location != location) {
                panel.Location = location;
                changed = true;
            }
            if (panel.ClientSize != clientSize) {
                panel.ClientSize = clientSize;
                changed = true;
            }
            if (panel.Location != location) {
                panel.Location = location;
                changed = true;
            }
            return changed;
        }

        static bool SameRegion(ThumbnailRegion a, ThumbnailRegion b) {
            if (a == null || b == null)
                return a == b;
            return a.Relative == b.Relative && a.Anchor == b.Anchor && GetRawBounds(a) == GetRawBounds(b);
        }

        /// <summary>
        /// Applies a pending panel state as far as currently possible. The region is
        /// applied as soon as the thumbnail is shown (the color detection depends on
        /// it, even while hidden), chrome and geometry once the panel is not
        /// minimized. The geometry goes last: region and chrome changes recompute
        /// the size from the current width. Returns whether anything changed.
        /// </summary>
        static bool ApplyPending(MainForm panel, PanelState state) {
            if (!state.Pending || panel.IsDisposed || panel.Disposing)
                return false;

            bool changed = false;
            bool showing = panel.ThumbnailPanel.IsShowingThumbnail;
            if (showing && !SameRegion(panel.SelectedThumbnailRegion, state.Region)) {
                panel.SelectedThumbnailRegion = state.Region;
                //Updating the region can drop the thumbnail on a transient DWM error
                showing = panel.ThumbnailPanel.IsShowingThumbnail;
                changed = true;
            }

            if (panel.IsMinimizedWindow)
                return changed;

            if (showing && panel.IsChromeVisible != state.Chrome) {
                ApplyChrome(panel, state.Chrome);
                changed = true;
            }

            if (state.Location.HasValue && state.ClientSize.HasValue) {
                changed |= ApplyGeometry(panel, state.Location.Value, state.ClientSize.Value);
            }

            //Done once everything could be applied; otherwise retried by the watcher
            if (showing && panel.IsChromeVisible == state.Chrome &&
                SameRegion(panel.SelectedThumbnailRegion, state.Region)) {
                state.Pending = false;
            }
            return changed;
        }

        /// <summary>
        /// Applies the pending states of the whole panel set. Returns whether
        /// anything changed.
        /// </summary>
        static bool ApplyPendingAll(MainForm primary) {
            bool changed = false;
            foreach (var panel in new[] { primary }.Concat(primary.ChildPanels).ToArray()) {
                var state = GetState(panel, false);
                if (state != null)
                    changed |= ApplyPending(panel, state);
            }
            return changed;
        }

        static bool AnyPending() {
            foreach (var kv in _snapshot.Panels) {
                if (kv.Value.Pending && !kv.Key.IsDisposed)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Applies the pending layout of the panel set of the given panel (e.g. once
        /// the primary is restored from the minimized state).
        /// </summary>
        public static void ApplyPendingLayout(MainForm panel) {
            if (panel.IsDisposed)
                return;
            var primary = panel.PrimaryPanel;
            //Revealed for a preview: applied once really restored
            if (primary.IsDisposed || !AnyPending() || IsPreviewActive)
                return;
            primary.RunWithoutLayoutSave(() => ApplyPendingAll(primary));
        }

        /// <summary>
        /// Records the current position and size of a panel as its layout. Called
        /// when the user finishes moving or resizing it, so that the new geometry is
        /// kept even while a restoration is pending (e.g. no source window yet).
        /// </summary>
        public static void NotePanelGeometry(MainForm panel) {
            if (panel.IsDisposed || panel.IsMinimizedWindow)
                return;
            var state = GetState(panel, true);
            state.Location = panel.VisibleLocation;
            state.ClientSize = panel.VisibleClientSize;
        }

        /// <summary>
        /// Re-attaches the primary panel to the saved source window, unless the
        /// primary is already cloning something (e.g. via command line options
        /// or the "restore last window" setting).
        /// </summary>
        static void RestoreSourceWindow(MainForm primary, string line) {
            //Format: window=<hwnd>,<class>|title=<title>  (title parsed as raw tail)
            int titleIdx = line.IndexOf(TitleToken, StringComparison.Ordinal);
            if (titleIdx < 0)
                return;
            string head = line.Substring(WindowLinePrefix.Length, titleIdx - WindowLinePrefix.Length);
            int comma = head.IndexOf(',');
            if (comma < 0)
                return;

            long hwnd;
            if (!long.TryParse(head.Substring(0, comma), NumberStyles.Integer, CultureInfo.InvariantCulture, out hwnd))
                return;

            if (primary.CurrentThumbnailWindowHandle != null)
                return;

            _snapshot.Hwnd = hwnd;
            _snapshot.ClassName = head.Substring(comma + 1);
            _snapshot.Title = line.Substring(titleIdx + TitleToken.Length);

            var handle = SeekWindow();
            if (handle != null) {
                Log.Write("Panel layout: restoring source window '{0}'", handle.Title);
                primary.SetThumbnail(handle, null);
            }
            else {
                Log.WriteDetails("Panel layout: source window not found (will watch for it)",
                    "HWND {0}, Title '{1}', Class '{2}'", _snapshot.Hwnd, _snapshot.Title, _snapshot.ClassName);
            }
        }

        /// <summary>
        /// Seeks the remembered source window among the currently open windows.
        /// </summary>
        static WindowHandle SeekWindow() {
            var seeker = new WindowSeekers.RestoreWindowSeeker(
                    new IntPtr(_snapshot.Hwnd), _snapshot.Title, _snapshot.ClassName) {
                SkipNotVisibleWindows = true
            };
            seeker.Refresh();

            foreach (var w in seeker.Windows) {
                //A stale stored HWND can be re-used by Windows for one of our own
                //panel windows: cloning ourselves makes DWM throw. Skip them.
                if (MainForm.IsPanelHandle(w.Handle))
                    continue;
                return w;
            }
            return null;
        }

        /// <summary>
        /// Marks the layout of every tracked panel as to be re-applied (after the
        /// source window was re-attached or a thumbnail was re-created).
        /// </summary>
        static void MarkPending(IEnumerable<MainForm> panels) {
            foreach (var panel in panels) {
                var state = GetState(panel, false);
                if (state != null && !panel.IsDisposed)
                    state.Pending = true;
            }
        }

        /// <summary>
        /// Starts (or re-targets, e.g. after primary promotion) the permanent source
        /// window watcher. While a window is attached the snapshot is kept fresh;
        /// whenever no window is attached, the watcher re-attaches the remembered
        /// window as soon as it appears.
        /// </summary>
        public static void StartWatcher(MainForm primary) {
            if (_watcher != null) {
                _watcher.Stop();
                _watcher.Dispose();
                _watcher = null;
            }

            var timer = new System.Windows.Forms.Timer { Interval = 2000 };
            timer.Tick += (s, e) => {
                if (primary.IsDisposed) {
                    timer.Stop();
                    return;
                }
                //The panels show a temporary preview: do not capture or re-apply it
                if (IsPreviewActive)
                    return;

                var current = primary.CurrentThumbnailWindowHandle;
                if (current != null) {
                    //The source window may close without any thumbnail error being
                    //raised: detect it actively so the watcher can start seeking.
                    if (!Native.WindowManagerMethods.IsWindow(current.Handle)) {
                        Log.Write("Panel layout: source window is gone, watching for it to reappear");
                        primary.UnsetThumbnail(true);
                        primary.StopColorAlertForSourceLoss();
                        return;
                    }

                    PruneSnapshotPanels();

                    //A thumbnail can be dropped by a transient DWM error while the
                    //source window is still alive: re-attach it and restore the
                    //remembered layout instead of leaving the panel blank.
                    if (ReattachDroppedThumbnails(primary, current))
                        return;

                    //Layout still waiting to be applied (e.g. panel was minimized)
                    if (AnyPending()) {
                        primary.RunWithoutLayoutSave(() => ApplyPendingAll(primary));
                    }

                    //Attached: refresh the snapshot with the live state
                    //(the primary is tracked like any other panel)
                    _snapshot.Hwnd = current.Handle.ToInt64();
                    _snapshot.ClassName = current.Class;
                    _snapshot.Title = current.Title;
                    RefreshPanelSnapshot(primary);
                    foreach (var child in primary.ChildPanels) {
                        RefreshPanelSnapshot(child);
                    }
                    return;
                }

                //Not attached: look for the remembered window
                if (!_snapshot.HasWindow)
                    return;
                var handle = SeekWindow();
                if (handle == null) {
                    //対象ウィンドウが未アタッチかつ見つからない = 喪失状態。
                    //DWM エラー等で targetLost=false のまま UnsetThumbnail された経路でも
                    //確実にカラーアラートを停止する(設定が有効な場合、一度きり)。
                    primary.StopColorAlertForSourceLoss();
                    return;
                }

                Log.Write("Panel layout: source window appeared, restoring '{0}'", handle.Title);

                //再アタッチは「サムネイル設定 → 領域 → 枠 → 位置・サイズ」の複数手順になる。
                //途中の状態(領域なし・全体表示のアスペクト比で計算されたサイズなど)が
                //保存されないよう、全手順を終えてから一度だけ保存する。
                primary.RunWithoutLayoutSave(() => {
                    //対象ウィンドウが起動したら（再出現したら）ウィンドウを表示する（自動非表示設定がONの場合）。
                    //自動非表示の復帰と挙動を揃え、フォーカスを奪わずに復元する。
                    if (Settings.Default.HideWhenSourceDeactivated && primary.IsHandleCreated &&
                        primary.WindowState == System.Windows.Forms.FormWindowState.Minimized) {
                        Native.WindowManagerMethods.ShowWindow(primary.Handle,
                            Native.WindowManagerMethods.SW_SHOWNOACTIVATE);
                    }

                    //Everything recomputed by the re-attachment must be put back as it was
                    MarkPending(new[] { primary }.Concat(primary.ChildPanels));

                    //Attach the primary (propagates the window to all secondary panels)
                    primary.SetThumbnail(handle, null);

                    //Re-apply the remembered regions, chrome, positions and sizes
                    ApplyPendingAll(primary);
                    return true;
                });
            };
            timer.Start();
            _watcher = timer;
            Log.Write("Panel layout: source window watcher started");
        }

        /// <summary>
        /// Re-creates the thumbnails dropped by a transient DWM error (the source
        /// window is still alive) and re-applies the remembered layout of those
        /// panels. Returns whether anything had to be re-attached.
        /// </summary>
        static bool ReattachDroppedThumbnails(MainForm primary, WindowHandle current) {
            //Only panels still bound to the window: a panel whose thumbnail setup
            //threw has been unset explicitly (and reported to the user) already
            var dropped = new[] { primary }.Concat(primary.ChildPanels)
                .Where(p => !p.IsDisposed && p.CurrentThumbnailWindowHandle != null &&
                            !p.ThumbnailPanel.IsShowingThumbnail)
                .ToArray();
            if (dropped.Length == 0)
                return false;

            Log.Write("Panel layout: {0} thumbnail(s) dropped while source window is alive, re-attaching", dropped.Length);
            primary.RunWithoutLayoutSave(() => {
                MarkPending(dropped);
                foreach (var panel in dropped) {
                    if (panel.ThumbnailPanel.IsShowingThumbnail)
                        continue; //re-attached along with the primary
                    var state = GetState(panel, false);
                    panel.SetThumbnail(current, state != null ? state.Region : null);
                }
                ApplyPendingAll(primary);
                //Nothing to save while the DWM error persists (retried next tick)
                return dropped.Any(p => p.ThumbnailPanel.IsShowingThumbnail);
            });
            return true;
        }

        /// <summary>
        /// Refreshes the snapshot entry of a single panel from its live state.
        /// The snapshot is only updated while the panel actually shows a thumbnail
        /// and no restoration is pending: a transient DWM error unsets the thumbnail
        /// at the panel level (while the source window is still alive), and
        /// refreshing from that broken or half-restored state would wipe the
        /// remembered layout.
        /// </summary>
        static void RefreshPanelSnapshot(MainForm panel) {
            if (panel.IsDisposed || !panel.ThumbnailPanel.IsShowingThumbnail)
                return;

            var state = GetState(panel, true);
            if (state.Pending)
                return;

            state.Region = panel.SelectedThumbnailRegion;
            state.Chrome = panel.IsChromeVisible;
            //Only remember the geometry of a panel in its normal state, so a
            //minimized/hidden position or size is never captured as the "good" one.
            if (!panel.IsMinimizedWindow) {
                state.Location = panel.VisibleLocation;
                state.ClientSize = panel.VisibleClientSize;
            }
        }

        /// <summary>
        /// Removes snapshot entries of disposed panels (the snapshot is updated
        /// incrementally, so stale entries must be pruned explicitly).
        /// </summary>
        static void PruneSnapshotPanels() {
            var stale = _snapshot.Panels.Keys.Where(p => p.IsDisposed).ToList();
            foreach (var p in stale) {
                _snapshot.Panels.Remove(p);
            }
        }

        /// <summary>
        /// Gets the raw internal rectangle of a region, valid in both relative
        /// (padding) and absolute (bounds) mode.
        /// </summary>
        static Rectangle GetRawBounds(ThumbnailRegion region) {
            if (region.Relative) {
                var p = region.BoundsAsPadding;
                return new Rectangle(p.Left, p.Top, p.Right, p.Bottom);
            }
            return region.Bounds;
        }

        /// <summary>
        /// Appends the color detection settings of a panel as "key=value" tokens.
        /// </summary>
        static void AppendColorTokens(MainForm panel, List<string> tokens) {
            try {
                var proc = panel.MessagePumpManager.Get<ColorDetectionProcessor>();
                var inv = CultureInfo.InvariantCulture;

                tokens.Add("alert=" + (proc.Enabled ? "1" : "0"));
                tokens.Add("paused=" + (proc.Paused ? "1" : "0"));
                var categories = string.Join(";", proc.EnabledCategories);
                if (categories.Length > 0) {
                    tokens.Add("colors=" + categories);
                }
                tokens.Add("interval=" + proc.SampleInterval.ToString(inv));
                if (proc.CustomTargetColor.HasValue) {
                    tokens.Add("custom=" + ((uint)proc.CustomTargetColor.Value.ToArgb()).ToString("X8", inv));
                }
                tokens.Add("volume=" + ((int)Math.Round(proc.AlarmVolume * 100)).ToString(inv));
                if (!string.IsNullOrEmpty(proc.AlarmSoundFile)) {
                    //File path or "system:" pseudo-path: cannot contain the separator
                    tokens.Add("sound=" + proc.AlarmSoundFile);
                }
                tokens.Add("losson=" + (proc.AlertOnLoss ? "1" : "0"));
                tokens.Add("darkskip=" + (proc.IgnoreDarkFrames ? "1" : "0"));
                tokens.Add("lossmiss=" + proc.LossMissThreshold.ToString(inv));
                tokens.Add("minpx=" + proc.MinDetectionPixels.ToString(inv));
                tokens.Add("keyon=" + (proc.KeyPressEnabled ? "1" : "0"));
                if (proc.KeyPressKey != System.Windows.Forms.Keys.None) {
                    tokens.Add("key=" + ((int)proc.KeyPressKey).ToString(inv));
                }
            }
            catch { }
        }

        /// <summary>
        /// Applies color detection tokens to a panel's processor.
        /// </summary>
        static void ApplyColorTokens(MainForm panel, Dictionary<string, string> tokens) {
            try {
                var proc = panel.MessagePumpManager.Get<ColorDetectionProcessor>();
                var inv = CultureInfo.InvariantCulture;
                string v;

                if (tokens.TryGetValue("colors", out v)) {
                    var cats = new HashSet<ColorCategory>();
                    foreach (var c in v.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                        ColorCategory cat;
                        if (Enum.TryParse(c, out cat) && cat != ColorCategory.None)
                            cats.Add(cat);
                    }
                    //Apply even when empty: the token reflects the saved state,
                    //otherwise the processor's default (Red) would incorrectly remain
                    proc.EnabledCategories = cats;
                }

                int iv;
                if (tokens.TryGetValue("interval", out v) &&
                    int.TryParse(v, NumberStyles.Integer, inv, out iv)) {
                    proc.SampleInterval = iv;
                }

                uint argb;
                if (tokens.TryGetValue("custom", out v) &&
                    uint.TryParse(v, NumberStyles.HexNumber, inv, out argb)) {
                    proc.CustomTargetColor = Color.FromArgb((int)argb);
                }

                int vol;
                if (tokens.TryGetValue("volume", out v) &&
                    int.TryParse(v, NumberStyles.Integer, inv, out vol)) {
                    proc.AlarmVolume = vol / 100f;
                }

                if (tokens.TryGetValue("sound", out v) && v.Length > 0) {
                    proc.AlarmSoundFile = v;
                }

                int keyCode;
                if (tokens.TryGetValue("key", out v) &&
                    int.TryParse(v, NumberStyles.Integer, inv, out keyCode)) {
                    proc.KeyPressKey = (System.Windows.Forms.Keys)keyCode;
                }

                if (tokens.TryGetValue("keyon", out v)) {
                    proc.KeyPressEnabled = v == "1";
                }

                if (tokens.TryGetValue("losson", out v)) {
                    proc.AlertOnLoss = v == "1";
                }

                if (tokens.TryGetValue("darkskip", out v)) {
                    proc.IgnoreDarkFrames = v == "1";
                }

                int lossMiss;
                if (tokens.TryGetValue("lossmiss", out v) &&
                    int.TryParse(v, NumberStyles.Integer, inv, out lossMiss)) {
                    proc.LossMissThreshold = lossMiss;
                }

                int minPx;
                if (tokens.TryGetValue("minpx", out v) &&
                    int.TryParse(v, NumberStyles.Integer, inv, out minPx)) {
                    proc.MinDetectionPixels = minPx;
                }

                if (tokens.TryGetValue("alert", out v)) {
                    proc.Enabled = v == "1";
                }

                //Enabled の後に適用する(Enabled セッターがインジケーター等を更新するため)
                if (tokens.TryGetValue("paused", out v)) {
                    proc.Paused = v == "1";
                }
            }
            catch { }
        }

        static string SerializePanel(MainForm panel) {
            var inv = CultureInfo.InvariantCulture;
            var tokens = new List<string>();

            //復元待ちのパネルは、途中状態(枠の強制表示、全体表示を基準に計算された
            //サイズ、未適用の領域)ではなく、復元しようとしているレイアウトを保存する
            var state = GetState(panel, false);
            bool pending = state != null && state.Pending;

            //最小化中の画面外座標・最小化枠のサイズではなく、表示時の位置とサイズを保存する
            var pos = panel.VisibleLocation;
            var size = panel.VisibleClientSize;
            if (pending && state.Location.HasValue && state.ClientSize.HasValue) {
                pos = state.Location.Value;
                size = state.ClientSize.Value;
            }
            tokens.Add("pos=" + pos.X.ToString(inv) + "," + pos.Y.ToString(inv));
            tokens.Add("size=" + size.Width.ToString(inv) + "," + size.Height.ToString(inv));
            bool chrome = pending ? state.Chrome : panel.IsChromeVisible;
            tokens.Add("chrome=" + (chrome ? "1" : "0"));
            //隠されている(Opacity=0)パネルの見た目の値ではなく、表示時の透明度を保存する
            tokens.Add("opacity=" + panel.VisibleOpacity.ToString("R", inv));
            //Region (fall back to the last known region while no thumbnail is
            //shown: no window attached, or thumbnail dropped by a DWM error).
            //A region deliberately cleared by the user is also removed from the
            //snapshot by the watcher, so the fallback never resurrects it.
            ThumbnailRegion region;
            if (pending)
                region = state.Region;
            else if (panel.ThumbnailPanel.IsShowingThumbnail)
                region = panel.SelectedThumbnailRegion;
            else
                region = state != null ? state.Region : null;
            if (region != null) {
                var raw = GetRawBounds(region);
                tokens.Add("region=" +
                    raw.X.ToString(inv) + "," + raw.Y.ToString(inv) + "," +
                    raw.Width.ToString(inv) + "," + raw.Height.ToString(inv));
                if (region.Relative) {
                    tokens.Add("regionRel=1");
                }
                else if (region.Anchor != RegionAnchor.TopLeft) {
                    tokens.Add("regionAnchor=" + region.Anchor);
                }
            }

            AppendColorTokens(panel, tokens);

            return string.Join(Sep.ToString(), tokens);
        }

        /// <summary>
        /// Splits a line into "key=value" tokens. Tokens without '=' are ignored.
        /// </summary>
        static Dictionary<string, string> ParseTokens(string line) {
            var tokens = new Dictionary<string, string>();
            foreach (var token in line.Split(Sep)) {
                int eq = token.IndexOf('=');
                if (eq <= 0)
                    continue;
                tokens[token.Substring(0, eq)] = token.Substring(eq + 1);
            }
            return tokens;
        }

        //パネルの最小クライアントサイズ。これより小さい値が保存されていても操作可能に保つ。
        const int MinPanelClientSize = 40;

        /// <summary>
        /// 保存されたクライアントサイズを、操作可能な最小値と、パネルが置かれる
        /// 画面の作業領域の範囲内に収める。
        /// </summary>
        static Size SanitizeClientSize(Size size, Point location) {
            var workingArea = System.Windows.Forms.Screen.FromRectangle(new Rectangle(location, size)).WorkingArea;
            return new Size(
                Math.Min(Math.Max(size.Width, MinPanelClientSize), workingArea.Width),
                Math.Min(Math.Max(size.Height, MinPanelClientSize), workingArea.Height));
        }

        /// <summary>
        /// 保存された位置を、いずれかの画面の作業領域と重なる位置へ補正する。
        /// どの画面にも掛からない場合(最小化中に保存された -32000,-32000 や、
        /// 取り外されたモニターの座標)は、最も近い画面の中に収める。
        /// </summary>
        static Point SanitizeLocation(Point location, Size windowSize) {
            var bounds = new Rectangle(location, windowSize);
            foreach (var screen in System.Windows.Forms.Screen.AllScreens) {
                if (screen.WorkingArea.IntersectsWith(bounds))
                    return location;
            }

            var workingArea = System.Windows.Forms.Screen.FromPoint(location).WorkingArea;
            Log.Write("Panel layout: stored position {0},{1} is off-screen, moving into {2}",
                location.X, location.Y, workingArea);
            return new Point(
                Math.Min(Math.Max(location.X, workingArea.Left), Math.Max(workingArea.Right - windowSize.Width, workingArea.Left)),
                Math.Min(Math.Max(location.Y, workingArea.Top), Math.Max(workingArea.Bottom - windowSize.Height, workingArea.Top)));
        }

        /// <summary>
        /// Parses a token value holding a fixed number of comma-separated integers.
        /// </summary>
        static bool TryParseInts(Dictionary<string, string> tokens, string key, int count, out int[] values) {
            values = null;
            string v;
            if (!tokens.TryGetValue(key, out v))
                return false;

            var parts = v.Split(',');
            if (parts.Length != count)
                return false;

            var result = new int[count];
            for (int i = 0; i < count; ++i) {
                if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out result[i]))
                    return false;
            }
            values = result;
            return true;
        }

        /// <summary>
        /// Parses the layout part of a serialized panel line (position, size, chrome,
        /// region). The position and size are corrected so that the panel stays
        /// visible and usable. Returns null on a malformed line.
        /// </summary>
        static PanelState ParsePanelLayout(Dictionary<string, string> tokens, bool defaultChrome) {
            int[] pos, size;
            if (!TryParseInts(tokens, "pos", 2, out pos) || !TryParseInts(tokens, "size", 2, out size))
                return null;

            var state = new PanelState();

            string chrome;
            state.Chrome = tokens.TryGetValue("chrome", out chrome) ? chrome == "1" : defaultChrome;

            int[] regionBounds;
            if (TryParseInts(tokens, "region", 4, out regionBounds)) {
                bool relative = tokens.ContainsKey("regionRel") && tokens["regionRel"] == "1";
                var raw = new Rectangle(regionBounds[0], regionBounds[1], regionBounds[2], regionBounds[3]);
                state.Region = new ThumbnailRegion(raw, relative);

                //基準位置(省略時・不明な値は左上 = 旧バージョンと同じ)
                string anchorToken;
                RegionAnchor anchor;
                if (!relative && tokens.TryGetValue("regionAnchor", out anchorToken) &&
                    Enum.TryParse(anchorToken, false, out anchor) && Enum.IsDefined(typeof(RegionAnchor), anchor)) {
                    state.Region.Anchor = anchor;
                }
            }

            //古いバージョンが最小化中に保存した壊れた値(画面外座標や極小サイズ)でも
            //パネルが見えなくならないよう、画面内に収まる値へ補正する
            var storedSize = new Size(Math.Max(size[0], MinPanelClientSize), Math.Max(size[1], MinPanelClientSize));
            var location = SanitizeLocation(new Point(pos[0], pos[1]), storedSize);
            state.Location = location;
            state.ClientSize = SanitizeClientSize(storedSize, location);
            return state;
        }

        /// <summary>
        /// Applies a serialized panel line. When <paramref name="target"/> is given
        /// (the primary), settings are applied to it; otherwise a new secondary
        /// panel is created. Returns the affected panel or null on a malformed line.
        /// </summary>
        static MainForm ApplyPanelLine(MainForm primary, string line, MainForm target) {
            var tokens = ParseTokens(line);

            //A new panel starts with its chrome visible
            var parsed = ParsePanelLayout(tokens, target == null || target.IsChromeVisible);
            if (parsed == null)
                return null;

            var panel = target ?? primary.CreateChildPanel();

            //The line describes the whole panel layout: it replaces what is known.
            //Chrome and region cannot be applied while the source window is
            //missing: they stay pending and are applied by the watcher once the
            //thumbnail is shown. Meanwhile saves keep writing these values.
            var state = GetState(panel, true);
            state.Chrome = parsed.Chrome;
            state.Region = parsed.Region;
            state.Location = parsed.Location;
            state.ClientSize = parsed.ClientSize;
            state.Pending = true;
            var location = parsed.Location.Value;
            var clientSize = parsed.ClientSize.Value;

            //Apply what can be applied now (everything when the window was found)
            ApplyPending(panel, state);
            //Minimized: the geometry is applied on restore, report it meanwhile
            if (panel.IsMinimizedWindow)
                panel.SeedVisibleGeometry(location, clientSize);

            string opacityToken;
            double opacity;
            if (tokens.TryGetValue("opacity", out opacityToken) &&
                double.TryParse(opacityToken, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out opacity)) {
                panel.VisibleOpacity = opacity; //セッター側でクランプされる
            }

            ApplyColorTokens(panel, tokens);

            return panel;
        }
    }
}
