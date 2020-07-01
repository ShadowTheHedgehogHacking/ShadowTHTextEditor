using AFSLib;
using ShadowFNT.Structures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MessageBox = System.Windows.MessageBox;

namespace ShadowTH_Text_Editor {
    public partial class MainWindow : Window {
        List<FNT> initialFntsOpenedState;
        List<FNT> openedFnts;
        String lastOpenDir;
        bool localeWarningSeen = false;
        FNT currentFnt;
        AfsArchive currentAfs;
        ICollectionView displayFntsView, displaySubtitleListView;

        public MainWindow() {
            InitializeComponent();
        }

        private void Button_SelectFNTSClick(object sender, RoutedEventArgs e) {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (!dialog.SelectedPath.EndsWith("fonts")) {
                MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog", "Try Again");
                return;
            }
            lastOpenDir = dialog.SelectedPath;
            ProcessFNTS();
        }

        private void ProcessFNTS() {
            ClearData();
            if (lastOpenDir == null) return;
            String localeSwitcher = ComboBox_LocaleSwitcher.SelectedItem.ToString();
            string[] foundFnts = Directory.GetFiles(lastOpenDir, "*_" + localeSwitcher.Substring(localeSwitcher.Length - 2) + ".fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++) {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);
                FNT originalFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);

                openedFnts.Add(newFnt);
                initialFntsOpenedState.Add(originalFnt);
            }
            Button_OpenAFS.IsEnabled = true;
            displayFntsView = CollectionViewSource.GetDefaultView(openedFnts);
            ListBox_OpenedFNTS.ItemsSource = displayFntsView;
        }

        private void ClearData() {
            openedFnts = new List<FNT>();
            initialFntsOpenedState = new List<FNT>();
            currentAfs = null;
            Button_OpenAFS.IsEnabled = false;
            Button_ExportAFS.IsEnabled = false;
            Button_ExportChangedFNTs.IsEnabled = false;
            ClearUIData();
            ListBox_OpenedFNTS.ItemsSource = null;
            ListBox_CurrentFNTOpened.ItemsSource = null;
        }

        private void ClearUIData() {
            TextBox_EditSubtitle.Clear();
            TextBlock_AfsAudioIDName.Text = "";
            TextBox_AudioID.Clear();
            TextBox_SubtitleActiveTime.Clear();
        }

        private void ListBox_OpenedFNTS_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ClearUIData();
            if (ListBox_OpenedFNTS.SelectedIndex == -1) {
                ListBox_CurrentFNTOpened.ItemsSource = null;
                return;
            }
            currentFnt = (FNT)ListBox_OpenedFNTS.SelectedItem;
            ListBox_CurrentFNTOpened.SelectedIndex = -1;
            displaySubtitleListView = CollectionViewSource.GetDefaultView(currentFnt.subtitleList);

