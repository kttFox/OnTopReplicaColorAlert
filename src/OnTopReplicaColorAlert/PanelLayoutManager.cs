using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using OnTopReplicaColorAlert.MessagePumpProcessors;
using OnTopReplicaColorAlert.Properties;

namespace OnTopReplicaColorAlert {

    /// <summary>
    /// Persists the panel layout and restores it on startup. The file consists of
    /// "key=value" tokens separated by '|'. The first line after the header stores
    /// the source window ("window=hwnd,class|title=..."), followed by one line per
    /// panel (the primary first): "pos=x,y|size=w,h|chrome=1|region=x,y,w,h|..."
    /// plus the color detection settings. Optional tokens are simply omitted.
    /// </summary>
    static class PanelLayoutManager {

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
        /// Last known source window and regions. While a source window is attached
        /// the watcher keeps this snapshot up to date; when no window is attached
        /// (at startup or after the source application exits) the watcher uses it
        /// to re-attach all panels as soon as the window (re)appears.
        /// </summary>
        class SourceSnapshot {
            public long Hwnd;
            public string ClassName;
            public string Title;
            //Keyed by panel window; the primary is included like any other panel
            public readonly Dictionary<MainForm, ThumbnailRegion> PanelRegions = new Dictionary<MainForm, ThumbnailRegion>();
            public readonly Dictionary<MainForm, bool> PanelChrome = new Dictionary<MainForm, bool>();
            //Remembered client size of each panel, so the size can be restored on
            //reappear instead of being recomputed from a stale/minimized width.
            public readonly Dictionary<MainForm, System.Drawing.Size> PanelSizes = new Dictionary<MainForm, System.Drawing.Size>();

            public bool HasWindow {
                get { return Hwnd != 0 || !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(ClassName); }
            }
        }

        static readonly SourceSnapshot _snapshot = new SourceSnapshot();
        static System.Windows.Forms.Timer _watcher;

