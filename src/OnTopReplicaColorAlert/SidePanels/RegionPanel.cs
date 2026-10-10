using System;
using System.Drawing;
using System.Windows.Forms;
using OnTopReplicaColorAlert.Properties;

namespace OnTopReplicaColorAlert.SidePanels {

	partial class RegionPanel : SidePanel {

		public RegionPanel() {
			InitializeComponent();

            Localize();

			//Copy settings into combo box
			if (Settings.Default.SavedRegions != null) {
				foreach (object o in Settings.Default.SavedRegions) {
					comboRegions.Items.Add(o);
				}
			}

            _regionDrawnHandler = new ThumbnailPanel.RegionDrawnHandler(ThumbnailPanel_RegionDrawn);
		}

        /// <summary>
        /// Localizes the dialog's labels.
        /// </summary>
        private void Localize() {
            this.SuspendLayout();

            groupRegions.Text = Strings.RegionsTitle;
            comboRegions.CueBannerText = Strings.RegionsStoredRegions;
            labelCurrentRegion.Text = Strings.RegionsCurrentRegion;
            buttonReset.Text = Strings.RegionsResetButton;
            buttonDone.Text = Strings.RegionsDoneButton;
            labelAnchor.Text = Strings.RegionsAnchor;

            //Item order matches RegionAnchor, followed by the relative mode
            try {
                _ignoreValueChanges = true;
                comboAnchor.Items.Clear();
                comboAnchor.Items.AddRange(new object[] {
                    Strings.RegionsAnchorTopLeft,
                    Strings.RegionsAnchorTopCenter,
                    Strings.RegionsAnchorTopRight,
                    Strings.RegionsAnchorMiddleLeft,
                    Strings.RegionsAnchorMiddleCenter,
                    Strings.RegionsAnchorMiddleRight,
                    Strings.RegionsAnchorBottomLeft,
                    Strings.RegionsAnchorBottomCenter,
                    Strings.RegionsAnchorBottomRight,
                    Strings.RegionsAnchorRelative
                });
                comboAnchor.SelectedIndex = _modeIndex;
            }
            finally {
                _ignoreValueChanges = false;
            }
            UpdateRegionLabels();

            toolTip.SetToolTip(buttonSave, Strings.RegionsSaveButton);
            toolTip.SetToolTip(buttonDelete, Strings.RegionsDeleteButton);
            toolTip.SetToolTip(comboAnchor, Strings.RegionsAnchorTT);
            toolTip.SetToolTip(labelAnchor, Strings.RegionsAnchorTT);

            this.ResumeLayout();
        }

        //Index of the anchor combo item representing the relative mode
        //(the items before it are the RegionAnchor values)
        const int RelativeModeIndex = (int)RegionAnchor.BottomRight + 1;

        //Mode (anchor or relative) the values of the spinners are expressed in
        int _modeIndex = (int)RegionAnchor.TopLeft;

        static int GetModeIndex(ThumbnailRegion region) {
            return region.Relative ? RelativeModeIndex : (int)region.Anchor;
        }

        /// <summary>
        /// Switches a region to the mode of an anchor combo item, keeping it at the same place.
        /// </summary>
        static void SwitchToMode(ThumbnailRegion region, int modeIndex, Size sourceSize) {
            if (modeIndex == RelativeModeIndex)
                region.SwitchToRelative(sourceSize);
            else
                region.SwitchToAnchor((RegionAnchor)modeIndex, sourceSize);
        }

        /// <summary>
        /// Updates the labels for the region value selectors and the anchor selector.
        /// </summary>
        private void UpdateRegionControls(ThumbnailRegion region) {
            _modeIndex = GetModeIndex(region);
            comboAnchor.SelectedIndex = _modeIndex;
            
            if (region.Relative) {
                Padding p = region.BoundsAsPadding;
                numX.Value = p.Left;
                numY.Value = p.Top;
                numW.Value = p.Right;
                numH.Value = p.Bottom;
            }
            else {
                Rectangle r = region.Bounds;
                numX.Value = r.X;
                numY.Value = r.Y;
                numW.Value = r.Width;
                numH.Value = r.Height;
            }

            UpdateRegionLabels();
        }

