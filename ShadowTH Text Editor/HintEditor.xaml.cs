using AFSLib;
using ShadowFNT.Structures;
using ShadowTH_Text_Editor.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MessageBox = System.Windows.MessageBox;

namespace ShadowTH_Text_Editor {
    public partial class HintEditor : Window {
        private FNT currentFnt;
        private AfsArchive currentAfs;
        private ICollectionView displayTableListView;

        public HintEditor() {
            InitializeComponent();
            PreferredThemeCheck();
        }

        private void Button_SelectFNTSClick(object sender, RoutedEventArgs e) {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            if (dialog.ShowDialog() == false) {
                return;
            }
            byte[] readFile = File.ReadAllBytes(dialog.FileName);
            currentFnt = FNT.ParseFNTFile(dialog.FileName, ref readFile, Path.GetDirectoryName(dialog.FileName));
            ListBox_CurrentFNT.SelectedIndex = -1;
            displayTableListView = CollectionViewSource.GetDefaultView(currentFnt.entryTable);
            ListBox_CurrentFNT.ItemsSource = displayTableListView;
            UpdateDisplayTableListView();
        }


        private void ListBox_CurrentFNT_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ListBox_CurrentFNT.SelectedItem == null) {
                //ClearUIData();
                return;
            }
            var currentSubtitleIndex = currentFnt.entryTable.IndexOf((TableEntry)ListBox_CurrentFNT.SelectedItem);
            if (currentSubtitleIndex == -1) {
                //ClearUIData();
                return;
            }
            Button_DeleteEntry.IsEnabled = true;
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

        private void UpdateDisplayTableListView() {
            if (displayTableListView == null)
                return;
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
            UpdateDisplayTableListView();
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
            int audioId;
            try
            {
                audioId = currentFnt.GetEntryAudioID(currentFnt.entryTable.IndexOf((TableEntry)ListBox_CurrentFNT.SelectedItem));
            } catch (NullReferenceException)
            {
                return;
            }
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
                "https://github.com/ShadowTheHedgehogHacking\n\nto check for updates for this software.", "About ShadowTH Text Editor / FNT Editor v1.5.2");
        }

        private void TextBox_AddEntryMessageID_TextChanged(object sender, TextChangedEventArgs e) {
            if (TextBox_AddEntryMessageID.Text == "") {
                TextBlock_AddEntryHint.Visibility = Visibility.Visible;
                Button_AddEntry.IsEnabled = false;
            } else {
                TextBlock_AddEntryHint.Visibility = Visibility.Hidden;
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
            MessageBox.Show("Add a new entry to a _hint.bin", "Info");
        }

        private void Button_DeleteEntry_Click(object sender, RoutedEventArgs e) {
            currentFnt.DeleteEntry(currentFnt.entryTable.IndexOf((TableEntry)ListBox_CurrentFNT.SelectedItem));
            ListBox_CurrentFNT.SelectedIndex = -1;
            UpdateDisplayTableListView();
        }

        private void SetGroupBoxBorder(double multiplier)
        {
            double thickness = 1.0d * multiplier;
            var value = new Thickness(thickness, thickness, thickness, thickness);
            GroupBoxEntries.BorderThickness = value;
            GroupBoxEntryControls.BorderThickness = value;
            GroupBoxCurrentEntryAttributes.BorderThickness = value;
        }

        private void PreferredThemeCheck()
        {
            string themeConfig = AppDomain.CurrentDomain.BaseDirectory + "/theme.ini";
            if (File.Exists(themeConfig))
            {
                foreach (string i in File.ReadAllLines(themeConfig))
                {
                    if (i.StartsWith("Dark"))
                    {
                        ThemeHelper.ApplySkin(Skin.Dark);
                        SetGroupBoxBorder(0.1d);
                        break;
                    } else
                    {
                        ThemeHelper.ApplySkin(Skin.Light);
                        SetGroupBoxBorder(1);
                        break;
                    }
                }
            }
        }
    }
}
