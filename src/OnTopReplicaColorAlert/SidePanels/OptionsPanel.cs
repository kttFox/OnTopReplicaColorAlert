using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OnTopReplicaColorAlert.Properties;
using System.Globalization;

namespace OnTopReplicaColorAlert.SidePanels {
    partial class OptionsPanel : SidePanel {

        public OptionsPanel() {
            InitializeComponent();

            LocalizePanel();
        }

        private void LocalizePanel() {
            groupLanguage.Text = Strings.SettingsLanguageTitle;
            lblLanguage.Text = Strings.SettingsRestartRequired;

            groupIndicator.Text = Strings.SettingsIndicatorTitle;
            checkIndicator.Text = Strings.SettingsIndicatorShow;
            lblIndicatorSize.Text = Strings.SettingsIndicatorSize;
            lblIndicatorRunColor.Text = Strings.SettingsIndicatorRunColor;
            lblIndicatorPauseColor.Text = Strings.SettingsIndicatorPauseColor;
            checkPauseColorAlertOnLoss.Text = Strings.SettingsPauseColorAlertWhenSourceLost;

            groupAutoHide.Text = Strings.SettingsAutoHideTitle;
            checkAutoHide.Text = Strings.SettingsAutoHideWhenSourceInactive;

            btnClose.Text = Strings.MenuClose;
        }

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);

            PopulateLanguageComboBox();

            //Load color alert indicator settings (events suppressed while loading)
            _loadingIndicatorSettings = true;
            try {
                checkIndicator.Checked = Settings.Default.ColorAlertIndicatorEnabled;
                numIndicatorSize.Value = Math.Max(numIndicatorSize.Minimum,
                    Math.Min(numIndicatorSize.Maximum, Settings.Default.ColorAlertIndicatorSize));
                panelIndicatorRunColor.BackColor = Settings.Default.ColorAlertIndicatorRunningColor;
                panelIndicatorPauseColor.BackColor = Settings.Default.ColorAlertIndicatorPausedColor;
                checkPauseColorAlertOnLoss.Checked = Settings.Default.PauseColorAlertWhenSourceLost;
            }
            finally {
                _loadingIndicatorSettings = false;
            }

            //Load auto hide setting (event suppressed while loading)
            _loadingAutoHideSetting = true;
            try {
                checkAutoHide.Checked = Settings.Default.HideWhenSourceDeactivated;
            }
            finally {
                _loadingAutoHideSetting = false;
            }
        }

        #region Auto hide

        bool _loadingAutoHideSetting = false;

        private void AutoHide_CheckedChanged(object sender, EventArgs e) {
            if (_loadingAutoHideSetting) return;

            Settings.Default.HideWhenSourceDeactivated = checkAutoHide.Checked;
            Settings.Default.Save();
        }

        #endregion

        private void Close_click(object sender, EventArgs e) {
            OnRequestClosing();
        }

        public override string Title {
            get {
                return Strings.SettingsTitle;
            }
        }

        public override void OnClosing(MainForm form) {
            base.OnClosing(form);
        }

        #region Color alert indicator

        bool _loadingIndicatorSettings = false;

        /// <summary>
        /// インジケーター設定を保存し、開いている全パネルへ即時反映する。
        /// </summary>
        private void ApplyIndicatorSettings() {
            if (_loadingIndicatorSettings) return;

            Settings.Default.ColorAlertIndicatorEnabled = checkIndicator.Checked;
            Settings.Default.ColorAlertIndicatorSize = (int)numIndicatorSize.Value;
            Settings.Default.ColorAlertIndicatorRunningColor = panelIndicatorRunColor.BackColor;
            Settings.Default.ColorAlertIndicatorPausedColor = panelIndicatorPauseColor.BackColor;
            Settings.Default.Save();

            MainForm.UpdateAllColorAlertIndicators();
        }

        private void Indicator_SettingChanged(object sender, EventArgs e) {
            ApplyIndicatorSettings();
        }

        private void PauseColorAlertOnLoss_CheckedChanged(object sender, EventArgs e) {
            if (_loadingIndicatorSettings) return;

            Settings.Default.PauseColorAlertWhenSourceLost = checkPauseColorAlertOnLoss.Checked;
            Settings.Default.Save();
        }

        private void PanelIndicatorRunColor_Click(object sender, EventArgs e) {
            PickIndicatorColor(panelIndicatorRunColor);
        }

        private void PanelIndicatorPauseColor_Click(object sender, EventArgs e) {
            PickIndicatorColor(panelIndicatorPauseColor);
        }

        private void PickIndicatorColor(Panel swatch) {
            using (var dialog = new ColorDialog()) {
                dialog.FullOpen = true;
                dialog.Color = swatch.BackColor;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                swatch.BackColor = dialog.Color;
            }
            ApplyIndicatorSettings();
        }

        #endregion

        #region Language

        class CultureWrapper {
            public CultureWrapper(string name, CultureInfo culture, Image img) {
                Culture = culture;
                Image = img;
                Name = name;
            }
            public CultureInfo Culture { get; set; }
            public Image Image { get; set; }
            public string Name { get; set; }
        }

        CultureWrapper[] _languageList = {
            new CultureWrapper("English", new CultureInfo("en-US"), Resources.flag_usa),
            new CultureWrapper("Čeština", new CultureInfo("cs-CZ"), Resources.flag_czech),
            new CultureWrapper("Dansk", new CultureInfo("da-DK"), Resources.flag_danish),
            new CultureWrapper("Deutsch", new CultureInfo("de-DE"), Resources.flag_germany),
            new CultureWrapper("Español", new CultureInfo("es-ES"), Resources.flag_spanish),
            new CultureWrapper("Italiano", new CultureInfo("it-IT"), Resources.flag_ita),
            new CultureWrapper("Polski", new CultureInfo("pl-PL"), Resources.flag_poland),
            new CultureWrapper("简体中文", new CultureInfo("zh-CN"), Resources.flag_china),
            new CultureWrapper("繁體中文", new CultureInfo("zh-TW"), Resources.flag_taiwan),
            new CultureWrapper("Português", new CultureInfo("pt-BR"), Resources.flag_taiwan),
            new CultureWrapper("日本語", new CultureInfo("ja-JP"), Resources.flag_japan),
        };

        private void PopulateLanguageComboBox() {
            comboLanguage.Items.Clear();

            var imageList = new ImageList() {
                ImageSize = new Size(16, 16),
                ColorDepth = ColorDepth.Depth32Bit
            };
            comboLanguage.IconList = imageList;

            int selectedIndex = -1;
            foreach (var langPair in _languageList) {
                var item = new ImageComboBoxItem(langPair.Name, imageList.Images.Count) {
                    Tag = langPair.Culture
                };
                imageList.Images.Add(langPair.Image);
                comboLanguage.Items.Add(item);

                if (langPair.Culture.Equals(CultureInfo.CurrentUICulture)) {
                    selectedIndex = comboLanguage.Items.Count - 1;
                }
            }

            //Handle case when there is not explicitly set culture (default to first one, i.e. english)
            if (CultureInfo.CurrentUICulture.Equals(CultureInfo.InvariantCulture))
                selectedIndex = 0;

            comboLanguage.SelectedIndex = selectedIndex;
        }

        private void LanguageBox_IndexChange(object sender, EventArgs e) {
            var item = comboLanguage.SelectedItem as ImageComboBoxItem;
            if (item == null)
                return;

            Settings.Default.Language = item.Tag as CultureInfo;
            Settings.Default.Save();
        }

        #endregion

    }

}