        /// <summary>
        /// Saves the layout of the given secondary panels. Deletes the layout file
        /// when no panel is open, so that nothing is restored on next launch.
        /// </summary>
        /// <summary>
        /// Saves the layout of the primary's secondary panels, including the source
        /// window cloned by the primary and its selected region, so that the whole
        /// panel set can be restored on next launch.
        /// </summary>
        public static void Save(MainForm primary, IEnumerable<MainForm> childPanels) {
            try {
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

                int panelCount = 0;
                foreach (var panel in childPanels) {
                    if (panel.IsDisposed)
                        continue;
                    lines.Add(SerializePanel(panel));
                    panelCount++;
                }

                File.WriteAllLines(LayoutFilePath, lines);
                Log.Write("Panel layout saved ({0} panels)", panelCount);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to save panel layout", ex);
            }
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
                if (lines.Length < 2 || Array.IndexOf(AcceptedHeaders, lines[0]) < 0)
                    return;

                //Read from a pre-rename file: take it over under the current name
                if (path != LayoutFilePath)
                    MigrateLegacyFile(path);

                int count = 0;
                bool primaryDone = false;
                for (int i = 1; i < lines.Length; ++i) {
                    if (string.IsNullOrWhiteSpace(lines[i]))
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
        /// Applies chrome visibility to a panel. Requires a shown thumbnail (the
        /// IsChromeVisible setter refuses to hide the chrome otherwise) and keeps
        /// the stored window position (the setter shifts it by the frame size).
        /// </summary>
        static void ApplyChrome(MainForm form, bool visible) {
            if (form.IsChromeVisible == visible)
                return;
            if (!visible && !form.ThumbnailPanel.IsShowingThumbnail)
                return;

            var location = form.Location;
            var clientSize = form.ClientSize;
            form.IsChromeVisible = visible;
            form.Location = location;
            form.ClientSize = clientSize;
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
            _snapshot.Hwnd = hwnd;
            _snapshot.ClassName = head.Substring(comma + 1);
            _snapshot.Title = line.Substring(titleIdx + TitleToken.Length);

            if (primary.CurrentThumbnailWindowHandle != null)
                return;

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

                    //The thumbnail can be dropped by a transient DWM error while
                    //the source window is still alive: re-attach it and restore
                    //the remembered region instead of leaving the panel blank.
                    if (!primary.ThumbnailPanel.IsShowingThumbnail) {
                        Log.Write("Panel layout: thumbnail dropped while source window is alive, re-attaching");
                        ThumbnailRegion primaryRegion;
                        _snapshot.PanelRegions.TryGetValue(primary, out primaryRegion);
                        primary.SetThumbnail(current, primaryRegion);
                        foreach (var kv in _snapshot.PanelRegions) {
                            if (!kv.Key.IsDisposed && kv.Key.ThumbnailPanel.IsShowingThumbnail) {
                                kv.Key.SelectedThumbnailRegion = kv.Value;
                            }
                        }
                        return;
                    }

                    //Attached: refresh the snapshot with the live state
                    //(the primary is tracked like any other panel)
                    _snapshot.Hwnd = current.Handle.ToInt64();
                    _snapshot.ClassName = current.Class;
                    _snapshot.Title = current.Title;
                    PruneSnapshotPanels();
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

                //対象ウィンドウが起動したら（再出現したら）ウィンドウを表示する（自動非表示設定がONの場合）。
                //自動非表示の復帰と挙動を揃え、フォーカスを奪わずに復元する。
                //SetThumbnail より前に復元することで、最小化中の小さな ClientSize.Width を
                //基準にアスペクト比計算が走り、パネルが縮小してしまうのを防ぐ。
                if (Settings.Default.HideWhenSourceDeactivated && primary.IsHandleCreated &&
                    primary.WindowState == System.Windows.Forms.FormWindowState.Minimized) {
                    Native.WindowManagerMethods.ShowWindow(primary.Handle,
                        Native.WindowManagerMethods.SW_SHOWNOACTIVATE);
                }

                //Attach the primary (propagates the window to all secondary panels)
                primary.SetThumbnail(handle, null);

                //Re-apply the remembered regions (primary included)
                foreach (var kv in _snapshot.PanelRegions) {
                    if (!kv.Key.IsDisposed && kv.Key.ThumbnailPanel.IsShowingThumbnail) {
                        kv.Key.SelectedThumbnailRegion = kv.Value;
                    }
                }

                //Restore the remembered panel sizes: SetThumbnail/region changes
                //recompute the size from the current width, which may still be
                //stale after a restore, so re-apply the size we saw while attached.
                foreach (var kv in _snapshot.PanelSizes) {
                    if (!kv.Key.IsDisposed && kv.Key.ThumbnailPanel.IsShowingThumbnail &&
                        kv.Key.WindowState == System.Windows.Forms.FormWindowState.Normal) {
                        kv.Key.ClientSize = kv.Value;
                    }
                }

                //Re-apply the remembered chrome visibility (requires a shown thumbnail)
                foreach (var kv in _snapshot.PanelChrome) {
                    if (!kv.Key.IsDisposed) {
                        ApplyChrome(kv.Key, kv.Value);
                    }
                }
            };
            timer.Start();
            _watcher = timer;
            Log.Write("Panel layout: source window watcher started");
        }

        /// <summary>
        /// Refreshes the snapshot entries of a single panel from its live state.
        /// The snapshot is only updated while the panel actually shows a thumbnail:
        /// a transient DWM error unsets the thumbnail at the panel level (while the
        /// source window is still alive), and refreshing from that broken state
        /// would wipe the remembered region.
        /// </summary>
        static void RefreshPanelSnapshot(MainForm panel) {
            if (panel.IsDisposed || !panel.ThumbnailPanel.IsShowingThumbnail)
                return;

            var region = panel.SelectedThumbnailRegion;
            if (region != null)
                _snapshot.PanelRegions[panel] = region;
            else
                _snapshot.PanelRegions.Remove(panel);
            _snapshot.PanelChrome[panel] = panel.IsChromeVisible;
            //Only remember a size while the panel is in its normal state, so a
            //minimized/hidden width is never captured as the "good" size.
            if (panel.WindowState == System.Windows.Forms.FormWindowState.Normal)
                _snapshot.PanelSizes[panel] = panel.ClientSize;
        }

        /// <summary>
        /// Removes snapshot entries of disposed panels (the snapshot is updated
        /// incrementally, so stale entries must be pruned explicitly).
        /// </summary>
        static void PruneSnapshotPanels() {
            var stale = new List<MainForm>();
            foreach (var p in _snapshot.PanelRegions.Keys)
                if (p.IsDisposed) stale.Add(p);
            foreach (var p in _snapshot.PanelChrome.Keys)
                if (p.IsDisposed && !stale.Contains(p)) stale.Add(p);
            foreach (var p in _snapshot.PanelSizes.Keys)
                if (p.IsDisposed && !stale.Contains(p)) stale.Add(p);
            foreach (var p in stale) {
                _snapshot.PanelRegions.Remove(p);
                _snapshot.PanelChrome.Remove(p);
                _snapshot.PanelSizes.Remove(p);
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

            tokens.Add("pos=" + panel.Location.X.ToString(inv) + "," + panel.Location.Y.ToString(inv));
            tokens.Add("size=" + panel.ClientSize.Width.ToString(inv) + "," + panel.ClientSize.Height.ToString(inv));
            tokens.Add("chrome=" + (panel.IsChromeVisible ? "1" : "0"));
            tokens.Add("opacity=" + panel.Opacity.ToString("R", inv));
            //Region (fall back to the last known region while no thumbnail is
            //shown: no window attached, or thumbnail dropped by a DWM error).
            //A region deliberately cleared by the user is also removed from the
            //snapshot by the watcher, so the fallback never resurrects it.
            var region = panel.SelectedThumbnailRegion;
            if (region == null && !panel.ThumbnailPanel.IsShowingThumbnail) {
                _snapshot.PanelRegions.TryGetValue(panel, out region);
            }
            if (region != null) {
                var raw = GetRawBounds(region);
                tokens.Add("region=" +
                    raw.X.ToString(inv) + "," + raw.Y.ToString(inv) + "," +
                    raw.Width.ToString(inv) + "," + raw.Height.ToString(inv));
                if (region.Relative) {
                    tokens.Add("regionRel=1");
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
        /// Applies a serialized panel line. When <paramref name="target"/> is given
        /// (the primary), settings are applied to it; otherwise a new secondary
        /// panel is created. Returns the affected panel or null on a malformed line.
        /// </summary>
        static MainForm ApplyPanelLine(MainForm primary, string line, MainForm target) {
            var tokens = ParseTokens(line);

            int[] pos, size;
            if (!TryParseInts(tokens, "pos", 2, out pos) || !TryParseInts(tokens, "size", 2, out size))
                return null;

            var panel = target ?? primary.CreateChildPanel();

            //Chrome affects the client area: apply it before position and size.
            //Remember it as well: it cannot be applied while the source window is
            //missing, and is then re-applied by the watcher on re-attachment.
            string chrome;
            if (tokens.TryGetValue("chrome", out chrome)) {
                bool visible = chrome == "1";
                _snapshot.PanelChrome[panel] = visible;
                ApplyChrome(panel, visible);
            }
            panel.Location = new Point(pos[0], pos[1]);
            panel.ClientSize = new Size(size[0], size[1]);

            string opacityToken;
            double opacity;
            if (tokens.TryGetValue("opacity", out opacityToken) &&
                double.TryParse(opacityToken, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out opacity)) {
                panel.Opacity = Math.Max(0.1, Math.Min(1.0, opacity));
            }

            int[] regionBounds;
            if (TryParseInts(tokens, "region", 4, out regionBounds)) {
                bool relative = tokens.ContainsKey("regionRel") && tokens["regionRel"] == "1";
                var raw = new Rectangle(regionBounds[0], regionBounds[1], regionBounds[2], regionBounds[3]);
                var region = new ThumbnailRegion(raw, relative);
                //Remember for re-attachment; apply now if the window was found
                _snapshot.PanelRegions[panel] = region;
                if (panel.ThumbnailPanel.IsShowingThumbnail) {
                    panel.SelectedThumbnailRegion = region;
                }
            }

            ApplyColorTokens(panel, tokens);

            return panel;
        }
    }
}
