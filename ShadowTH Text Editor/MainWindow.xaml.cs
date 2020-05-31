using AFSLib;
using ShadowFNT.Structures;
using ShadowTH_Text_Editor.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ShadowTH_Text_Editor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Everything because lazy - to be rewritten in MVVM
    /// </summary>
    /// 
    /// PLAN:
    /// Rewrite following MVVM
    /// Keep track of which .FNT are modified, at save export 'global supported' .met/.txd overwriting originals, warn if ex mets found
    /// Support editing subtitle length
    /// Support replacing and extracting associated audio from audioID and opened AFS (DONE)
    /// Search also filters by subtitles rather than just FNT parent (DONE, but implemented poorly, need to MVVM and use INotifyPropertyChanged & Binding)
    /// 
    /// bonus: 
    /// - preview based on AFS
    /// - Add text to speech AFS option
    /// - Add google translate option
    ///

    public partial class MainWindow : Window {
        List<FNT> initialFntsOpenedState;
        List<FNT> openedFnts;

        FNT currentFnt;
        AfsArchive currentAfs;

        public MainWindow() {
            InitializeComponent();
            SetAFSUI(false);
            FNTHolder button1DataContext = new FNTHolder() { Name = "I'm button 1" };
            //button1.DataContext = button1DataContext;
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
            clearData();
            string[] foundFnts = Directory.GetFiles(dialog.SelectedPath, "*_EN.fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++) {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, dialog.SelectedPath);
                FNT originalFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, dialog.SelectedPath);

                openedFnts.Add(newFnt);
                initialFntsOpenedState.Add(originalFnt);
            }

            ListBox_OpenedFNTS.ItemsSource = openedFnts;
        }

        private void clearData() {
            openedFnts = new List<FNT>();
            initialFntsOpenedState = new List<FNT>();
            currentAfs = null;
            SetAFSUI(false);
            clearUIData();
        }

        private void clearUIData() {
            TextBox_EditSubtitle.Clear();
            TextBlock_AfsAudioIDName.Text = "";
            TextBox_AudioID.Clear();
            TextBox_SubtitleActiveTime.Clear();
        }

        private void ListBox_OpenedFNTS_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            clearUIData();
            if (ListBox_OpenedFNTS.SelectedIndex == -1) {
                ListBox_CurrentFNTOpened.ItemsSource = null;
                return;
            }
            currentFnt = (FNT)ListBox_OpenedFNTS.SelectedItem;
            ListBox_CurrentFNTOpened.ItemsSource = currentFnt.subtitleList;
        }

        private void ListBox_CurrentFNTOpened_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ListBox_CurrentFNTOpened.SelectedItem == null) {
                clearUIData();
                return;
            }
            var currentSubtitleIndex = currentFnt.subtitleList.IndexOf(ListBox_CurrentFNTOpened.SelectedItem.ToString());
            if (currentSubtitleIndex == -1) {
                clearUIData();
                return;
            }
            var audioID = currentFnt.GetSubtitleAudioID(currentSubtitleIndex);

            TextBox_EditSubtitle.Text = ListBox_CurrentFNTOpened.SelectedItem.ToString();
            TextBox_SubtitleActiveTime.Text = currentFnt.GetSubtitleActiveTime(currentSubtitleIndex).ToString();
            TextBox_AudioID.Text = audioID.ToString();

            if (currentAfs == null) {
                TextBlock_AfsAudioIDName.Text = "AFS not loaded"; 
            } else if (audioID != -1 && audioID < currentAfs.Files.Count) {
                TextBlock_AfsAudioIDName.Text = currentAfs.Files[audioID].Name;
            } else {
                TextBlock_AfsAudioIDName.Text = "None";
            }
        }

        private void TextBox_SearchText_TextChanged(object sender, TextChangedEventArgs e) {
            //clear current selection and clear UI (but not fnt data)
        }

        private void Button_SaveCurrentSubtitle_Click(object sender, RoutedEventArgs e) {
            if (ListBox_CurrentFNTOpened.SelectedItem == null)
                return;
            var currentSubtitleIndex = currentFnt.subtitleList.IndexOf(ListBox_CurrentFNTOpened.SelectedItem.ToString());
            if (currentSubtitleIndex == -1) { 
                MessageBox.Show("Error, subtitle not found");
                return;
            }
            currentFnt.UpdateSubtitle(currentSubtitleIndex, TextBox_EditSubtitle.Text);
            currentFnt.UpdateSubtitleAudioID(currentSubtitleIndex, Int32.Parse(TextBox_AudioID.Text));
            currentFnt.UpdateSubtitleActiveTime(currentSubtitleIndex, Int32.Parse(TextBox_SubtitleActiveTime.Text));
            ListBox_CurrentFNTOpened.Items.Refresh();
            TextBox_EditSubtitle.Clear();           
        }

        private void Button_SelectAFSClick(object sender, RoutedEventArgs e) {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
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
                SetAFSUI(true);
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
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (dialog.FileName != "") {
                File.WriteAllBytes(dialog.FileName, currentAfs.Files[Int32.Parse(TextBox_AudioID.Text)].Data);
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
            //TODO: add optional checkbox to manually pick a path, by default overwrite original files
            //      add optional checkbox to NOT replace met/txd
            foreach (FNT fnt in filesToWrite) {
                File.WriteAllBytes(fnt.fileName, fnt.BuildFNTFile().ToArray());
                String prec = fnt.fileName.Remove(fnt.fileName.Length - 4);
                File.Copy("res/EN.txd", prec + ".txd", true);
                File.Copy("res/EN00.met", prec + "00.met", true);
            }
            clearData();
        }

        private void SetAFSUI(bool active) {
            Button_ExportAFS.IsEnabled = active;
            Button_ReplaceADX.IsEnabled = active;
            Button_ExtractADX.IsEnabled = active;
        }
    }
}
