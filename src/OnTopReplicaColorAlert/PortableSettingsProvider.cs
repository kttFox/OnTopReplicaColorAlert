using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace OnTopReplicaColorAlert {

    /// <summary>
    /// アプリケーション設定 (Settings.Default) を user.config の代わりに
    /// 実行ファイルと同じフォルダーの XML ファイルへ保存するプロバイダー。
    /// ログや PanelLayout と同様に exe 横へ保存され、ポータブル運用が可能になる。
    /// </summary>
    public sealed class PortableSettingsProvider : SettingsProvider {

        const string FileName = "OnTopReplicaColorAlert.Settings.xml";

        //Settings file used before the application was renamed: read once when the
        //current file does not exist yet, so existing installations keep their settings.
        //(The next save writes the current file name.)
        const string LegacyFileName = "OnTopReplica.Settings.xml";

        const string RootName = "Settings";

        static string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);

        static string LegacyFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LegacyFileName);

        XDocument _doc;

        public override string Name => "PortableSettingsProvider";

        public override string ApplicationName {
            get { return "OnTopReplicaColorAlert"; }
            set { }
        }

        public override void Initialize(string name, NameValueCollection config) {
            base.Initialize(name ?? Name, config);
        }

        XElement LoadRoot() {
            if (_doc == null) {
                _doc = TryLoad(FilePath);

                //Fall back to the pre-rename file (never overwritten: saves go to FilePath)
                if (_doc == null && File.Exists(LegacyFilePath)) {
                    _doc = TryLoad(LegacyFilePath);
                    if (_doc != null) {
                        Log.Write("Imported settings from legacy file {0}", LegacyFileName);
                        MigrateLegacyFile();
                    }
                }

                if (_doc == null)
                    _doc = new XDocument(new XElement(RootName));
            }
            return _doc.Root;
        }

        static XDocument TryLoad(string path) {
            try {
                if (!File.Exists(path))
                    return null;

                var doc = XDocument.Load(path);
                if (doc.Root != null && doc.Root.Name == RootName)
                    return doc;
            }
            catch (Exception ex) {
                Log.WriteException("Unable to load settings file " + path, ex);
            }
            return null;
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection) {
            var root = LoadRoot();
            var values = new SettingsPropertyValueCollection();

            foreach (SettingsProperty prop in collection) {
                var value = new SettingsPropertyValue(prop) {
                    IsDirty = false
                };
                var elem = root.Element(prop.Name);
                if (elem != null) {
                    //Stored as text content; deserialized lazily according to SerializeAs
                    value.SerializedValue = elem.Value;
                }
                values.Add(value);
            }
            return values;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection) {
            var root = LoadRoot();

            foreach (SettingsPropertyValue value in collection) {
                var serialized = value.SerializedValue as string
                    ?? value.SerializedValue?.ToString();

                var elem = root.Element(value.Name);
                if (serialized == null) {
                    elem?.Remove();
                    continue;
                }
                if (elem == null) {
                    elem = new XElement(value.Name);
                    root.Add(elem);
                }
                elem.Value = serialized;
                value.IsDirty = false;
            }

            try {
                _doc.Save(FilePath);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to save settings file", ex);
            }
        }

        /// <summary>
        /// Writes the imported settings under the current file name and removes the
        /// pre-rename file. The old file is only deleted once the new one is written,
        /// so a failure cannot lose the settings.
        /// </summary>
        void MigrateLegacyFile() {
            try {
                _doc.Save(FilePath);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to migrate legacy settings file", ex);
                return;
            }

            try {
                File.Delete(LegacyFilePath);
                Log.Write("Migrated settings to {0} and deleted {1}", FileName, LegacyFileName);
            }
            catch (Exception ex) {
                Log.WriteException("Unable to delete legacy settings file", ex);
            }
        }
    }
}
