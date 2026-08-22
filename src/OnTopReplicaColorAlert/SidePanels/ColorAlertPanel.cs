using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OnTopReplicaColorAlert.Properties;
using OnTopReplicaColorAlert.MessagePumpProcessors;

namespace OnTopReplicaColorAlert.SidePanels {
    partial class ColorAlertPanel : SidePanel {

        public ColorAlertPanel() {
            InitializeComponent();
            LocalizePanel();
        }

        private void LocalizePanel() {
            groupColor.Text = Strings.ColorAlert_Title;
            labelInterval.Text = Strings.ColorAlert_Interval;
            labelIntervalUnit.Text = Strings.ColorAlert_IntervalRange;
            checkEnabled.Text = Strings.ColorAlert_Enable;
            checkAlertOnLoss.Text = Strings.ColorAlert_AlertOnLoss;
            tooltipInfo.SetToolTip(checkAlertOnLoss, Strings.ColorAlert_AlertOnLossTooltip);
            checkIgnoreDark.Text = Strings.ColorAlert_IgnoreDark;
            tooltipInfo.SetToolTip(checkIgnoreDark, Strings.ColorAlert_IgnoreDarkTooltip);
            labelLossMiss.Text = Strings.ColorAlert_LossMissCount;
            tooltipInfo.SetToolTip(labelLossMiss, Strings.ColorAlert_LossMissCountTooltip);
            labelMinPixels.Text = Strings.ColorAlert_MinPixels;
            tooltipInfo.SetToolTip(labelDetectedPixels, Strings.ColorAlert_DetectedPixelsTooltip);
            tooltipInfo.SetToolTip(labelMinPixels, Strings.ColorAlert_MinPixelsTooltip);
            labelColorSelection.Text = Strings.ColorAlert_DetectColors;
            checkRed.Text = Strings.ColorAlert_Red;
            checkOrange.Text = Strings.ColorAlert_Orange;
            checkGray.Text = Strings.ColorAlert_Gray;
            checkCustomColor.Text = Strings.ColorAlert_CustomColor;
            btnPickCustomColor.Text = Strings.ColorAlert_ChooseColor;
            btnSampleCursorColor.Text = Strings.ColorAlert_SampleCursorColor;
            btnTestAlarm.Text = Strings.ColorAlert_TestAlarm;
            checkKeyPress.Text = Strings.ColorAlert_KeyPress;
            labelVolume.Text = Strings.ColorAlert_Volume;
            labelSoundFile.Text = Strings.ColorAlert_SoundFile;
            btnClose.Text = Strings.MenuClose;

            tooltipInfo.SetToolTip(labelInterval, Strings.ColorAlert_IntervalTooltip);
        }

        ColorDetectionProcessor _processor;
        private bool _loading = false; // suppress CheckColor_CheckedChanged during panel init

        private const string SoundsRelativePath = "Sounds";
        private Color? _customTargetColor;

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);
            Log.Write("ColorAlertPanel shown size {0}", this.Size);

            try {
                _processor = form.MessagePumpManager.Get<ColorDetectionProcessor>();
            }
            catch {
                _processor = null;
            }

