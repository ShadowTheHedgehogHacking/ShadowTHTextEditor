using AFSLib;
using ShadowFNT.Structures;
using ShadowTH_Text_Editor.Models;
using System;
using System.Collections.Generic;
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
        List<FNT> openedFnts;

        int currentFnt;
        AfsArchive currentAfs;

        public MainWindow() {
            InitializeComponent();
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
                openedFnts.Add(newFnt);
            }
            ListBox_OpenedFNTS.ItemsSource = openedFnts;
        }

        private void clearData() {
            openedFnts = new List<FNT>();
            ListBox_CurrentFNTOpened.Items.Clear();
            currentAfs = null;
        }

        private void ListBox_OpenedFNTS_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListBox_CurrentFNTOpened.Items.Clear();
            TextBox_EditSubtitle.Clear();
            if (ListBox_OpenedFNTS.SelectedIndex == -1)
                return;
            currentFnt = ListBox_OpenedFNTS.SelectedIndex;
            var subtitleList = openedFnts[currentFnt].subtitleList;
            foreach (String subtitle in subtitleList) {
                ListBox_CurrentFNTOpened.Items.Add(subtitle);
            }
        }

        private void ListBox_CurrentFNTOpened_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ListBox_CurrentFNTOpened.SelectedItem == null)
                return;
            TextBox_EditSubtitle.Text = ListBox_CurrentFNTOpened.SelectedItem.ToString();
            TextBox_SubtitleActiveTime.Text = openedFnts[currentFnt].GetSubtitleActiveTime(ListBox_CurrentFNTOpened.SelectedIndex).ToString();
            var audioID = openedFnts[currentFnt].GetSubtitleAudioID(ListBox_CurrentFNTOpened.SelectedIndex);
            TextBox_AudioID.Text = audioID.ToString();
            if (currentAfs != null && audioID != -1) {
                TextBox_AfsAudioIDName.Text = currentAfs.Files[audioID].Name;
            } else {
                TextBox_AfsAudioIDName.Text = "";
            }
        }

        private void TextBox_SearchText_TextChanged(object sender, TextChangedEventArgs e) {

        }

        private void Button_SaveCurrentSubtitle_Click(object sender, RoutedEventArgs e) {
            int selectedSubtitle = ListBox_CurrentFNTOpened.SelectedIndex;
            if (selectedSubtitle == -1)
                return;
            openedFnts[currentFnt].UpdateSubtitle(selectedSubtitle, TextBox_EditSubtitle.Text);
            ListBox_CurrentFNTOpened.Items[selectedSubtitle] = TextBox_EditSubtitle.Text;
            TextBox_EditSubtitle.Clear();           
        }

        private void Button_SelectAFSClick(object sender, RoutedEventArgs e) {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (!dialog.FileName.ToLower().EndsWith(".afs")) {
                MessageBox.Show("Pick an 'afs' file", "Try Again");
                return;
            }
            var data = File.ReadAllBytes(dialog.FileName);
            if (AfsArchive.TryFromFile(data, out var afsArchive)) {
                currentAfs = afsArchive;
                MessageBox.Show("Success");
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

        private void Button_UpdateADXClick(object sender, RoutedEventArgs e) {
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
    }
}