        /// <summary>
        /// Updates the labels of region selectors based on the dialog's state.
        /// </summary>
        private void UpdateRegionLabels() {
            if (_modeIndex == RelativeModeIndex) {
                labelX.Text = Strings.RegionsLeft;
                labelY.Text = Strings.RegionsTop;
                labelWidth.Text = Strings.RegionsRight;
                labelHeight.Text = Strings.RegionsBottom;
            }
            else {
                //Anchored to the right/bottom: X/Y are the distances from that border;
                //anchored to the center: X/Y are the displacements from the center
                var anchor = (RegionAnchor)_modeIndex;
                labelX.Text = GetOffsetLabel(ThumbnailRegion.GetHorizontalEdge(anchor),
                    Strings.RegionsX, Strings.RegionsCenterX, Strings.RegionsRight);
                labelY.Text = GetOffsetLabel(ThumbnailRegion.GetVerticalEdge(anchor),
                    Strings.RegionsY, Strings.RegionsCenterY, Strings.RegionsBottom);
                labelWidth.Text = Strings.RegionsWidth;
                labelHeight.Text = Strings.RegionsHeight;
            }
        }

        static string GetOffsetLabel(AnchorEdge edge, string start, string center, string end) {
            switch (edge) {
                case AnchorEdge.Center:
                    return center;
                case AnchorEdge.End:
                    return end;
                default:
                    return start;
            }
        }

        public override string Title {
            get {
                return Strings.MenuRegion;
            }
        }

        ThumbnailPanel.RegionDrawnHandler _regionDrawnHandler;

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);

            //Init shown region if current thumbnail is clipped to region
            if (form.SelectedThumbnailRegion != null) {
                SetRegion(form.SelectedThumbnailRegion);
            }