            _loading = true;
            try {
                if (_processor != null) {
                    checkEnabled.Checked = _processor.Enabled;
                    numInterval.Value = _processor.SampleInterval;
                    checkAlertOnLoss.Checked = _processor.AlertOnLoss;
                    checkIgnoreDark.Checked = _processor.IgnoreDarkFrames;
                    numLossMiss.Value = Math.Max(numLossMiss.Minimum, Math.Min(numLossMiss.Maximum, _processor.LossMissThreshold));
                    numMinPixels.Value = Math.Max(numMinPixels.Minimum, Math.Min(numMinPixels.Maximum, _processor.MinDetectionPixels));
                }
                UpdateIgnoreDarkUi();

                // Load current state from the processor (factory default: empty selection)
                HashSet<ColorCategory> categories;
                Color? loadedCustomColor = null;

                if (_processor != null) {
                    categories = new HashSet<ColorCategory>(_processor.EnabledCategories);
                    loadedCustomColor = _processor.CustomTargetColor;
                }
                else {
                    categories = new HashSet<ColorCategory>();
                }

                _customTargetColor = loadedCustomColor;

                // Set checkboxes (events suppressed by _loading flag)
                checkRed.Checked = categories.Contains(ColorCategory.Red);
                checkOrange.Checked = categories.Contains(ColorCategory.Orange);
                checkGray.Checked = categories.Contains(ColorCategory.Gray);
                checkCustomColor.Checked = _customTargetColor.HasValue;

                // キー送信設定
                if (_processor != null) {
                    _keyPressKey = _processor.KeyPressKey;
                    checkKeyPress.Checked = _processor.KeyPressEnabled;
                }
                UpdateKeyCaptureUi();

                Log.Write("Loaded ColorAlert categories: {0}", CategoriesToString(categories));

                // Sync to processor
                if (_processor != null) {
                    _processor.EnabledCategories = new HashSet<ColorCategory>(categories);
                    _processor.CustomTargetColor = _customTargetColor;
                }

                UpdateCustomColorUi();
            }
            finally {
                _loading = false;
            }

            // Load volume and sound settings from this panel's processor (persisted
            // per panel in the layout file and restored at startup)
            trackBarVolume.Value = _processor != null ? (int)(_processor.AlarmVolume * 100) : 100;
            PopulateSoundList();
            var savedSound = _processor != null ? _processor.AlarmSoundFile : null;
            if (!string.IsNullOrEmpty(savedSound)) {
                string item;
                if (savedSound.StartsWith(ColorDetectionProcessor.SystemSoundPrefix, StringComparison.OrdinalIgnoreCase)) {
                    item = SystemSoundDisplayPrefix + savedSound.Substring(ColorDetectionProcessor.SystemSoundPrefix.Length);
                }
                else {
                    item = Path.GetFileName(savedSound);
                }
                if (comboSound.Items.Contains(item))
                    comboSound.SelectedItem = item;
            }

