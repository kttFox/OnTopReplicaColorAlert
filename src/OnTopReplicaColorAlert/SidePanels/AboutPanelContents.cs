using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using WindowsFormsAero.TaskDialog;
using System.IO;
using WindowsFormsAero;

namespace OnTopReplicaColorAlert.SidePanels {
    public partial class AboutPanelContents : UserControl {

        public AboutPanelContents() {
            InitializeComponent();

            LocalizePanel();

            this.FixDefaultFont();
        }

        private void LocalizePanel() {
            lblSlogan.Text = Strings.AboutSlogan;
            linkAuthor.Internationalize(Strings.AboutAuthor, Strings.AboutAuthorContent);
            labeledDivider2.Text = Strings.AboutDividerCredits;
            linkCredits.Internationalize(Strings.AboutCreditsSources, Strings.AboutCreditsSourcesContent);
            labelTranslators.Text = string.Format(Strings.AboutTranslators, Strings.AboutTranslatorsContent);
            labeledDivider3.Text = Strings.AboutDividerLicense;
            linkLicense.Internationalize(Strings.AboutLicense, Strings.AboutLicenseContent);
            labeledDivider4.Text = Strings.AboutDividerContribute;
            linkContribute.Internationalize(Strings.AboutContribute, Strings.AboutContributeContent);
        }

        private void LinkHomepage_clicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Shell.Execute(AppStrings.ApplicationWebsite);
        }

        private void LinkAuthor_clicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Shell.Execute(AppStrings.AuthorWebsite);
        }

        private void LinkCredits_click(object sender, LinkLabelLinkClickedEventArgs e) {
            var exeDir = Path.GetDirectoryName(Application.ExecutablePath);
            var filePath = Path.Combine(exeDir, "CREDITS.txt");

            Shell.Execute(filePath);
        }

        private void LinkLicense_click(object sender, LinkLabelLinkClickedEventArgs e) {
            Shell.Execute(AppStrings.MsRlLicenseLink);
        }

        private void LinkContribute_clicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Shell.Execute(AppStrings.LatestCommitsLink);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Shell.Execute("https://github.com/kttFox/OnTopReplicaColorAlert");
        }
    }
}