            //Enable region drawing
            form.ThumbnailPanel.DrawMouseRegions = true;
            form.ThumbnailPanel.RegionDrawn += _regionDrawnHandler;
        }

        public override void OnClosing(MainForm form) {
            base.OnClosing(form);
            
            //Reset region drawing
            form.ThumbnailPanel.DrawMouseRegions = false;
            form.ThumbnailPanel.RegionDrawn -= _regionDrawnHandler;
        }

        void ThumbnailPanel_RegionDrawn(object sender, ThumbnailRegion region) {
            //Drawn regions are absolute from the top left corner: express them in
            //the selected mode, so they follow the selected edges
            if (ParentMainForm.ThumbnailPanel.IsShowingThumbnail) {
                SwitchToMode(region, _modeIndex, ParentMainForm.ThumbnailPanel.ThumbnailOriginalSize);
            }
            SetRegion(region);
        }

        #region Interface

        /// <summary>
        /// Sets the current selected region to a specific instance of a stored region.
        /// </summary>
        /// <param name="region">A stored region instance or null to reset.</param>
        public void SetRegion(StoredRegion region) {
            if (region == null) {
                Reset();
                return;
            }

            SetRegion(region.Region);

            //Select right combobox
            if (comboRegions.Items.Contains(region)) {
                comboRegions.SelectedItem = region;
            }
        }

        /// <summary>
        /// Sets the current selected region to a specific region rectangle.
        /// </summary>
        /// <param name="region">The region boundaries.</param>
		public void SetRegion(ThumbnailRegion region) {
			try {
				_ignoreValueChanges = true;

                UpdateRegionControls(region);

				numX.Enabled = numY.Enabled = numW.Enabled = numH.Enabled = true;
			}
			finally {
				_ignoreValueChanges = false;
			}

			OnRegionSet(region);
		}

        /// <summary>
        /// Resets the selected region and disables the num spinners.
        /// </summary>
		public void Reset() {
			try {
				_ignoreValueChanges = true;

				numX.Value = numY.Value = numW.Value = numH.Value = 0;
				numX.Enabled = numY.Enabled = numW.Enabled = numH.Enabled = false;
                //The selected mode is kept: it applies to the next drawn region
                UpdateRegionLabels();

				buttonSave.Enabled = false;

				comboRegions.SelectedIndex = -1;
			}
			finally {
				_ignoreValueChanges = false;
			}
		}

        #endregion

        /// <summary>
        /// Constructs a ThumbnailRegion from the dialog's current state.
        /// </summary>
        protected ThumbnailRegion ConstructCurrentRegion() {
            Rectangle bounds = new Rectangle {
                X = (int)numX.Value,
                Y = (int)numY.Value,
                Width = (int)numW.Value,
                Height = (int)numH.Value
            };

            ThumbnailRegion newRegion = (_modeIndex == RelativeModeIndex) ?
                new ThumbnailRegion(bounds, true) :
                new ThumbnailRegion(bounds, (RegionAnchor)_modeIndex);

            return newRegion;
        }

        /// <summary>
        /// Adds a new stored region.
        /// </summary>
        /// <param name="rectangle">Region bounds.</param>
        /// <param name="regionName">Name of the region.</param>
        /// <param name="isRelative">Whether the region is relative to the border.</param>
        private void StoreCurrentRegion(string regionName) {
            StoredRegion storedRegion = new StoredRegion(this.ConstructCurrentRegion(), regionName);

            int index = comboRegions.Items.Add(storedRegion);
            comboRegions.SelectedIndex = index;

            if (Settings.Default.SavedRegions == null)
                Settings.Default.SavedRegions = new StoredRegionArray();
            Settings.Default.SavedRegions.Add(storedRegion);
        }

        /// <summary>
        /// Internal event raised when a change occurs in the selected region.
        /// </summary>
        /// <param name="regionBounds">Region bounds.</param>
        protected virtual void OnRegionSet(ThumbnailRegion region) {
            //Forward region to thumbnail
            ParentMainForm.SelectedThumbnailRegion = region;

            //Have region, allowed to save
            buttonSave.Enabled = true;
        }

        #region GUI event handlers

        private void Close_click(object sender, EventArgs e) {
			OnRequestClosing();
		}
        
		private void Reset_click(object sender, EventArgs e) {
			Reset();
            ParentMainForm.SelectedThumbnailRegion = null;
		}

		private void Delete_click(object sender, EventArgs e) {
            if (comboRegions.SelectedIndex < 0)
                return;

            var origIndex = comboRegions.SelectedIndex;
            comboRegions.SelectedIndex = -1;

			Settings.Default.SavedRegions.RemoveAt(origIndex);
			comboRegions.Items.RemoveAt(origIndex);
		}

		private void Save_click(object sender, EventArgs e) {
			//Display textbox instead of button
            buttonSave.Enabled = buttonDelete.Enabled = false;
			textRegionName.Visible = true;
			textRegionName.Focus();
		}

		private void Save_confirm(object sender, EventArgs e) {
			if (!string.IsNullOrEmpty(textRegionName.Text)) {
                StoreCurrentRegion(textRegionName.Text);
			}

			//Hide textbox and show button again
			textRegionName.Visible = false;
			textRegionName.Text = string.Empty;

            buttonSave.Enabled = buttonDelete.Enabled = true;
		}

		private void Save_lost(object sender, EventArgs e) {
			//Reset textbox
			textRegionName.Visible = false;
            textRegionName.Text = string.Empty;

            buttonSave.Enabled = buttonDelete.Enabled = true;
			buttonSave.Focus();
		}

        // Used to signal to the value change handler that all events should be temporarily ignored.
        bool _ignoreValueChanges = false;

        private void RegionValueSpinner_value_change(object sender, EventArgs e) {
            if (_ignoreValueChanges)
                return;

            OnRegionSet(ConstructCurrentRegion());
        }

        private void RegionCombo_index_change(object sender, EventArgs e) {
            buttonDelete.Enabled = (comboRegions.SelectedIndex >= 0);

            if (comboRegions.SelectedIndex >= 0) {
                var region = comboRegions.SelectedItem as StoredRegion;
                if (region == null) {
                    Log.Write("Stored region at index {0} does not contain a valid StoredRegion instance (object is {2})", comboRegions.SelectedIndex, comboRegions.SelectedItem);
                    return;
                }

                SetRegion(region.Region);
            }
        }

        private void ComboAnchor_index_change(object sender, EventArgs e) {
            if (_ignoreValueChanges || comboAnchor.SelectedIndex < 0 || comboAnchor.SelectedIndex == _modeIndex)
                return;

            //No region (or no source to convert against): only change the mode
            //used for the next region
            if (!numX.Enabled || !ParentMainForm.ThumbnailPanel.IsShowingThumbnail) {
                _modeIndex = comboAnchor.SelectedIndex;
                UpdateRegionLabels();
                return;
            }

            //Get current region (in the previous mode) and switch mode, keeping
            //the region at the same place in the source window
            var region = ConstructCurrentRegion();
            SwitchToMode(region, comboAnchor.SelectedIndex, ParentMainForm.ThumbnailPanel.ThumbnailOriginalSize);

            //Update GUI
            SetRegion(region);
        }

        #endregion

	}

}
