using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OnTopReplicaColorAlert.MessagePumpProcessors;

namespace OnTopReplicaColorAlert {

    /// <summary>
    /// Layout preview: while an entry of the layout history is hovered, the current
    /// panels temporarily take the stored positions, sizes, regions and chrome
    /// (missing panels are created, extra ones hidden). Ending the preview puts
    /// everything back. Nothing is saved meanwhile, and the color detection of
    /// all panels is paused so that the changed regions cannot raise an alarm.
    /// Minimized or hidden panels are shown for the preview (without taking the
    /// focus, which would close the menu) and hidden again afterwards.
    /// </summary>
    static partial class PanelLayoutManager {

        /// <summary>
        /// State of a panel before the preview changed it.
        /// </summary>
        sealed class PreviewOriginal {
            public MainForm Panel;
            public ThumbnailRegion Region;
            public bool Chrome;
            public Point Location;
            public Size ClientSize;
            public bool Hidden;         //hidden by the platform (opacity 0)
            public bool WindowVisible;  //native visibility (auto hide uses SW_HIDE)
            public bool Minimized;
            public bool Paused;
        }

        sealed class PreviewSession {
            public MainForm Primary;
            public string EntryPath;
            public readonly List<PreviewOriginal> Originals = new List<PreviewOriginal>();
            //Panels created for the preview only (closed when it ends)
            public readonly List<MainForm> TempPanels = new List<MainForm>();
            //The panel set was minimized/hidden and has been shown for the preview
            public bool Revealed;
        }

        static PreviewSession _preview;

        /// <summary>
        /// Gets whether the panels currently show a layout preview.
        /// </summary>
        public static bool IsPreviewActive => _preview != null;

        /// <summary>
        /// Shows the given history entry on the current panels (or switches an
        /// active preview to it).
        /// </summary>
        public static void BeginPreview(MainForm primary, PanelLayoutHistory.Entry entry) {
            if (primary == null || primary.IsDisposed || entry == null)
                return;
            if (_preview != null && _preview.Primary != primary)
                EndPreview();
            if (_preview != null && _preview.EntryPath == entry.FilePath)
                return;

            var lines = PanelLayoutHistory.ReadLines(entry);
            if (!HasValidHeader(lines))
                return;
            var layouts = lines.Skip(1).Where(IsPanelLine)
                .Select(line => ParsePanelLayout(ParseTokens(line), true))
                .Where(layout => layout != null)
                .ToList();
            if (layouts.Count == 0)
                return;

            if (_preview == null) {
                _preview = new PreviewSession { Primary = primary };
                var panels = new[] { primary }.Concat(primary.ChildPanels)
                    .Where(p => !p.IsDisposed && p.IsHandleCreated).ToArray();
                foreach (var panel in panels) {
                    _preview.Originals.Add(new PreviewOriginal {
                        Panel = panel,
                        Hidden = Program.Platform.IsHidden(panel),
                        WindowVisible = Native.WindowManagerMethods.IsWindowVisible(panel.Handle),
                        Minimized = panel.IsMinimizedWindow,
                        Paused = IsDetectionPaused(panel)
                    });
                    SetDetectionPaused(panel, true);
                }

                //Show the panel set first: a minimized window reports the off-screen
                //position and must not be moved (see ApplyChrome)
                if (_preview.Originals.Any(o => o.Hidden || !o.WindowVisible || o.Minimized)) {
                    _preview.Revealed = true;
                    //Secondaries first: restoring the primary shows them via Show(),
                    //which activates hidden windows
                    foreach (var original in _preview.Originals.Where(o => o.Panel != primary))
                        RevealPanel(original.Panel);
                    RevealPanel(primary);
                }

                foreach (var original in _preview.Originals) {
                    original.Region = original.Panel.SelectedThumbnailRegion;
                    original.Chrome = original.Panel.IsChromeVisible;
                    original.Location = original.Panel.Location;
                    original.ClientSize = original.Panel.ClientSize;
                }
                Log.Write("Panel layout preview started{0}", _preview.Revealed ? " (panels revealed)" : "");
            }
            _preview.EntryPath = entry.FilePath;

            try {
                //Stored panel i is shown on current panel i (the primary first)
                var panels = _preview.Originals.Select(o => o.Panel)
                    .Concat(_preview.TempPanels)
                    .Where(p => !p.IsDisposed)
                    .ToList();
                while (panels.Count < layouts.Count) {
                    //Without activation: activating a window would close the menu
                    var temp = primary.CreateChildPanel(false);
                    SetDetectionPaused(temp, true);
                    _preview.TempPanels.Add(temp);
                    panels.Add(temp);
                }

                for (int i = 0; i < panels.Count; ++i) {
                    if (i < layouts.Count) {
                        SetPanelHidden(panels[i], false);
                        ApplyPreviewLayout(panels[i], layouts[i].Region, layouts[i].Chrome,
                            layouts[i].Location.Value, layouts[i].ClientSize.Value);
                    }
                    else {
                        SetPanelHidden(panels[i], true);
                    }
                }
            }
            catch (Exception ex) {
                Log.WriteException("Unable to preview panel layout", ex);
            }
        }