            ListBox_CurrentFNTOpened.ItemsSource = displaySubtitleListView;
            UpdateDisplaySubtitleListView();

        }

        private void ListBox_CurrentFNTOpened_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ListBox_CurrentFNTOpened.SelectedItem == null) {
                ClearUIData();
                return;
            }
            var currentSubtitleIndex = currentFnt.subtitleList.IndexOf(ListBox_CurrentFNTOpened.SelectedItem.ToString());
            if (currentSubtitleIndex == -1) {
                ClearUIData();
                return;
            }
            var audioID = currentFnt.GetSubtitleAudioID(currentSubtitleIndex);

            TextBox_EditSubtitle.Text = ListBox_CurrentFNTOpened.SelectedItem.ToString();
            TextBox_SubtitleActiveTime.Text = currentFnt.GetSubtitleActiveTime(currentSubtitleIndex).ToString();
            TextBox_AudioID.Text = audioID.ToString();

            if (currentAfs == null) {
                TextBlock_AfsAudioIDName.Text = "AFS not loaded";
                Button_ExtractADX.IsEnabled = false;
                Button_ReplaceADX.IsEnabled = false;
            } else if (audioID != -1 && audioID < currentAfs.Files.Count) {
                TextBlock_AfsAudioIDName.Text = currentAfs.Files[audioID].Name;
                Button_ExtractADX.IsEnabled = true;
                Button_ReplaceADX.IsEnabled = true;
            } else {
                TextBlock_AfsAudioIDName.Text = "None";
                Button_ExtractADX.IsEnabled = false;
                Button_ReplaceADX.IsEnabled = false;
            }
        }

        private void TextBox_SearchFilters_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateDisplayFntsView();
            UpdateDisplaySubtitleListView();
        }

        private void UpdateDisplayFntsView() {
            if (displayFntsView == null)
                return;
            displayFntsView.Filter = fnt => {
                FNT curfnt = (FNT)fnt;
                for (int i = 0; i < curfnt.subtitleList.Count; i++) {
                    if (curfnt.subtitleList[i].ToLower().Contains(TextBox_SearchText.Text.ToLower())) {
                        if (TextBox_SearchAudioFileName.Text == "" || currentAfs == null)
                            return true;

                        var audioId = curfnt.GetSubtitleAudioID(i);

                        if (audioId == -1)
                            continue;

                        if (currentAfs.Files[audioId].Name.Contains(TextBox_SearchAudioFileName.Text.ToLower()))
                            return true;
                    }
                }
                return false;
            };
            displayFntsView.Refresh();
        }

        private void UpdateDisplaySubtitleListView() {
            if (displaySubtitleListView == null)
                return;
            displaySubtitleListView.Filter = sub => {
                String subtitle = (String)sub;
                if (subtitle.ToLower().Contains(TextBox_SearchText.Text.ToLower())) {
                    if (TextBox_SearchAudioFileName.Text == "" || currentAfs == null)
                        return true;

                    var audioId = currentFnt.GetSubtitleAudioID(currentFnt.subtitleList.IndexOf(subtitle));

                    if (audioId == -1)
                        return false;

                    if (currentAfs.Files[audioId].Name.Contains(TextBox_SearchAudioFileName.Text.ToLower()))
                        return true;
                }
                return false;
            };
            displaySubtitleListView.Refresh();
        }

        private void Button_SaveCurrentSubtitle_Click(object sender, RoutedEventArgs e) {
            if (ListBox_CurrentFNTOpened.SelectedItem == null)
                return;
            var currentSubtitleIndex = currentFnt.subtitleList.IndexOf(ListBox_CurrentFNTOpened.SelectedItem.ToString());
            if (currentSubtitleIndex == -1) { 
                MessageBox.Show("Error, subtitle not found, report this bug");
                return;
            }
            currentFnt.UpdateSubtitle(currentSubtitleIndex, TextBox_EditSubtitle.Text);
            currentFnt.UpdateSubtitleAudioID(currentSubtitleIndex, Int32.Parse(TextBox_AudioID.Text));
            currentFnt.UpdateSubtitleActiveTime(currentSubtitleIndex, Int32.Parse(TextBox_SubtitleActiveTime.Text));
            UpdateDisplayFntsView();
            UpdateDisplaySubtitleListView();
            Button_ExportChangedFNTs.IsEnabled = true;
            TextBox_EditSubtitle.Clear();
            ListBox_CurrentFNTOpened.SelectedIndex = currentSubtitleIndex;
        }

        private void Button_SelectAFSClick(object sender, RoutedEventArgs e) {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.Filter = "AFS files (*.afs)|*.afs|All files (*.*)|*.*";
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (!dialog.FileName.ToLower().EndsWith(".afs")) {
                MessageBox.Show("Pick an 'AFS' file", "Try Again");
                return;
            }
            var data = File.ReadAllBytes(dialog.FileName);
            if (AfsArchive.TryFromFile(data, out var afsArchive)) {
                currentAfs = afsArchive;
                Button_ExportAFS.IsEnabled = true;
                if (ListBox_CurrentFNTOpened.SelectedItem == null)
                    return;
                var currentSubtitleIndex = currentFnt.subtitleList.IndexOf(ListBox_CurrentFNTOpened.SelectedItem.ToString());
                if (currentSubtitleIndex == -1) {
                    return;
                }
                ListBox_CurrentFNTOpened.SelectedIndex = -1;
                ListBox_CurrentFNTOpened.SelectedIndex = currentSubtitleIndex;
            };
        }

        private void Button_ExportAFSClick(object sender, RoutedEventArgs e) {
            if (currentAfs == null)
                return;
            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog();
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (dialog.FileName != "") {
                File.WriteAllBytes(dialog.FileName, currentAfs.ToBytes());
            }
        }

        private void Button_ReplaceADXClick(object sender, RoutedEventArgs e) {
            if (currentAfs == null)
                return;
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (dialog.FileName != "") {
                var newData = File.ReadAllBytes(dialog.FileName);
                currentAfs.Files[Int32.Parse(TextBox_AudioID.Text)].Data = newData;
                return;
            }
            MessageBox.Show("Failed");
        }

        private void Button_ExtractADXClick(object sender, RoutedEventArgs e) {
            if (currentAfs == null)
                return;
            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog();
            dialog.FileName = TextBlock_AfsAudioIDName.Text;
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (dialog.FileName != "") {
                File.WriteAllBytes(dialog.FileName, currentAfs.Files[Int32.Parse(TextBox_AudioID.Text)].Data);
            }
        }

        private void Button_About_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("Created by dreamsyntax\n" +
                ".fnt struct reversal done by LimblessVector\n" +
                ".met/.txd universal map by TheHatedGravity\n" +
                "Uses AFSLib by Sewer56 for AFS support\n" +
                "Uses Ookii.Dialogs for dialogs\n\n" +
                "https://github.com/ShadowTheHedgehogHacking\n\nto check for updates for this software.", "About ShadowTH Text Editor / FNT Editor");
        }

        private void ComboBox_LocaleSwitcher_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (initialFntsOpenedState == null) return;
            ProcessFNTS();
        }

        private void ComboBox_LocaleSwitcher_DropDownClosed(object sender, EventArgs e) {
            if (!localeWarningSeen) {
                MessageBox.Show("Saving in languages other than English is currently unsupported.\n" +
                    "Attempting this will cause issues, as the \'global\' .met used does not contain full non-English characters.");
                localeWarningSeen = true;
            }
        }

        private void Button_ExportChangedFNTsClick(object sender, RoutedEventArgs e) {
            List<FNT> filesToWrite = new List<FNT>();
            String filesToWriteReportingString = "";
            for (int i = 0; i < initialFntsOpenedState.Count; i++) {
                if (initialFntsOpenedState[i].Equals(openedFnts[i]) == false) {
                    filesToWrite.Add(openedFnts[i]);
                    filesToWriteReportingString = filesToWriteReportingString + "\n" + openedFnts[i];
                }
            }
            MessageBox.Show("Files to be written:" + filesToWriteReportingString);
            var customSavePath = "";
            if (CheckBox_ChooseWhereToSave.IsChecked == true) {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                if (dialog.ShowDialog() == false) {
                    MessageBox.Show("Save cancelled");
                    return;
                }
                customSavePath = dialog.SelectedPath;
            }
            foreach (FNT fnt in filesToWrite) {
                if (CheckBox_ChooseWhereToSave.IsChecked == true) {
                    var newFntFilePath = customSavePath + '\\' + fnt.fileName.Split(fnt.filterString + '\\')[1];
                    Directory.CreateDirectory(Directory.GetParent(newFntFilePath).FullName);
                    File.WriteAllBytes(newFntFilePath, fnt.BuildFNTFile().ToArray());
                    if (CheckBox_NoReplaceMetTxd.IsChecked == false) {
                        String prec = newFntFilePath.Remove(newFntFilePath.Length - 4);
                        File.Copy("res/EN.txd", prec + ".txd", true);
                        File.Copy("res/EN00.met", prec + "00.met", true);
                    }
                } else {
                    File.WriteAllBytes(fnt.fileName, fnt.BuildFNTFile().ToArray());
                    if (CheckBox_NoReplaceMetTxd.IsChecked == false) {
                        String prec = fnt.fileName.Remove(fnt.fileName.Length - 4);
                        File.Copy("res/EN.txd", prec + ".txd", true);
                        File.Copy("res/EN00.met", prec + "00.met", true);
                    }
                }
            }
            ClearData();
        }
    }
}