            StartDetectedPixelsTimer();
        }

        private Timer _detectedPixelsTimer;

        /// <summary>
        /// パネル表示中、直近の検出ピクセル数を定期的にラベルへ反映するタイマーを開始する。
        /// </summary>
        private void StartDetectedPixelsTimer() {
            if (_detectedPixelsTimer != null) return;
            // 検出が無効・停止中でも検出数を更新できるよう、カウント監視モードを有効化する
            if (_processor != null) _processor.CountMonitoringEnabled = true;
            _detectedPixelsTimer = new Timer { Interval = 500 };
            _detectedPixelsTimer.Tick += (s, e) => UpdateDetectedPixelsLabel();
            _detectedPixelsTimer.Start();
            UpdateDetectedPixelsLabel();
        }

        private void StopDetectedPixelsTimer() {
            if (_processor != null) _processor.CountMonitoringEnabled = false;
            if (_detectedPixelsTimer == null) return;
            _detectedPixelsTimer.Stop();
            _detectedPixelsTimer.Dispose();
            _detectedPixelsTimer = null;
        }

        /// <summary>
        /// 直近サンプリングのカテゴリ別ピクセル数をラベルに表示する。
        /// 検出が無効/未実行の場合は "--" を表示する。
        /// ラベルに収まらない場合(AutoEllipsis で省略される場合)に備えて、
        /// 全文をツールチップにも設定する。
        /// </summary>
        private void UpdateDetectedPixelsLabel() {
            string text;
            // 直近サンプリングが古い(対象ウィンドウなし等でサンプリングが止まっている)場合は "--" を表示する
            bool stale = _processor == null ||
                _processor.LastSampleTimeUtc == DateTime.MinValue ||
                (DateTime.UtcNow - _processor.LastSampleTimeUtc).TotalMilliseconds > Math.Max(5000, _processor.SampleInterval * 3);
            if (stale) {
                text = Strings.ColorAlert_DetectedPixels + " --";
            }
            else {
                var parts = new List<string>();
                if (checkRed.Checked) parts.Add(Strings.ColorAlert_Red + ":" + _processor.LastRedCount);
                if (checkOrange.Checked) parts.Add(Strings.ColorAlert_Orange + ":" + _processor.LastOrangeCount);
                if (checkGray.Checked) parts.Add(Strings.ColorAlert_Gray + ":" + _processor.LastGrayCount);
                if (checkCustomColor.Checked) parts.Add(Strings.ColorAlert_CustomColor + ":" + _processor.LastCustomCount);

                text = Strings.ColorAlert_DetectedPixels + " " +
                    (parts.Count > 0 ? string.Join(" ", parts) : "--");
            }

            if (labelDetectedPixels.Text != text) {
                labelDetectedPixels.Text = text;
                tooltipInfo.SetToolTip(labelDetectedPixels, text);
            }
        }

        const string SystemSoundDisplayPrefix = "System: ";
        static readonly string[] SystemSoundNames = { "Beep", "Hand", };

        /// <summary>
        /// Gets the storable sound value for the current combo selection: either a
        /// file path in the Sounds folder or a "system:" pseudo-path selecting one
        /// of the System.Media.SystemSounds entries. Null if nothing is selected.
        /// </summary>
        private string GetSelectedSoundPath() {
            if (comboSound.SelectedItem == null)
                return null;

            string item = comboSound.SelectedItem.ToString();
            if (item.StartsWith(SystemSoundDisplayPrefix, StringComparison.Ordinal)) {
                return ColorDetectionProcessor.SystemSoundPrefix + item.Substring(SystemSoundDisplayPrefix.Length);
            }
            return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SoundsRelativePath, item);
        }

        public override string Title {
            get {
                return Strings.ColorAlert_Title;
            }
        }

        public override void OnClosing(MainForm form) {
            base.OnClosing(form);

            StopDetectedPixelsTimer();
            EndColorSampling();

            var categories = GetEnabledCategories();

            if (_processor != null) {
                _processor.EnabledCategories = new HashSet<ColorCategory>(categories);
                _processor.SampleInterval = (int)numInterval.Value;
                _processor.Enabled = checkEnabled.Checked;
                _processor.AlarmVolume = trackBarVolume.Value / 100f;
                if (GetSelectedSoundPath() != null) {
                    _processor.AlarmSoundFile = GetSelectedSoundPath();
                }
                _processor.KeyPressEnabled = checkKeyPress.Checked;
                _processor.KeyPressKey = _keyPressKey;
                _processor.AlertOnLoss = checkAlertOnLoss.Checked;
                _processor.IgnoreDarkFrames = checkIgnoreDark.Checked;
                _processor.LossMissThreshold = (int)numLossMiss.Value;
                _processor.MinDetectionPixels = (int)numMinPixels.Value;
            }

            //Color detection settings (incl. volume and sound) are persisted
            //per panel in the panel layout file
            form.NotifyPanelLayoutChanged();
        }

        /// <summary>
        /// Gets the set of enabled color categories from the UI checkboxes.
        /// </summary>
        private HashSet<ColorCategory> GetEnabledCategories() {
            var cats = new HashSet<ColorCategory>();
            if (checkRed.Checked) cats.Add(ColorCategory.Red);
            if (checkOrange.Checked) cats.Add(ColorCategory.Orange);
            if (checkGray.Checked) cats.Add(ColorCategory.Gray);
            return cats;
        }

        private Color? GetEffectiveCustomTargetColor() {
            if (!checkCustomColor.Checked) {
                return null;
            }

            return _customTargetColor;
        }

        private void UpdateCustomColorUi() {
            bool hasCustomColor = _customTargetColor.HasValue;
            if (hasCustomColor) {
                panelCustomColor.BackColor = _customTargetColor.Value;
            }
            else {
                panelCustomColor.BackColor = Color.White;
            }

            if (!hasCustomColor) {
                checkCustomColor.Checked = false;
            }

            btnPickCustomColor.Enabled = true;
            btnSampleCursorColor.Enabled = true;
        }

        /// <summary>
        /// Serializes categories to a comma-separated string.
        /// </summary>
        private string CategoriesToString(HashSet<ColorCategory> categories) {
            if (categories == null || categories.Count == 0) return string.Empty;
            var parts = new List<string>();
            foreach (var c in categories) parts.Add(c.ToString());
            return string.Join(",", parts);
        }

        private void CheckEnabled_CheckedChanged(object sender, EventArgs e) {
            if (_loading) return; // suppress during panel initialization
            if (_processor != null) {
                _processor.Enabled = checkEnabled.Checked;
                if (checkEnabled.Checked) {
                    SyncSettingsToProcessor();
                }
            }
        }

        private void CheckAlertOnLoss_CheckedChanged(object sender, EventArgs e) {
            UpdateIgnoreDarkUi();
            if (_loading) return; // suppress during panel initialization
            if (_processor != null) {
                _processor.AlertOnLoss = checkAlertOnLoss.Checked;
                Log.Write("AlertOnLoss changed: {0}", checkAlertOnLoss.Checked);
            }
            // Persist immediately so a restart restores the correct state
            if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
        }

        /// <summary>
        /// 「暗転を無視」チェックと連続消失回数は消失検知モード時のみ有効化する。
        /// </summary>
        private void UpdateIgnoreDarkUi() {
            checkIgnoreDark.Enabled = checkAlertOnLoss.Checked;
            labelLossMiss.Enabled = checkAlertOnLoss.Checked;
            numLossMiss.Enabled = checkAlertOnLoss.Checked;
        }

        private void NumLossMiss_ValueChanged(object sender, EventArgs e) {
            if (_loading) return; // suppress during panel initialization
            if (_processor != null) {
                _processor.LossMissThreshold = (int)numLossMiss.Value;
                Log.Write("LossMissThreshold changed: {0}", (int)numLossMiss.Value);
            }
            // Persist immediately so a restart restores the correct state
            if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
        }

        private void NumMinPixels_ValueChanged(object sender, EventArgs e) {
            if (_loading) return; // suppress during panel initialization
            if (_processor != null) {
                _processor.MinDetectionPixels = (int)numMinPixels.Value;
                Log.Write("MinDetectionPixels changed: {0}", (int)numMinPixels.Value);
            }
            // Persist immediately so a restart restores the correct state
            if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
        }

        private void CheckIgnoreDark_CheckedChanged(object sender, EventArgs e) {
            if (_loading) return; // suppress during panel initialization
            if (_processor != null) {
                _processor.IgnoreDarkFrames = checkIgnoreDark.Checked;
                Log.Write("IgnoreDarkFrames changed: {0}", checkIgnoreDark.Checked);
            }
            // Persist immediately so a restart restores the correct state
            if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
        }

        private void CheckColor_CheckedChanged(object sender, EventArgs e) {
            if (_loading) return; // suppress during panel initialization
            var categories = GetEnabledCategories();
            if (_processor != null) {
                _processor.EnabledCategories = new HashSet<ColorCategory>(categories);
                Log.Write("Color categories changed: {0}", CategoriesToString(categories));
            }
            // Persist immediately so a restart restores the correct state
            if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
        }

        /// <summary>
        /// Syncs all current panel settings to the color detection processor.
        /// </summary>
        private void SyncSettingsToProcessor() {
            if (_processor == null) return;
            _processor.EnabledCategories = GetEnabledCategories();
            _processor.CustomTargetColor = GetEffectiveCustomTargetColor();
            _processor.SampleInterval = (int)numInterval.Value;
            _processor.AlarmVolume = trackBarVolume.Value / 100f;
            var soundPath = GetSelectedSoundPath();
            if (soundPath != null) {
                _processor.AlarmSoundFile = soundPath;
            }
            _processor.KeyPressEnabled = checkKeyPress.Checked;
            _processor.KeyPressKey = _keyPressKey;
            _processor.AlertOnLoss = checkAlertOnLoss.Checked;
            _processor.IgnoreDarkFrames = checkIgnoreDark.Checked;
            _processor.LossMissThreshold = (int)numLossMiss.Value;
            _processor.MinDetectionPixels = (int)numMinPixels.Value;
            Log.Write("SyncSettingsToProcessor: categories={0}, custom={1}", CategoriesToString(_processor.EnabledCategories), _processor.CustomTargetColor.HasValue ? _processor.CustomTargetColor.Value.ToString() : "none");
        }

        private void TrackBarVolume_Scroll(object sender, EventArgs e) {
            if (_processor != null) {
                _processor.AlarmVolume = trackBarVolume.Value / 100f;
                Log.Write("Alarm volume set to {0}", _processor.AlarmVolume);
                if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
            }
        }

        private void BtnClose_Click(object sender, EventArgs e) {
            OnRequestClosing();
        }

        private void NumInterval_ValueChanged(object sender, EventArgs e) {
            if (_processor != null) {
                _processor.SampleInterval = (int)numInterval.Value;
            }
        }

        private void CheckCustomColor_CheckedChanged(object sender, EventArgs e) {
            if (_loading) {
                return;
            }

            if (checkCustomColor.Checked && !_customTargetColor.HasValue) {
                _customTargetColor = Color.Red;
            }

            UpdateCustomColorUi();
            SyncSettingsToProcessor();
            if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
        }

        private void BtnPickCustomColor_Click(object sender, EventArgs e) {
            using (var colorDialog = new ColorDialog()) {
                colorDialog.FullOpen = true;
                colorDialog.Color = _customTargetColor ?? Color.Red;

                if (colorDialog.ShowDialog(this) != DialogResult.OK) {
                    return;
                }

                ApplyCustomColor(colorDialog.Color);
            }
        }

        private ColorSamplingOverlay _samplingOverlay;

        private void BtnSampleCursorColor_Click(object sender, EventArgs e) {
            if (_samplingOverlay != null) {
                return; // sampling already in progress
            }

            StartColorSampling();
        }

        /// <summary>
        /// Enters color-sampling mode: shows a full-screen overlay so the next click
        /// anywhere on screen (not the button itself) determines the sampled color.
        /// </summary>
        private void StartColorSampling() {
            btnSampleCursorColor.Enabled = false;
            btnPickCustomColor.Enabled = false;
            btnSampleCursorColor.Text = Strings.ColorAlert_SamplingInProgress;

            _samplingOverlay = new ColorSamplingOverlay();
            _samplingOverlay.Picked += SamplingOverlay_Picked;
            _samplingOverlay.Cancelled += SamplingOverlay_Cancelled;
            _samplingOverlay.Show();
        }

        private void SamplingOverlay_Picked(object sender, ColorPickedEventArgs e) {
            EndColorSampling();
            ApplyCustomColor(e.Color);
            Log.Write("Sampled custom color from screen: {0}", e.Color);
        }

        private void SamplingOverlay_Cancelled(object sender, EventArgs e) {
            EndColorSampling();
            Log.Write("Color sampling cancelled");
        }

        /// <summary>
        /// Closes the sampling overlay (if any) and restores the panel's normal state.
        /// </summary>
        private void EndColorSampling() {
            if (_samplingOverlay != null) {
                _samplingOverlay.Picked -= SamplingOverlay_Picked;
                _samplingOverlay.Cancelled -= SamplingOverlay_Cancelled;
                _samplingOverlay.Close();
                _samplingOverlay.Dispose();
                _samplingOverlay = null;

                //Closing the (topmost, full-screen) overlay can drop the non-topmost
                //settings window behind other windows: bring it back to the front.
                var host = FindForm();
                if (host != null && host.Visible && !host.IsDisposed) {
                    host.Activate();
                }
            }

            btnSampleCursorColor.Enabled = true;
            btnPickCustomColor.Enabled = true;
            btnSampleCursorColor.Text = Strings.ColorAlert_SampleCursorColor;
        }

        private void ApplyCustomColor(Color color) {
            _customTargetColor = color;
            checkCustomColor.Checked = true;
            UpdateCustomColorUi();
            SyncSettingsToProcessor();
            if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
        }

        private void ComboSound_SelectedIndexChanged(object sender, EventArgs e) {
            if (_processor != null) {
                var path = GetSelectedSoundPath();
                if (path != null) {
                    _processor.AlarmSoundFile = path;
                    if (!_loading && ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
                }
            }
        }

        private void PopulateSoundList() {
            comboSound.Items.Clear();

            foreach (var systemSoundName in SystemSoundNames) {
                comboSound.Items.Add(SystemSoundDisplayPrefix + systemSoundName);
            }

            string dir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SoundsRelativePath);
            if (Directory.Exists(dir)) {
                foreach (var ext in new[] { "*.wav", "*.mp3" }) {
                    foreach (var f in Directory.GetFiles(dir, ext)) {
                        comboSound.Items.Add(Path.GetFileName(f));
                    }
                }
            }

            Log.Write("PopulateSoundList found {0} items", comboSound.Items.Count);
        }

        private Keys _keyPressKey = Keys.None;

        /// <summary>
        /// キー表示テキストボックスを現在の設定に合わせて更新する。
        /// </summary>
        private void UpdateKeyCaptureUi() {
            textKeyCapture.Text = _keyPressKey == Keys.None
                ? Strings.ColorAlert_KeyNone
                : _keyPressKey.ToString();
        }

        private void CheckKeyPress_CheckedChanged(object sender, EventArgs e) {
            if (_loading) return;
            if (_processor != null) {
                _processor.KeyPressEnabled = checkKeyPress.Checked;
                _processor.KeyPressKey = _keyPressKey;
            }
            if (checkKeyPress.Checked && _keyPressKey == Keys.None) {
                textKeyCapture.Focus();
            }
            if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
        }

        private void TextKeyCapture_KeyDown(object sender, KeyEventArgs e) {
            e.Handled = true;
            e.SuppressKeyPress = true;

            // 修飾キー単独は無視する(キーが確定するまで待つ)
            var key = e.KeyCode;
            if (key == Keys.ShiftKey || key == Keys.ControlKey || key == Keys.Menu ||
                key == Keys.LWin || key == Keys.RWin || key == Keys.None) {
                return;
            }

            _keyPressKey = key;
            UpdateKeyCaptureUi();
            if (_processor != null) {
                _processor.KeyPressKey = _keyPressKey;
                _processor.KeyPressEnabled = checkKeyPress.Checked;
            }
            Log.Write("ColorAlert key press set to {0}", _keyPressKey);
            if (ParentMainForm != null) ParentMainForm.NotifyPanelLayoutChanged();
        }

        private void BtnTestAlarm_Click(object sender, EventArgs e) {
            var path = GetSelectedSoundPath();
            if (path == null) {
                Log.Write("Test alarm: no sound file selected");
                return;
            }

            if (ColorDetectionProcessor.TryPlaySystemSound(path)) {
                Log.Write("Test alarm: system sound {0}", path);
                return;
            }

            float volume = trackBarVolume.Value / 100f;
            try {
                var player = new System.Windows.Media.MediaPlayer();
                player.Open(new Uri(path));
                player.Volume = Math.Max(0, Math.Min(1, volume));
                player.Play();
                Log.Write("Test alarm: playing {0} at volume {1}", path, volume);
            }
            catch (Exception ex) {
                Log.Write("Test alarm error: {0}", ex.Message);
            }
        }
    }
}
