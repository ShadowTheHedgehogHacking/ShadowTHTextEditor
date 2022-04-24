using AFSLib;
using ShadowFNT.Structures;
using ShadowTH_Text_Editor.Helpers;
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
        private List<FNT> initialFntsOpenedState;
        private List<FNT> openedFnts;
        private string lastOpenDir;
        private bool localeWarningSeen = false;
        private FNT currentFnt;
        private AfsArchive currentAfs;
        private ICollectionView displayFntsView, displayTableListView;

        public MainWindow() {
            InitializeComponent();
        }

        private void Button_SelectFNTSClick(object sender, RoutedEventArgs e) {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (!dialog.SelectedPath.EndsWith("fonts")) {
                MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Try Again");
                return;
            }
            lastOpenDir = dialog.SelectedPath;
            ProcessFNTS();
        }

        private void ProcessFNTS() {
            ClearData();
            if (lastOpenDir == null) return;
            string localeSwitcher = ComboBox_LocaleSwitcher.SelectedItem.ToString();
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
            ListBox_AllFNTS.ItemsSource = displayFntsView;
        }

        private void ClearData() {
            openedFnts = new List<FNT>();
            initialFntsOpenedState = new List<FNT>();
            currentAfs = null;
            Button_OpenAFS.IsEnabled = false;
            Button_ExportAFS.IsEnabled = false;
            Button_ExportChangedFNTs.IsEnabled = false;
            Button_PreviewADX.IsEnabled = false;
            Button_AddEntry.IsEnabled = false;
            ClearUIData();
            ListBox_AllFNTS.ItemsSource = null;
            ListBox_CurrentFNT.ItemsSource = null;
        }

        private void ClearUIData() {
            TextBlock_SubtitleAddress.Text = "";
            TextBox_MessageIdBranchSequence.Clear();
            TextBox_EditSubtitle.Clear();
            TextBlock_AfsAudioIDName.Text = "";
            TextBox_AudioID.Clear();
            TextBox_SubtitleActiveTime.Clear();
            Button_DeleteEntry.IsEnabled = false;
            Button_GotoSelected.IsEnabled = false;
        }

        private void ListBox_OpenedFNTS_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ClearUIData();
            if (ListBox_AllFNTS.SelectedIndex == -1) {
                ListBox_CurrentFNT.ItemsSource = null;
                return;
            }
            currentFnt = (FNT)ListBox_AllFNTS.SelectedItem;
            ListBox_CurrentFNT.SelectedIndex = -1;
            displayTableListView = CollectionViewSource.GetDefaultView(currentFnt.entryTable);

            ListBox_CurrentFNT.ItemsSource = displayTableListView;
            UpdateDisplayTableListView();

        }

        private void ListBox_CurrentFNT_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ListBox_CurrentFNT.SelectedItem == null) {
                ClearUIData();
                return;
            }
            var currentSubtitleIndex = currentFnt.entryTable.IndexOf((TableEntry)ListBox_CurrentFNT.SelectedItem);
            if (currentSubtitleIndex == -1) {
                ClearUIData();
                return;
            }
            Button_DeleteEntry.IsEnabled = true;
            Button_GotoSelected.IsEnabled = true;
            var audioID = currentFnt.GetEntryAudioID(currentSubtitleIndex);

            TextBlock_SubtitleAddress.Text = currentFnt.GetEntrySubtitleAddress(currentSubtitleIndex).ToString();
            TextBox_EditSubtitle.Text = currentFnt.GetEntrySubtitle(currentSubtitleIndex);
            TextBox_MessageIdBranchSequence.Text = currentFnt.GetEntryMessageIdBranchSequence(currentSubtitleIndex).ToString();
            EntryType currentTextType = currentFnt.GetEntryEntryType(currentSubtitleIndex);
            ComboBox_EntryType.SelectedIndex = Array.IndexOf(Enum.GetValues(currentTextType.GetType()), currentTextType);
            TextBox_SubtitleActiveTime.Text = currentFnt.GetEntryActiveTime(currentSubtitleIndex).ToString();
            TextBox_AudioID.Text = audioID.ToString();

            if (currentAfs == null) {
                TextBlock_AfsAudioIDName.Text = "AFS not loaded";
                Button_ExtractADX.IsEnabled = false;
                Button_ReplaceADX.IsEnabled = false;
                Button_PreviewADX.IsEnabled = false;
            } else if (audioID != -1 && audioID < currentAfs.Files.Count) {
                TextBlock_AfsAudioIDName.Text = currentAfs.Files[audioID].Name;
                Button_ExtractADX.IsEnabled = true;
                Button_ReplaceADX.IsEnabled = true;
                Button_PreviewADX.IsEnabled = true;
            } else {
                TextBlock_AfsAudioIDName.Text = "None";
                Button_ExtractADX.IsEnabled = false;
                Button_ReplaceADX.IsEnabled = false;
                Button_PreviewADX.IsEnabled = false;
            }
        }          

        private void TextBox_SearchFilters_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateDisplayFntsView();
            UpdateDisplayTableListView();
        }

        private void UpdateDisplayFntsView() {
            if (displayFntsView == null)
                return;
            displayFntsView.Filter = fnt => {
                FNT curfnt = (FNT)fnt;
                for (int i = 0; i < curfnt.entryTable.Count; i++) {
                    if (curfnt.entryTable[i].subtitle.ToLower().Contains(TextBox_SearchText.Text.ToLower())) {
                        if (TextBox_SearchAudioFileName.Text == "" || currentAfs == null)
                            return true;

                        var audioId = curfnt.GetEntryAudioID(i);

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

        private void UpdateDisplayTableListView() {
            if (displayTableListView == null)
                return;
            displayTableListView.Filter = entry => {
                TableEntry tblEntry = (TableEntry)entry;
                if (tblEntry.subtitle.ToLower().Contains(TextBox_SearchText.Text.ToLower())) {
                    if (TextBox_SearchAudioFileName.Text == "" || currentAfs == null)
                        return true;

                    var audioId = currentFnt.GetEntryAudioID(currentFnt.entryTable.IndexOf(tblEntry));

                    if (audioId == -1)
                        return false;

                    if (currentAfs.Files[audioId].Name.Contains(TextBox_SearchAudioFileName.Text.ToLower()))
                        return true;
                }
                return false;
            };
            displayTableListView.Refresh();
        }

        private void Button_SaveCurrentEntry_Click(object sender, RoutedEventArgs e) {
            if (ListBox_CurrentFNT.SelectedItem == null)
                return;
            var currentEntryIndex = currentFnt.entryTable.IndexOf((TableEntry)ListBox_CurrentFNT.SelectedItem);
            if (currentEntryIndex == -1) { 
                MessageBox.Show("Error, subtitle not found, report this bug and what you did to cause it.", "Impossible Bug. If you see this screenshot it!");
                return;
            }
            currentFnt.UpdateEntrySubtitle(currentEntryIndex, TextBox_EditSubtitle.Text);
            currentFnt.UpdateEntryMessageIdBranchSequence(currentEntryIndex, int.Parse(TextBox_MessageIdBranchSequence.Text));
            currentFnt.UpdateEntryEntryType(currentEntryIndex, ComboBox_EntryType.SelectedIndex);
            currentFnt.UpdateEntryAudioID(currentEntryIndex, int.Parse(TextBox_AudioID.Text));
            currentFnt.UpdateEntryActiveTime(currentEntryIndex, int.Parse(TextBox_SubtitleActiveTime.Text));
            UpdateDisplayFntsView();
            UpdateDisplayTableListView();
            Button_ExportChangedFNTs.IsEnabled = true;
            TextBox_EditSubtitle.Clear();
            ListBox_CurrentFNT.SelectedIndex = -1; // trigger deselect-reselect event
            ListBox_CurrentFNT.SelectedIndex = ListBox_CurrentFNT.Items.IndexOf(currentFnt.entryTable[currentEntryIndex]);
         }

        private void Button_SelectAFSClick(object sender, RoutedEventArgs e) {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog {
                Filter = "AFS files (*.afs)|*.afs|All files (*.*)|*.*"
            };
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
                if (ListBox_CurrentFNT.SelectedItem == null)
                    return;
                var currentSubtitleIndex = currentFnt.entryTable.IndexOf((TableEntry)ListBox_CurrentFNT.SelectedItem);
                if (currentSubtitleIndex == -1) {
                    return;
                }
                ListBox_CurrentFNT.SelectedIndex = -1;
                ListBox_CurrentFNT.SelectedIndex = currentSubtitleIndex;
            };
        }

        private void Button_ExportAFSClick(object sender, RoutedEventArgs e) {
            if (currentAfs == null)
                return;
            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog {
                Filter = "AFS files (*.afs)|*.afs|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (dialog.FileName != "") {
                try {
                    File.WriteAllBytes(dialog.FileName, currentAfs.ToBytes());
                    MessageBox.Show("AFS Successfully Written.", "Success");
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, "An Exception Occurred");
                }
            }
        }

        private void Button_ReplaceADXClick(object sender, RoutedEventArgs e) {
            if (currentAfs == null)
                return;
            Ookii.Dialogs.Wpf.VistaOpenFileDialog dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog {
                Filter = "ADX files (*.adx)|*.adx|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (dialog.FileName != "") {
                try {
                    byte[] newData = File.ReadAllBytes(dialog.FileName);
                    var decoder = new VGAudio.Containers.Adx.AdxReader();
                    VGAudio.Formats.CriAdx.CriAdxFormat audioFormat = (VGAudio.Formats.CriAdx.CriAdxFormat)decoder.ReadFormat(newData);

                    if (audioFormat.ChannelCount >= 2) {
                        if (MessageBox.Show("Replacement ADX is Stereo, needs to be Mono.\nThe voice line will fail to play in-game if Stereo." +
                            "\nCancel Replacement?", "Problem Identified with Replacement", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            return;
                    }
                    if (audioFormat.Version != 4) {
                        if (MessageBox.Show("Shadow ADX files are encoded in ADX Version 4.\nVersion 3 is also supported, but not recommended.\nYour replacement is ADX Version " + audioFormat.Version +
                            "\nCancel Replacement?", "Potential Problem Identified with Replacement", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            return;
                    }
                    currentAfs.Files[int.Parse(TextBox_AudioID.Text)].Data = newData;
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, "Failed to Replace");
                }
                return;
            }
            MessageBox.Show("File was not found", "Error");
        }

        private void Button_ExtractADXClick(object sender, RoutedEventArgs e) {
            if (currentAfs == null)
                return;
            Ookii.Dialogs.Wpf.VistaSaveFileDialog dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog {
                FileName = TextBlock_AfsAudioIDName.Text,
                Filter = "ADX files (*.adx)|*.adx|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (dialog.FileName != "") {
                try {
                    File.WriteAllBytes(dialog.FileName, currentAfs.Files[int.Parse(TextBox_AudioID.Text)].Data);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, "An Exception Occurred");
                }
            }
        }

        private void Button_PreviewADXClick(object sender, RoutedEventArgs e) {
            if (currentAfs == null)
                return;
            int audioId = currentFnt.GetEntryAudioID(currentFnt.entryTable.IndexOf((TableEntry)ListBox_CurrentFNT.SelectedItem));
            if (audioId == -1)
                return;

            try {
                var decoder = new VGAudio.Containers.Adx.AdxReader();
                var audio = decoder.Read(currentAfs.Files[audioId].Data);
                var writer = new VGAudio.Containers.Wave.WaveWriter();
                MemoryStream stream = new MemoryStream();
                writer.WriteToStream(audio, stream);

                var player = new System.Media.SoundPlayer();
                stream.Position = 0;
                player.Stream = stream;
                player.Play();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "An Exception Occurred");
            }
        }

        private void Button_About_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("Created by dreamsyntax\n" +
                ".fnt struct reversal done by LimblessVector & dreamsyntax\n" +
                ".met/.txd universal map and design work by TheHatedGravity\n" +
                "Uses AfsLib by Sewer56 for AFS support\n" +
                "Uses VGAudio by Alex Barney for ADX playback\n" +
                "Uses modified version of DarkTheme by Otiel\n" +
                "Uses Ookii.Dialogs for dialogs\n\n" +
                "https://github.com/ShadowTheHedgehogHacking\n\nto check for updates for this software.", "About ShadowTH Text Editor / FNT Editor v1.5");
        }

        private void ComboBox_LocaleSwitcher_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (initialFntsOpenedState == null) return;
            ProcessFNTS();
        }

        private void ComboBox_LocaleSwitcher_DropDownClosed(object sender, EventArgs e) {
            if (!localeWarningSeen) {
                MessageBox.Show("Saving in languages other than English is currently unsupported.\n" +
                    "Attempting this will cause issues, as the \'global\' .met used does not contain full non-English characters.", "Info");
                localeWarningSeen = true;
            }
        }

        private void TextBox_AddEntryMessageID_TextChanged(object sender, TextChangedEventArgs e) {
            if (TextBox_AddEntryMessageID.Text == "") {
                TextBlock_AddEntryHint.Visibility = Visibility.Visible;
                Button_AddEntry.IsEnabled = false;
            } else {
                TextBlock_AddEntryHint.Visibility = Visibility.Hidden;
                if (initialFntsOpenedState != null)
                    Button_AddEntry.IsEnabled = true;
            }
        }

        private void Button_AddEntry_Click(object sender, RoutedEventArgs e) {
            int result = currentFnt.InsertNewEntry(int.Parse(TextBox_AddEntryMessageID.Text));
            displayTableListView.Refresh();
            if (result == -1) {
                MessageBox.Show("Failed to add, make sure BranchId you picked is not in use\n and is not the first/last entry.", "Error");
            } else {
                MessageBox.Show("Added successfully", "Success");
            }
        }

        private void Button_EntryControls_Question_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("Add a new entry to a fnt.\n\nSpecify the Message ID / Branch / Sequence int to automatically assign the proper position in the fnt.\nAllows for chaining subtitles/audio by adding +1 to an existing number.\n\nDo not add prior to the first or after the last entry in a fnt.\n\nFormat: XYZ\n    X=Message ID\n    Y=Branch\n    Z=Sequence\n\nExample: 25101\nMessage ID = 25\nBranch = 1 (Normal)\nSequence = 01 (second entry in a chain)\n\nExample branch:\n25000 = Dark\n25100 = Normal\n25200 = Hero\n\nExample sequence:\n25100 = first entry\n25101 = second entry", "Info");
        }

        private void Button_DeleteEntry_Click(object sender, RoutedEventArgs e) {
            currentFnt.DeleteEntry(currentFnt.entryTable.IndexOf((TableEntry)ListBox_CurrentFNT.SelectedItem));
            ListBox_CurrentFNT.SelectedIndex = -1;
            UpdateDisplayTableListView();
            Button_ExportChangedFNTs.IsEnabled = true;
        }

        private void Button_GotoEntryNumber_Click(object sender, RoutedEventArgs e) {
            var response = Microsoft.VisualBasic.Interaction.InputBox("Enter Index Number", "Jump to Entry Number...", "0");
            try
            {
                var element = int.Parse(response);
                TableEntry entry = currentFnt.entryTable[element];
                TextBox_SearchText.Text = "";
                TextBox_SearchAudioFileName.Text = "";
                UpdateDisplayTableListView();
                ListBox_CurrentFNT.ScrollIntoView(entry);
                ListBox_CurrentFNT.SelectedIndex = element;
            } catch (Exception ex)
            {
                // catch all
                MessageBox.Show("Invalid Input\n" + ex.Message);
                return;
            }
        }

        private void Button_GotoSelected_Click(object sender, RoutedEventArgs e) {
            TableEntry entry = (TableEntry)ListBox_CurrentFNT.SelectedItem;
            TextBox_SearchText.Text = "";
            TextBox_SearchAudioFileName.Text = "";
            UpdateDisplayTableListView();
            ListBox_CurrentFNT.ScrollIntoView(entry);
        }

        private void Button_DebugMenu_Click(object sender, RoutedEventArgs e)
        {
            new DebugMenu().Show();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (CoreWindow.Width > 675)
                FileControlsGroupBoxTranslateTransform.X = 200;
            else
                FileControlsGroupBoxTranslateTransform.X = 0;
        }

        private void CheckBox_DarkMode_Checked(object sender, RoutedEventArgs e)
        {
            ThemeHelper.ApplySkin(Skin.Dark);
            SetGroupBoxBorder(0.1d);

        }

        private void CheckBox_DarkMode_Unchecked(object sender, RoutedEventArgs e)
        {
            ThemeHelper.ApplySkin(Skin.Light);
            SetGroupBoxBorder(1);
        }

        private void SetGroupBoxBorder(double multiplier)
        {
            double thickness = 1.0d * multiplier;
            var value = new Thickness(thickness, thickness, thickness, thickness);
            GroupBoxEntries.BorderThickness = value;
            GroupBoxEntryControls.BorderThickness = value;
            GroupBoxFileControls.BorderThickness = value; ;
            GroupBoxFNTFiles.BorderThickness = value;
            GroupBoxMisc.BorderThickness = value;
            GroupBoxSearchFilters.BorderThickness = value;
            GroupBoxCurrentEntryAttributes.BorderThickness = value;
        }

        private void Button_ExportChangedFNTsClick(object sender, RoutedEventArgs e) {
            List<FNT> filesToWrite = new List<FNT>();
            string filesToWriteReportingString = "";
            for (int i = 0; i < initialFntsOpenedState.Count; i++) {
                if (initialFntsOpenedState[i].Equals(openedFnts[i]) == false) {
                    filesToWrite.Add(openedFnts[i]);
                    filesToWriteReportingString = filesToWriteReportingString + "\n" + openedFnts[i];
                }
            }
            if (filesToWriteReportingString == "")
            {
                MessageBox.Show("No changes detected. Nothing will be written.", "Report");
                return;
            }
            MessageBox.Show("Files to be written:" + filesToWriteReportingString, "Report");
            var customSavePath = "";
            if (CheckBox_ChooseWhereToSave.IsChecked == true) {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                if (dialog.ShowDialog() == false) {
                    MessageBox.Show("Save cancelled.", "Cancelled");
                    return;
                }
                customSavePath = dialog.SelectedPath;
            }
            foreach (FNT fnt in filesToWrite) {
                try {
                    if (CheckBox_RecalculateAddresses.IsChecked == true)
                    {
                        fnt.RecomputeAllSubtitleAddresses();
                    }
                    if (CheckBox_ChooseWhereToSave.IsChecked == true)
                    {
                        var newFntFilePath = customSavePath + '\\' + fnt.fileName.Split(fnt.filterString + '\\')[1];
                        Directory.CreateDirectory(Directory.GetParent(newFntFilePath).FullName);
                        File.WriteAllBytes(newFntFilePath, fnt.BuildFNTFile().ToArray());
                        if (CheckBox_NoReplaceMetTxd.IsChecked == false)
                        {
                            string prec = newFntFilePath.Remove(newFntFilePath.Length - 4);
                            File.Copy("res/EN.txd", prec + ".txd", true);
                            File.Copy("res/EN00.met", prec + "00.met", true);
                        }
                    }
                    else
                    {
                        File.WriteAllBytes(fnt.fileName, fnt.BuildFNTFile().ToArray());
                        if (CheckBox_NoReplaceMetTxd.IsChecked == false)
                        {
                            string prec = fnt.fileName.Remove(fnt.fileName.Length - 4);
                            File.Copy("res/EN.txd", prec + ".txd", true);
                            File.Copy("res/EN00.met", prec + "00.met", true);
                        }
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Failed on " + fnt.ToString(), "An Exception Occurred");
                    MessageBox.Show(ex.Message, "An Exception Occurred");
                }
            }
            ClearData();
        }
    }
}
