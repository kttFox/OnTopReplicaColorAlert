using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace OnTopReplicaColorAlert {

    /// <summary>
    /// "Layout history" submenu of the tray icon menu:
    /// saves the current panel layout and restores past layouts (entries are
    /// deleted by removing their file from the history folder).
    /// Entries are listed when the menu opens, so the list is always current.
    /// Hovering an entry previews it on the current panels.
    /// </summary>
    static class LayoutHistoryMenu {

        //Hover handling: the preview starts/ends after a short delay, so that moving
        //the mouse across the menu does not rearrange the panels on every item and
        //moving from one entry to the next switches directly between the two
        const int PreviewStartDelay = 200;
        const int PreviewEndDelay = 250;

        static readonly Timer _previewTimer = new Timer();
        static Action _previewAction;
        //True while the restore confirmation is shown: the preview is kept meanwhile
        static bool _confirming;

        static LayoutHistoryMenu() {
            _previewTimer.Tick += (s, e) => {
                _previewTimer.Stop();
                var action = _previewAction;
                _previewAction = null;
                if (!_confirming && action != null)
                    action();
            };
        }

        static void SchedulePreview(int delay, Action action) {
            _previewTimer.Stop();
            _previewAction = action;
            _previewTimer.Interval = delay;
            _previewTimer.Start();
        }

        static void CancelScheduledPreview() {
            _previewTimer.Stop();
            _previewAction = null;
        }

        /// <summary>
        /// Starts previewing an entry, keeping the menu above the moved panels.
        /// </summary>
        static void StartPreview(MainForm primary, PanelLayoutHistory.Entry entry, ToolStripDropDown menu) {
            PanelLayoutManager.BeginPreview(primary, entry);
            //The panels are topmost as well: new ones may appear above the menu
            for (ToolStrip strip = menu; strip != null; strip = (strip as ToolStripDropDown)?.OwnerItem?.Owner) {
                if (strip.IsHandleCreated && strip.Visible) {
                    Native.WindowManagerMethods.SetWindowPos(strip.Handle, Native.WindowManagerMethods.HWND_TOPMOST, 0, 0, 0, 0,
                        Native.WindowManagerMethods.SWP_NOMOVE | Native.WindowManagerMethods.SWP_NOSIZE |
                        Native.WindowManagerMethods.SWP_NOACTIVATE);
                }
            }
        }

        /// <summary>
        /// Creates the menu item. <paramref name="getPrimary"/> resolves the primary
        /// panel when the menu is used (it changes on primary promotion).
        /// </summary>
        public static ToolStripMenuItem Create(Func<MainForm> getPrimary) {
            var root = new ToolStripMenuItem(Strings.MenuLayoutHistory) {
                ToolTipText = Strings.MenuLayoutHistoryTT
            };
            //Placeholder: a drop down arrow is only shown when there are items
            root.DropDownItems.Add(new ToolStripMenuItem(Strings.MenuLayoutHistoryEmpty) { Enabled = false });
            root.DropDownOpening += (s, e) => {
                //The parent menu (tray menu) is known only once the item is added to it
                if (root.Owner is ToolStripDropDown parent && !(parent.Tag is FocusCloseGuard)) {
                    parent.Tag = FocusCloseGuard.Instance;
                    parent.Closing += (ps, pe) => KeepOpenDuringPreview(root.DropDown, pe);
                }
                Populate(root, getPrimary);
            };
            root.DropDown.Closing += (s, e) => KeepOpenDuringPreview(root.DropDown, e);
            //Menu gone (item clicked, dismissed): put the panels back
            root.DropDown.Closed += (s, e) => SchedulePreview(PreviewEndDelay, PanelLayoutManager.EndPreview);
            Asztal.Szótár.NativeToolStripRenderer.SetToolStripRenderer(root.DropDown);
            return root;
        }

        /// <summary>
        /// Marks a parent menu whose Closing is already guarded.
        /// </summary>
        sealed class FocusCloseGuard {
            public static readonly FocusCloseGuard Instance = new FocusCloseGuard();
        }

        /// <summary>
        /// Showing, creating or rearranging panels for the preview can move the focus
        /// away from the menu (especially the tray menu), which closes it with
        /// <see cref="ToolStripDropDownCloseReason.AppFocusChange"/>. While a preview is
        /// shown or pending and the mouse is still on the menu, that close is canceled.
        /// </summary>
        static void KeepOpenDuringPreview(ToolStripDropDown historyMenu, ToolStripDropDownClosingEventArgs e) {
            if (e.CloseReason != ToolStripDropDownCloseReason.AppFocusChange)
                return;
            if (!PanelLayoutManager.IsPreviewActive && _previewAction == null)
                return;
            var cursor = Cursor.Position;
            for (ToolStrip strip = historyMenu; strip != null; strip = (strip as ToolStripDropDown)?.OwnerItem?.Owner) {
                if (strip.Visible && strip.Bounds.Contains(cursor)) {
                    e.Cancel = true;
                    return;
                }
            }
        }

        static void Populate(ToolStripMenuItem root, Func<MainForm> getPrimary) {
            root.DropDownItems.Clear();
            //Saving/restoring needs a panel set (none while shutting down)
            bool hasPrimary = getPrimary() != null;

            root.DropDownItems.Add(new ToolStripMenuItem(Strings.MenuLayoutHistorySave, null, (s, e) => SaveCurrent(getPrimary())) {
                ToolTipText = Strings.MenuLayoutHistorySaveTT,
                Enabled = hasPrimary
            });
            root.DropDownItems.Add(new ToolStripSeparator());

            var entries = PanelLayoutHistory.GetEntries();
            if (entries.Count == 0) {
                root.DropDownItems.Add(new ToolStripMenuItem(Strings.MenuLayoutHistoryEmpty) { Enabled = false });
            }
            foreach (var entry in entries) {
                var current = entry;
                //Entries are removed by deleting their file in the history folder
                var item = new ToolStripMenuItem(FormatEntry(entry), null, (s, e) => Restore(getPrimary(), current)) {
                    ToolTipText = Strings.MenuLayoutHistoryRestore,
                    Enabled = hasPrimary
                };
                item.MouseEnter += (s, e) => SchedulePreview(PreviewStartDelay,
                    () => StartPreview(getPrimary(), current, root.DropDown));
                item.MouseLeave += (s, e) => SchedulePreview(PreviewEndDelay, PanelLayoutManager.EndPreview);
                root.DropDownItems.Add(item);
            }

            root.DropDownItems.Add(new ToolStripSeparator());
            root.DropDownItems.Add(new ToolStripMenuItem(Strings.MenuLayoutHistoryOpenFolder, null, (s, e) => OpenFolder()));
        }

        static string KindLabel(PanelLayoutHistory.EntryKind kind) {
            switch (kind) {
                case PanelLayoutHistory.EntryKind.Startup: return Strings.LayoutHistoryKindStartup;
                case PanelLayoutHistory.EntryKind.BeforeRestore: return Strings.LayoutHistoryKindBeforeRestore;
                default: return Strings.LayoutHistoryKindManual;
            }
        }

        static string FormatTime(PanelLayoutHistory.Entry entry) {
            return entry.Time.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        static string FormatEntry(PanelLayoutHistory.Entry entry) {
            var label = string.IsNullOrEmpty(entry.Name) ? KindLabel(entry.Kind) : entry.Name;
            //'&' would be shown as a mnemonic underline
            return string.Format(Strings.LayoutHistoryEntryFormat,
                FormatTime(entry), label.Replace("&", "&&"), entry.PanelCount);
        }

        /// <summary>
        /// Owner for dialogs: the primary when it is on screen, so that the dialog
        /// is placed above the (topmost) panels.
        /// </summary>
        static IWin32Window DialogOwner(MainForm primary) {
            if (primary == null || primary.IsDisposed || !primary.Visible || primary.IsMinimizedWindow)
                return null;
            return primary;
        }

        static void SaveCurrent(MainForm primary) {
            //Save the real layout, not a preview
            CancelScheduledPreview();
            PanelLayoutManager.EndPreview();
            if (primary == null || primary.IsDisposed)
                return;

            string name;
            using (var dialog = new TextInputDialog(Strings.LayoutHistorySaveTitle, Strings.LayoutHistorySavePrompt)) {
                if (dialog.ShowDialog(DialogOwner(primary)) != DialogResult.OK)
                    return;
                name = dialog.Value;
            }

            var entry = PanelLayoutHistory.Add(
                PanelLayoutManager.BuildLayoutLines(primary, primary.ChildPanels),
                PanelLayoutHistory.EntryKind.Manual, name);
            if (entry == null) {
                MessageBox.Show(DialogOwner(primary), Strings.ErrorLayoutHistorySave, Strings.LayoutHistorySaveTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Restore(MainForm primary, PanelLayoutHistory.Entry entry) {
            if (primary == null || primary.IsDisposed)
                return;

            //Keep showing the layout while asking for confirmation
            CancelScheduledPreview();
            StartPreview(primary, entry, null);
            DialogResult result;
            _confirming = true;
            try {
                result = MessageBox.Show(DialogOwner(primary),
                    string.Format(Strings.ConfirmRestoreLayout, FormatEntry(entry).Replace("&&", "&")),
                    Strings.MenuLayoutHistory,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }
            finally {
                _confirming = false;
                //The restoration starts from the real layout (stored as "before restore")
                CancelScheduledPreview();
                PanelLayoutManager.EndPreview();
            }
            if (result != DialogResult.Yes)
                return;

            //Run after the menu is gone: restoring closes panels, possibly the one
            //the context menu was opened from
            primary.BeginInvoke(new Action(() => {
                if (primary.IsDisposed)
                    return;
                if (!PanelLayoutManager.RestoreFromHistory(primary, entry)) {
                    MessageBox.Show(DialogOwner(primary), Strings.ErrorLayoutHistoryRestore, Strings.MenuLayoutHistory,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        static void OpenFolder() {
            try {
                Process.Start(PanelLayoutHistory.EnsureFolder());
            }
            catch (Exception ex) {
                Log.WriteException("Unable to open panel layout history folder", ex);
            }
        }
    }
}
