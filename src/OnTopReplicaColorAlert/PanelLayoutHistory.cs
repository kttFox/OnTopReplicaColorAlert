using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OnTopReplicaColorAlert {

    /// <summary>
    /// Keeps past panel layouts so that a broken or unwanted layout can be rolled
    /// back. Each entry is a copy of the layout file in the "PanelLayoutHistory"
    /// folder next to it, named "yyyyMMdd-HHmmss-fff.&lt;kind&gt;.txt". An optional
    /// display name is stored as a "#name=" metadata line after the header.
    /// </summary>
    static class PanelLayoutHistory {

        public enum EntryKind {
            /// <summary>Layout loaded at application startup (automatic).</summary>
            Startup,
            /// <summary>Layout replaced by restoring another entry (automatic).</summary>
            BeforeRestore,
            /// <summary>Saved explicitly by the user.</summary>
            Manual
        }

        public sealed class Entry {
            public string FilePath;
            public DateTime Time;
            public EntryKind Kind;
            public string Name;
            public int PanelCount;
        }

        const string FolderName = "PanelLayoutHistory";
        const string TimeFormat = "yyyyMMdd-HHmmss-fff";
        const string NamePrefix = "#name=";

        //Automatic entries are created at every startup: keep a bounded number,
        //without letting them push out the entries the user saved explicitly
        const int MaxAutomaticEntries = 20;
        const int MaxManualEntries = 50;

        static string FolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName);

        /// <summary>
        /// Gets the folder containing the history (created if missing).
        /// </summary>
        public static string EnsureFolder() {
            Directory.CreateDirectory(FolderPath);
            return FolderPath;
        }

        static string KindToken(EntryKind kind) {
            switch (kind) {
                case EntryKind.Startup: return "startup";
                case EntryKind.BeforeRestore: return "before-restore";
                default: return "manual";
            }
        }

        static bool TryParseKind(string token, out EntryKind kind) {
            foreach (EntryKind k in Enum.GetValues(typeof(EntryKind))) {
                if (KindToken(k) == token) {
                    kind = k;
                    return true;
                }
            }
            kind = EntryKind.Manual;
            return false;
        }

        /// <summary>
        /// Gets all history entries, newest first.
        /// </summary>
        public static List<Entry> GetEntries() {
            var entries = new List<Entry>();
            try {
                if (!Directory.Exists(FolderPath))
                    return entries;

                foreach (var path in Directory.GetFiles(FolderPath, "*.txt")) {
                    var entry = ParseEntry(path);
                    if (entry != null)
                        entries.Add(entry);
                }
            }
            catch (Exception ex) {
                Log.WriteException("Unable to list panel layout history", ex);
            }
            return entries.OrderByDescending(e => e.Time).ThenByDescending(e => e.FilePath, StringComparer.Ordinal).ToList();
        }

        static Entry ParseEntry(string path) {
            //"<time>.<kind>.txt"
            var parts = Path.GetFileNameWithoutExtension(path).Split('.');
            if (parts.Length != 2)
                return null;

            DateTime time;
            EntryKind kind;
            if (!DateTime.TryParseExact(parts[0], TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out time) ||
                !TryParseKind(parts[1], out kind))
                return null;

            var entry = new Entry { FilePath = path, Time = time, Kind = kind };
            try {
                var lines = File.ReadAllLines(path);
                if (!PanelLayoutManager.HasValidHeader(lines))
                    return null;
                foreach (var line in lines.Skip(1)) {
                    if (line.StartsWith(NamePrefix, StringComparison.Ordinal))
                        entry.Name = line.Substring(NamePrefix.Length);
                    else if (PanelLayoutManager.IsPanelLine(line))
                        entry.PanelCount++;
                }
            }
            catch (Exception ex) {
                Log.WriteException("Unable to read panel layout history entry", ex);
                return null;
            }
            return entry;
        }

        /// <summary>
        /// Reads the layout lines of an entry (null if it cannot be read).
        /// </summary>
        public static string[] ReadLines(Entry entry) {
            try {
                return File.ReadAllLines(entry.FilePath);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to read panel layout history entry", ex);
                return null;
            }
        }

        /// <summary>
        /// Layout lines without the header and metadata, to compare two layouts.
        /// </summary>
        static IEnumerable<string> LayoutContent(IEnumerable<string> lines) {
            return lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l) && l[0] != '#');
        }

        /// <summary>
        /// Stores a layout in the history. Automatic entries identical to the newest
        /// entry are skipped, so restarting without changes does not fill the
        /// history. Never throws: the history must not break saving or restoring.
        /// </summary>
        public static Entry Add(IList<string> layoutLines, EntryKind kind, string name) {
            try {
                if (!PanelLayoutManager.HasValidHeader(layoutLines) || !layoutLines.Any(PanelLayoutManager.IsPanelLine))
                    return null;

                if (kind != EntryKind.Manual) {
                    var newest = GetEntries().FirstOrDefault();
                    if (newest != null) {
                        var newestLines = ReadLines(newest);
                        if (newestLines != null && LayoutContent(newestLines).SequenceEqual(LayoutContent(layoutLines))) {
                            return null;
                        }
                    }
                }

                EnsureFolder();
                var time = DateTime.Now;
                string path;
                //Two entries within the same millisecond: bump the time stamp
                while (File.Exists(path = Path.Combine(FolderPath, time.ToString(TimeFormat, CultureInfo.InvariantCulture) + "." + KindToken(kind) + ".txt"))) {
                    time = time.AddMilliseconds(1);
                }

                var lines = new List<string> { layoutLines[0] };
                if (!string.IsNullOrWhiteSpace(name)) {
                    //A single line: the name is shown in a menu
                    lines.Add(NamePrefix + name.Replace('\r', ' ').Replace('\n', ' ').Trim());
                }
                lines.AddRange(layoutLines.Skip(1).Where(l => !l.StartsWith(NamePrefix, StringComparison.Ordinal)));
                File.WriteAllLines(path, lines);
                Log.Write("Panel layout history: added {0}", Path.GetFileName(path));

                Prune();
                return ParseEntry(path);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to add panel layout history entry", ex);
                return null;
            }
        }

        /// <summary>
        /// Deletes a history entry.
        /// </summary>
        public static void Delete(Entry entry) {
            try {
                File.Delete(entry.FilePath);
                Log.Write("Panel layout history: deleted {0}", Path.GetFileName(entry.FilePath));
            }
            catch (Exception ex) {
                Log.WriteException("Unable to delete panel layout history entry", ex);
            }
        }

        /// <summary>
        /// Removes the oldest entries beyond the retention limits.
        /// </summary>
        static void Prune() {
            var entries = GetEntries();
            var stale = entries.Where(e => e.Kind != EntryKind.Manual).Skip(MaxAutomaticEntries)
                .Concat(entries.Where(e => e.Kind == EntryKind.Manual).Skip(MaxManualEntries));
            foreach (var entry in stale.ToList()) {
                Delete(entry);
            }
        }
    }
}
