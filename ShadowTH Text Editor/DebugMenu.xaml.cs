using ShadowFNT.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShadowTH_Text_Editor
{
    /// <summary>
    /// Interaction logic for DebugMenu.xaml
    /// </summary>
    public partial class DebugMenu : Window
    {
        public DebugMenu()
        {
            InitializeComponent();
        }

        private void Button_CompareFNT_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("WARNING: FNTs MUST have same entry counts to compare");

            MessageBox.Show("Pick first file");
            var file1Picker = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            if (file1Picker.ShowDialog() == false)
            {
                return;
            }
            if (!file1Picker.FileName.EndsWith(".fnt"))
            {
                MessageBox.Show("Pick a .fnt file extracted from Shadow The Hedgehog.", "Try Again");
                return;
            }
            MessageBox.Show("Pick second file");
            var file2Picker = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            if (file2Picker.ShowDialog() == false)
            {
                return;
            }
            if (!file2Picker.FileName.EndsWith(".fnt"))
            {
                MessageBox.Show("Pick a .fnt file extracted from Shadow The Hedgehog.", "Try Again");
                return;
            }
            byte[] file1 = File.ReadAllBytes(file1Picker.FileName);
            byte[] file2 = File.ReadAllBytes(file2Picker.FileName);
            FNT fnt1 = FNT.ParseFNTFile(file1Picker.FileName, ref file1);
            FNT fnt2 = FNT.ParseFNTFile(file2Picker.FileName, ref file2);
            String differenceLog = "";
            for (int i = 0; i < fnt1.entryTable.Count; i++)
            {
                String currentDiff = "";

                var subtitleAddress = fnt1.GetEntrySubtitleAddress(i);
                var messageIdBranchSequence = fnt1.GetEntryMessageIdBranchSequence(i);
                var type = fnt1.GetEntryEntryType(i);
                var activeTime = fnt1.GetEntryActiveTime(i);
                var audioId = fnt1.GetEntryAudioID(i);
                var subtitle = fnt1.GetEntrySubtitle(i);

                var subtitleAddress2 = fnt2.GetEntrySubtitleAddress(i);
                var messageIdBranchSequence2 = fnt2.GetEntryMessageIdBranchSequence(i);
                var type2 = fnt2.GetEntryEntryType(i);
                var activeTime2 = fnt2.GetEntryActiveTime(i);
                var audioId2 = fnt2.GetEntryAudioID(i);
                var subtitle2 = fnt2.GetEntrySubtitle(i);
                // TODO: we can get rid of the second loop if we treat element : element ratio rule (no adds/deletes)
                /*                for (int j = 0; i < fnt2.entryTable.Count; j++)
                                {
                                    if (activeTime == fnt2.GetEntryActiveTime(j))
                                    audioId == fnt2.GetEntryAudioID(j)
                                    type == fnt2.GetEntryEntryType(j)
                                    messageIdBranchSequence == fnt2.GetEntryMessageIdBranchSequence(j)
                                    subtitle == fnt2.GetEntrySubtitle(j)
                                    subtitleAddress == fnt2.GetEntrySubtitleAddress(j)
                                }*/
                if (subtitleAddress != subtitleAddress2)
                {
                    // TODO: probably split up subtitleAddress to a different log since all n+1 elements will be shifted on an edit
                    // currentDiff += subtitleAddress + " |!|!| " + subtitleAddress2 + "\n";
                }
                if (messageIdBranchSequence != messageIdBranchSequence2)
                {
                    currentDiff += "messageIdBranchSequence: " + messageIdBranchSequence + " |!|!| " + messageIdBranchSequence2 + "\n";
                }
                if (type != type2)
                {
                    currentDiff += "entryType: " + type + " |!|!| " + type2 + "\n";
                }
                if (activeTime != activeTime2)
                {
                    currentDiff += "activeTime: " + activeTime + " |!|!| " + activeTime2 + "\n";
                }
                if (audioId != audioId2)
                {
                    currentDiff += "audioId: " + audioId + " |!|!| " + audioId2 + "\n";
                }
                if (subtitle != subtitle2)
                {
                    currentDiff += "subtitle:\n|!|!|\n" + subtitle + "\n|!|!|\n" + subtitle2 + "\n|!|!|\n";
                }

                if (currentDiff != "")
                {
                    differenceLog += "Entry " + i.ToString() + " differences:\n" + currentDiff + "\n\n";
                }

            }
            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == false)
            {
                return;
            }
            if (dialog.FileName != "")
            {
                try
                {
                    File.WriteAllText(dialog.FileName, differenceLog);
                    MessageBox.Show("Log Successfully Written.", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "An Exception Occurred");
                }
            }
        }
    }
}
