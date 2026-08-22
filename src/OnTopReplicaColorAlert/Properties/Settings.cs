using System.Configuration;

namespace OnTopReplicaColorAlert.Properties {

    /// <summary>
    /// 設定の保存先を実行ファイルと同じフォルダー (OnTopReplicaColorAlert.Settings.xml) に
    /// 切り替える。属性は partial クラス経由で自動生成コードに合成される。
    /// </summary>
    [SettingsProvider(typeof(OnTopReplicaColorAlert.PortableSettingsProvider))]
    internal sealed partial class Settings {

        public Settings() {
            //設定値が変更されたら即座にファイルへ保存する
            PropertyChanged += (s, e) => Save();
        }
    }
}