        /// <summary>
        /// Ends the layout preview and puts all panels back as they were.
        /// </summary>
        public static void EndPreview() {
            var session = _preview;
            if (session == null)
                return;

            try {
                //Still flagged as previewing: closing panels must not save
                foreach (var temp in session.TempPanels) {
                    if (!temp.IsDisposed)
                        temp.Close();
                }

                foreach (var original in session.Originals) {
                    var panel = original.Panel;
                    if (panel.IsDisposed)
                        continue;
                    //Geometry first: it must not be changed once minimized again
                    ApplyPreviewLayout(panel, original.Region, original.Chrome, original.Location, original.ClientSize);
                    if (!session.Revealed)
                        SetPanelHidden(panel, original.Hidden);
                }

                if (session.Revealed) {
                    //Primary first: minimizing it hides the secondaries on its own
                    foreach (var original in session.Originals.OrderBy(o => o.Panel == session.Primary ? 0 : 1)) {
                        if (!original.Panel.IsDisposed)
                            ConcealPanel(original);
                    }
                }

                foreach (var original in session.Originals) {
                    if (!original.Panel.IsDisposed)
                        SetDetectionPaused(original.Panel, original.Paused);
                }
            }
            catch (Exception ex) {
                Log.WriteException("Unable to end panel layout preview", ex);
            }
            finally {
                _preview = null;
            }
            Log.Write("Panel layout preview ended");
        }

        static void ApplyPreviewLayout(MainForm panel, ThumbnailRegion region, bool chrome, Point location, Size clientSize) {
            if (panel.ThumbnailPanel.IsShowingThumbnail && !SameRegion(panel.SelectedThumbnailRegion, region)) {
                panel.SelectedThumbnailRegion = region;
            }
            if (panel.ThumbnailPanel.IsShowingThumbnail) {
                ApplyChrome(panel, chrome);
            }
            ApplyGeometry(panel, location, clientSize);
        }

        /// <summary>
        /// Shows a minimized or hidden panel without activating it (activation would
        /// close the menu the preview is shown from).
        /// </summary>
        static void RevealPanel(MainForm panel) {
            var handle = panel.Handle;
            if (!Native.WindowManagerMethods.IsWindowVisible(handle) || panel.IsMinimizedWindow) {
                Native.WindowManagerMethods.ShowWindow(handle, Native.WindowManagerMethods.SW_SHOWNOACTIVATE);
            }
            if (Program.Platform.IsHidden(panel)) {
                //Not Platform.RestoreForm: its Show() could activate the window
                panel.Opacity = panel.VisibleOpacity;
            }
            panel.ReassertTopMost();
            panel.UpdateColorAlertIndicator();
        }

        /// <summary>
        /// Puts a revealed panel back into the state it had before the preview.
        /// </summary>
        static void ConcealPanel(PreviewOriginal original) {
            var panel = original.Panel;
            var handle = panel.Handle;
            if (original.Hidden) {
                Program.Platform.HideForm(panel);
            }
            else if (Program.Platform.IsHidden(panel)) {
                panel.Opacity = panel.VisibleOpacity; //hidden as an extra panel by the preview
            }
            if (original.Minimized) {
                Native.WindowManagerMethods.ShowWindow(handle, Native.WindowManagerMethods.SW_SHOWMINNOACTIVE);
            }
            else if (!original.WindowVisible) {
                Native.WindowManagerMethods.ShowWindow(handle, Native.WindowManagerMethods.SW_HIDE);
            }
            panel.UpdateColorAlertIndicator();
        }

        static void SetPanelHidden(MainForm panel, bool hidden) {
            if (Program.Platform.IsHidden(panel) == hidden)
                return;
            if (hidden)
                Program.Platform.HideForm(panel);
            else
                Program.Platform.RestoreForm(panel);
            //Hiding via opacity does not raise a visibility change
            panel.UpdateColorAlertIndicator();
        }

        static bool IsDetectionPaused(MainForm panel) {
            try {
                return panel.MessagePumpManager.Get<ColorDetectionProcessor>().Paused;
            }
            catch {
                return false;
            }
        }

        static void SetDetectionPaused(MainForm panel, bool paused) {
            try {
                panel.MessagePumpManager.Get<ColorDetectionProcessor>().Paused = paused;
            }
            catch { }
            panel.UpdateColorAlertIndicator();
        }
    }
}
