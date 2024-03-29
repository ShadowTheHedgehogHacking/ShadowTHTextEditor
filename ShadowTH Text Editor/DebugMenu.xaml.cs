﻿using AFSLib;
using NAudio.Wave;
using ShadowFNT;
using ShadowFNT.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;

namespace ShadowTH_Text_Editor
{
    /// <summary>
    /// Interaction logic for DebugMenu.xaml
    /// </summary>
    public partial class DebugMenu : Window
    {
        private List<FNT> initialFntsOpenedState;
        private List<FNT> openedFnts;
        private AfsArchive currentAfs;
        private string lastOpenDir;
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
            for (int i = 0; i < fnt1.GetEntryTableCount(); i++)
            {
                String currentDiff = "";

                var subtitleAddress = fnt1.GetEntrySubtitleAddress(i);
                var messageIdBranchSequence = fnt1.GetEntryMessageIdBranchSequence(i);
                var type = fnt1.GetEntryEntryType(i);
                var activeTime = fnt1.GetEntrySubtitleActiveTime(i);
                var audioId = fnt1.GetEntryAudioId(i);
                var subtitle = fnt1.GetEntrySubtitle(i);

                var subtitleAddress2 = fnt2.GetEntrySubtitleAddress(i);
                var messageIdBranchSequence2 = fnt2.GetEntryMessageIdBranchSequence(i);
                var type2 = fnt2.GetEntryEntryType(i);
                var activeTime2 = fnt2.GetEntrySubtitleActiveTime(i);
                var audioId2 = fnt2.GetEntryAudioId(i);
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
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt"
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

        private void Button_AutoActiveTimeAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("NOTE: Currently chained entries are skipped. Intended for EN fnt and EN AFS currently.", "Warning");
            initialFntsOpenedState = new List<FNT>();
            openedFnts = new List<FNT>();
            // Load all target EN FNTs
            MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Step 1");
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == false)
            {
                return;
            }
            if (!dialog.SelectedPath.EndsWith("fonts"))
            {
                MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Try Again");
                return;
            }
            lastOpenDir = dialog.SelectedPath;

            if (lastOpenDir == null) return;
            string[] foundFnts = Directory.GetFiles(lastOpenDir, "*_EN.fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++)
            {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);
                FNT originalFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);

                openedFnts.Add(newFnt);
                initialFntsOpenedState.Add(originalFnt);
            }

            MessageBox.Show("Pick the 'PRS_VOICE_E.afs' file extracted from Shadow The Hedgehog.", "Step 2");
            var dialog2 = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "AFS files (*.afs)|*.afs|All files (*.*)|*.*"
            };
            if (dialog2.ShowDialog() == false)
            {
                return;
            }
            if (!dialog2.FileName.ToLower().EndsWith(".afs"))
            {
                MessageBox.Show("Pick an 'AFS' file", "Try Again");
                return;
            }
            var data = File.ReadAllBytes(dialog2.FileName);
            if (AfsArchive.TryFromFile(data, out var afsArchive))
            {
                currentAfs = afsArchive;
                data = null; // for GC purpose
            };

            // Do actual processing
            for (int i = 0; i < openedFnts.Count; i++)
            {
                // manually skip Advertise\Advertise_EN.fnt
                if (openedFnts[i].ToString() == "Advertise\\Advertise_EN.fnt")
                    continue;
                // perform checks
                for (int j = 0; j < openedFnts[i].GetEntryTableCount(); j++)
                {
                    var entry = openedFnts[i].GetTableEntry(j);
                    // if audioId = -1 skip
                    if (entry.audioId == -1)
                        continue;

                    // calculate for entries that have succeeding chained entries IFF type is BACKGROUND VOICE (GUN Soldiers)
                    if (entry.entryType == EntryType.BACKGROUND_VOICE)
                    {
                        AutoActiveTime(i, j);
                        // AutoActiveTime()
                    }
                    else
                    {
                        // everything else (non GUN soldier type)

                        // do NOT calculate for entries with succeeding chained entries and other type where 00 has an AudioID and successive entry has -1 for audioID
                        // ex: 652000 -> check succeeding entry for self+1 -> if exist, check next for (self+1)+1 (recursive)
                        // when done skip all entries for every self+1 IFF the successors were -1 for AudioID
                        var successorEntry = openedFnts[i].GetTableEntry(j + 1);
                        if ((entry.messageIdBranchSequence + 1) == successorEntry.messageIdBranchSequence)
                        {
                            // successive entry found, check for audioId
                            if (successorEntry.audioId == -1)
                            {
                                // implies current entry's audio is shared for this successor, abort without changing
                                continue;
                            }
                            //successor has its own AudioID, we can AutoActiveTime() the current entry
                        }
                        // has no successor, we can AutoActiveTime() the current entry
                        // AutoActiveTime();
                        AutoActiveTime(i, j);
                    }
                }
            }
            ExportChangedFNTs();
        }

        private void AutoActiveTime(int fntIndex, int entryIndex)
        {
            var decoder = new VGAudio.Containers.Adx.AdxReader();
            var audio = decoder.Read(currentAfs.Files[openedFnts[fntIndex].GetEntryAudioId(entryIndex)].Data);
            var writer = new VGAudio.Containers.Wave.WaveWriter();
            MemoryStream stream = new MemoryStream();
            writer.WriteToStream(audio, stream);
            stream.Position = 0;
            WaveFileReader wf = new WaveFileReader(stream);
            openedFnts[fntIndex].SetEntrySubtitleActiveTime(entryIndex, (int)(wf.TotalTime.TotalMilliseconds / ((double)1000 / (double)60)));
            wf.Close();
            stream.Close();
        }

        private int LoadFNTs(bool loadAFS, string localeOverride = "EN")
        {
            // Load all target FNTs
            initialFntsOpenedState = new List<FNT>();
            openedFnts = new List<FNT>();

            MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Step 1");
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == false)
            {
                return -1;
            }
            if (!dialog.SelectedPath.EndsWith("fonts"))
            {
                MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Try Again");
                return -1;
            }
            lastOpenDir = dialog.SelectedPath;

            if (lastOpenDir == null) return -1;
            string[] foundFnts = Directory.GetFiles(lastOpenDir, "*_" + localeOverride + ".fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++)
            {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);
                FNT originalFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);

                openedFnts.Add(newFnt);
                initialFntsOpenedState.Add(originalFnt);
            }

            if (!loadAFS)
                return 0;

            MessageBox.Show("Pick the 'PRS_VOICE_*.afs' file extracted from Shadow The Hedgehog.", "Step 2");
            var dialog2 = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "AFS files (*.afs)|*.afs|All files (*.*)|*.*"
            };
            if (dialog2.ShowDialog() == false)
            {
                return -1;
            }
            if (!dialog2.FileName.ToLower().EndsWith(".afs"))
            {
                MessageBox.Show("Pick an 'AFS' file", "Try Again");
                return -1;
            }
            var data = File.ReadAllBytes(dialog2.FileName);
            if (AfsArchive.TryFromFile(data, out var afsArchive))
            {
                currentAfs = afsArchive;
                data = null; // for GC purpose
            };
            return 0;
        }

        private void Button_DumpChainedEntries_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Dumps chained entries. Intended for EN fnt and EN AFS currently.", "Warning");
            initialFntsOpenedState = new List<FNT>();
            openedFnts = new List<FNT>();
            // Load all target EN FNTs
            MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Step 1");
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == false)
            {
                return;
            }
            if (!dialog.SelectedPath.EndsWith("fonts"))
            {
                MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Try Again");
                return;
            }
            lastOpenDir = dialog.SelectedPath;

            if (lastOpenDir == null) return;
            string[] foundFnts = Directory.GetFiles(lastOpenDir, "*_EN.fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++)
            {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);
                FNT originalFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);

                openedFnts.Add(newFnt);
                initialFntsOpenedState.Add(originalFnt);
            }

            MessageBox.Show("Pick the 'PRS_VOICE_E.afs' file extracted from Shadow The Hedgehog.", "Step 2");
            var dialog2 = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "AFS files (*.afs)|*.afs|All files (*.*)|*.*"
            };
            if (dialog2.ShowDialog() == false)
            {
                return;
            }
            if (!dialog2.FileName.ToLower().EndsWith(".afs"))
            {
                MessageBox.Show("Pick an 'AFS' file", "Try Again");
                return;
            }
            var data = File.ReadAllBytes(dialog2.FileName);
            if (AfsArchive.TryFromFile(data, out var afsArchive))
            {
                currentAfs = afsArchive;
                data = null; // for GC purpose
            };


            string dumpLog = "";
            // Do actual processing
            for (int i = 0; i < openedFnts.Count; i++)
            {
                // manually skip Advertise\Advertise_EN.fnt
                if (openedFnts[i].ToString() == "Advertise\\Advertise_EN.fnt")
                    continue;

                dumpLog += "\n\n" + openedFnts[i].ToString() + "\n\n";
                // perform checks
                for (int j = 0; j < openedFnts[i].GetEntryTableCount(); j++)
                {
                    var entry = openedFnts[i].GetTableEntry(j);
                    // if audioId = -1 skip
                    if (entry.audioId == -1)
                        continue;

                    // manually skip IFF type is BACKGROUND VOICE (GUN Soldiers)
                    if (entry.entryType == EntryType.BACKGROUND_VOICE)
                        continue;


                    // dump for entries with succeeding chained entries and other type where 00 has an AudioID and successive entry has -1 for audioID
                    // ex: 652000 -> check succeeding entry for self+1 -> if exist, check next for (self+1)+1 (recursive)
                    // need to be careful for scenario of entry1 [has audio id] -> successor [no audio id] -> successor's successor [has audio id]
                    do
                    {
                        var currentEntry = openedFnts[i].GetTableEntry(j);
                        var successorEntry = openedFnts[i].GetTableEntry(j + 1);
                        if ((currentEntry.messageIdBranchSequence + 1) == successorEntry.messageIdBranchSequence)
                        {
                            dumpLog += currentEntry.messageIdBranchSequence + " AFS: " + currentEntry.audioId + " chained " + successorEntry.messageIdBranchSequence + " AFS: " + successorEntry.audioId + "\n";
                            // successive entry found, check for audioId (optional)
                            /*                            if (successorEntry.audioId == -1)
                                                        {
                                                            // implies current entry's audio is shared for this successor. Dump this

                                                        }*/
                        } else
                        {
                            // no more successor, end the iteration
                            break;
                        }
                        j++; //increment next entry
                    } while (true);
                    // has no successor, we can skip the current entry
                }
            }
            var dialogSave = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt"
            };
            if (dialogSave.ShowDialog() == false)
            {
                return;
            }
            if (dialogSave.FileName != "")
            {
                try
                {
                    File.WriteAllText(dialogSave.FileName, dumpLog);
                    MessageBox.Show("Log Successfully Written.", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "An Exception Occurred");
                }
            }
        }

        private void Button_MFA_ChainedEntries_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("EXPERIMENTAL PROCESS THAT REQUIRES Montreal Forced Aligner. Intended for EN fnt and EN AFS currently.", "Warning");
            initialFntsOpenedState = new List<FNT>();
            openedFnts = new List<FNT>();
            if (LoadFNTs(loadAFS: true) == -1)
                return;

            MessageBox.Show("Next you will pick the folder for corpus output & processing.", "Info");
            MessageBox.Show("The same directory must have TextGrids either in the root or subdirectories BEFORE pressing the next message OK box.", "Info");
            var corpusOutputDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (corpusOutputDialog.ShowDialog() == false)
            {
                return;
            }

            string dumpLog = "";
            // Do actual processing
            for (int i = 0; i < openedFnts.Count; i++)
            {
                // FOR DEBUGGING, ONLY TARGET ONE FILE
                /* if (openedFnts[i].ToString() != "stg0100\\stg0100_EN.fnt")
                    continue; */

                // manually skip Advertise\Advertise_EN.fnt
                if (openedFnts[i].ToString() == "Advertise\\Advertise_EN.fnt")
                    continue;

                dumpLog += "\n\n" + openedFnts[i].ToString() + "\n\n";
                // perform checks
                for (int j = 0; j < openedFnts[i].GetEntryTableCount(); j++)
                {

                    // FOR DEBUGGING - BLACKDOOM ONLY
                    /* if (openedFnts[i].entryTable[j].audioId != -1 && !currentAfs.Files[openedFnts[i].entryTable[j].audioId].Name.Contains("_bd.adx"))
                        continue; */

                    var labTranscript = "";

                    var entry = openedFnts[i].GetTableEntry(j);
                    if (entry.audioId == -1)
                        continue;

                    // manually skip IFF type is BACKGROUND VOICE (GUN Soldiers)
                    if (entry.entryType == EntryType.BACKGROUND_VOICE)
                        continue;

                    // dump for entries with succeeding chained entries and other type where 00 has an AudioID and successive entry has -1 for audioID
                    // ex: 652000 -> check succeeding entry for self+1 -> if exist, check next for (self+1)+1 (recursive)
                    // need to be careful for scenario of entry1 [has audio id] -> successor [no audio id] -> successor's successor [has audio id]
                    var initialEntry = openedFnts[i].GetTableEntry(j);
                    do
                    {
                        var currentEntry = openedFnts[i].GetTableEntry(j);
                        var successorEntry = openedFnts[i].GetTableEntry(j + 1);
                        if ((currentEntry.messageIdBranchSequence + 1) == successorEntry.messageIdBranchSequence && successorEntry.subtitleActiveTime != 0 && successorEntry.audioId == -1)
                        {


                            dumpLog += currentEntry.messageIdBranchSequence + " AFS: " + currentEntry.audioId + " chained " + successorEntry.messageIdBranchSequence + " AFS: " + successorEntry.audioId + "\n";

                            // successive entry found, check for audioId to ensure successor is chained
                            if (successorEntry.audioId == -1) {
                                labTranscript += currentEntry.subtitle.ToLower();
                                labTranscript += "\n\n";
                            }
                        }
                        else
                        {
                            // no more successor, end the iteration
                            if (labTranscript != "")
                            {

                                labTranscript += currentEntry.subtitle.ToLower();
                                labTranscript += "\n\n";
                            }
                            break;
                        }
                        j++; //increment next entry
                    } while (true);

                    if (labTranscript != "")
                    {
                        // has data
                        if (initialEntry.audioId != -1)
                        {
                            var directory = corpusOutputDialog.SelectedPath + "\\" + openedFnts[i].ToString() + "\\";
                            Directory.CreateDirectory(directory);
                            File.WriteAllText(directory + initialEntry.messageIdBranchSequence + ".lab", labTranscript);
                            var decoder = new VGAudio.Containers.Adx.AdxReader();
                            var audio = decoder.Read(currentAfs.Files[initialEntry.audioId].Data);
                            var writer = new VGAudio.Containers.Wave.WaveWriter();
                            FileStream stream = new FileStream(directory + initialEntry.messageIdBranchSequence + ".wav", FileMode.OpenOrCreate);
                            writer.WriteToStream(audio, stream);
                            stream.Close();
                        }

                    }

                    // has no successor, we can skip the current entry
                }
            }

            // now we need to wait until MFA finishes (user manual)
            MessageBox.Show("Your corpus has been generated. Run your forced aligner now (external program) targeting the corpus output. Do NOT press OK until MFA alignment has completed!");

            // Read and parse TextGrids

            // How to get perfect timings:

            // MK 1 version
            // Get 'xmin' from TextGrid for successor word '1'. Convert to millis -> Convert to ActiveTime -> Set to "current"
            // Repeat for any successor, up until the final successor
            // If final successor, time AutoActiveTime (length of adx) and subtract the values assigned to ALL predecessors chained. This result becomes the new ActiveTime for the final successor.

            // MK 2 version
            // Get 'xmax' from every NUL "\0\" in the textgrid. This solves the issue of multi word use in same subtitles ex "jump"
            //

            for (int i = 0; i < openedFnts.Count; i++)
            {
                // FOR DEBUGGING, ONLY TARGET stg0404_EN
                /*                if (openedFnts[i].ToString() != "stg0404\\stg0404_EN.fnt")
                                    continue;*/

                // manually skip Advertise\Advertise_EN.fnt
                if (openedFnts[i].ToString() == "Advertise\\Advertise_EN.fnt")
                    continue;

                // perform checks
                for (int j = 0; j < openedFnts[i].GetEntryTableCount(); j++)
                {

                    // DEBUG FOR BLACKDOOM ONLY
                    /*                    if (openedFnts[i].entryTable[j].audioId != -1 && !currentAfs.Files[openedFnts[i].entryTable[j].audioId].Name.Contains("_bd.adx"))
                                            continue;
                    */
                    var chained = false;
                    var entry = openedFnts[i].GetTableEntry(j);

                    if (entry.audioId == -1)
                        continue;

                    // manually skip IFF type is BACKGROUND VOICE (GUN Soldiers)
                    if (entry.entryType == EntryType.BACKGROUND_VOICE)
                        continue;


                    var initialEntry = openedFnts[i].GetTableEntry(j);
                    var initialEntryIndex = j;
                    var chainPosition = 0;
                    do
                    {
                        chainPosition++; //increment sequence position
                        var currentEntry = openedFnts[i].GetTableEntry(j);
                        var currentEntryIndex = j;
                        var successorEntry = openedFnts[i].GetTableEntry(j + 1);
                        if ((currentEntry.messageIdBranchSequence + 1) == successorEntry.messageIdBranchSequence && successorEntry.subtitleActiveTime != 0 && successorEntry.audioId == -1)
                        {

                            chained = true;
                            // successive entry found, check for audioId to ensure successor is chained
                            if (successorEntry.audioId == -1)
                            {
                                // check self text, and successor's text
                                var directory = corpusOutputDialog.SelectedPath + "\\" + openedFnts[i].ToString() + "\\";

                                string[] textgrid;
                                try
                                {
                                    textgrid = File.ReadAllLines(directory + initialEntry.messageIdBranchSequence + ".TextGrid");
                                } catch (Exception ex) {
                                    MessageBox.Show(ex.Message);
                                    break;
                                }
                                List<int> candidates = new List<int>();


                                // MK 2 with NULL comparison and "number of chained" rather than prior text/first text comparison

                                for (int stringIndex = 0; stringIndex < textgrid.Length; stringIndex++)
                                {
                                    if (textgrid[stringIndex].Contains("\0"))
                                    {
                                        candidates.Add(stringIndex);
                                    }
                                }
                                if (candidates.Count > 0)
                                {
                                    var xLine = textgrid[candidates[chainPosition - 1] - 1];
                                    var seconds = xLine.Split("xmax = ")[1];
                                    var span = TimeSpan.FromSeconds(double.Parse(seconds));
                                    var activeTime = (int)(span.TotalMilliseconds / ((double)1000 / (double)60));

                                    // subtract prior entries from time
                                    for (int pos = 0; pos < chainPosition - 1; pos++)
                                    {
                                        activeTime -= openedFnts[i].GetEntrySubtitleActiveTime(initialEntryIndex + pos);
                                    }

                                    currentEntry.subtitleActiveTime = activeTime;
                                    openedFnts[i].SetEntrySubtitleActiveTime(currentEntryIndex, currentEntry.subtitleActiveTime);
                                } else
                                {
                                    MessageBox.Show("NO CANDIDATES FOR " + currentEntry.messageIdBranchSequence);
                                }
                            }
                        }
                        else
                        {
                            // no more successor, end the iteration
                            if (chained && currentEntry.audioId == -1)
                            {
                                // last entry, calc remaining time
                                var decoder = new VGAudio.Containers.Adx.AdxReader();
                                var audio = decoder.Read(currentAfs.Files[initialEntry.audioId].Data);
                                var writer = new VGAudio.Containers.Wave.WaveWriter();
                                MemoryStream stream = new MemoryStream();
                                writer.WriteToStream(audio, stream);
                                stream.Position = 0;
                                WaveFileReader wf = new WaveFileReader(stream);
                                wf.Close();
                                stream.Close();

                                var lastEntryTime = (int)(wf.TotalTime.TotalMilliseconds / ((double)1000 / (double)60)); //total audio length first

                                for (int pos = 0; pos < chainPosition - 1; pos++) {
                                    lastEntryTime -= openedFnts[i].GetEntrySubtitleActiveTime(initialEntryIndex + pos);
                                }
                                currentEntry.subtitleActiveTime = lastEntryTime;
                                openedFnts[i].SetEntrySubtitleActiveTime(currentEntryIndex, currentEntry.subtitleActiveTime);
                            }
                            break;
                        }
                        j++; //increment next entry
                    } while (true);

                    // has no successor, we can skip the current entry
                }
            }
            ExportChangedFNTs();
        }

        private void Button_AFS_Remove_Unused_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This will delete all unused AFS entries (not referenced by FNT) \nand some hardcoded entries (2P voice lines, game over, death line etc)", "Warning");
            initialFntsOpenedState = new List<FNT>();
            openedFnts = new List<FNT>();
            // Load all target EN FNTs
            MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Step 1");
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == false)
            {
                return;
            }
            if (!dialog.SelectedPath.EndsWith("fonts"))
            {
                MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Try Again");
                return;
            }
            lastOpenDir = dialog.SelectedPath;

            if (lastOpenDir == null) return;
            string[] foundFnts = Directory.GetFiles(lastOpenDir, "*_EN.fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++)
            {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);
                FNT originalFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);

                openedFnts.Add(newFnt);
                initialFntsOpenedState.Add(originalFnt);
            }

            MessageBox.Show("Pick the 'PRS_VOICE_E.afs' file extracted from Shadow The Hedgehog.", "Step 2");
            var dialog2 = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "AFS files (*.afs)|*.afs|All files (*.*)|*.*"
            };
            if (dialog2.ShowDialog() == false)
            {
                return;
            }
            if (!dialog2.FileName.ToLower().EndsWith(".afs"))
            {
                MessageBox.Show("Pick an 'AFS' file", "Try Again");
                return;
            }
            var data = File.ReadAllBytes(dialog2.FileName);
            if (AfsArchive.TryFromFile(data, out var afsArchive))
            {
                currentAfs = afsArchive;
                data = null; // for GC purpose
            };

            HashSet<int> afsIndexes = new()
            {
                // add hardcoded entries
                1,
                2,
                3,
                7,
                8,
                9,
                10,
                11,
                13,
                15,
                16,
                17,
                42,
                24,
                26,
                27,
                29,
                30,
                32,
                34,
                36,
                38,
                40
            };

            // Do actual processing
            for (int i = 0; i < openedFnts.Count; i++)
            {
                // perform checks
                for (int j = 0; j < openedFnts[i].GetEntryTableCount(); j++)
                {
                    var entry = openedFnts[i].GetTableEntry(j);
                    // if audioId = -1 skip
                    if (entry.audioId == -1)
                        continue;

                    afsIndexes.Add(entry.audioId);
                }
            }

            MessageBox.Show("Pick replacement delete file, valid adx required");

            var nulledAdxDialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "ADX files (*.adx)|*.adx|All files (*.*)|*.*",
                DefaultExt = ".adx",
            };
            if (nulledAdxDialog.ShowDialog() == false)
            {
                return;
            }
            byte[] nullAdxSafeFile = File.ReadAllBytes(nulledAdxDialog.FileName);

            // null entries
            for (int afsIndex = 0; afsIndex < currentAfs.Files.Count; afsIndex++) { 
                if (!afsIndexes.Contains(afsIndex))
                {
                    currentAfs.Files[afsIndex].Data = nullAdxSafeFile;
                }
            }

            //// hardcoded removals ///

            for (int i = 0; i < openedFnts.Count; i++)
            {
                // perform checks
                for (int j = 0; j < openedFnts[i].GetEntryTableCount(); j++)
                {
                    var entry = openedFnts[i].GetTableEntry(j);
                    // if audioId = -1 skip
                    if (entry.audioId == -1)
                        continue;

                    // manually skip Advertise\Advertise_EN.fnt
                    if (openedFnts[i].ToString() == "Advertise\\Advertise_EN.fnt")
                        continue;

                    if (entry.subtitle.Contains("orange") || entry.subtitle.Contains("key"))
                    {
                        currentAfs.Files[entry.audioId].Data = nullAdxSafeFile;
                    }
                }
            }


            /// end harcoded removals ///

            // processing complete, export AFS
            if (currentAfs == null)
                return;
            var dialogSave = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
            {
                Filter = "AFS files (*.afs)|*.afs|All files (*.*)|*.*",
                DefaultExt = ".afs",
            };
            if (dialogSave.ShowDialog() == false)
            {
                return;
            }
            if (dialogSave.FileName != "")
            {
                try
                {
                    File.WriteAllBytes(dialogSave.FileName, currentAfs.ToBytes());
                    MessageBox.Show("AFS Successfully Written.", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "An Exception Occurred");
                }
            }
        }

        private void Button_AFS_Dump_Unused_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This will dump all unused AFS entries as .wav files.\nUnused is defined as not referenced by FNT, so there may be some 2P lines etc that are used.", "Info");
            initialFntsOpenedState = new List<FNT>();
            openedFnts = new List<FNT>();
            // Load all target EN FNTs
            MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Step 1");
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == false)
            {
                return;
            }
            if (!dialog.SelectedPath.EndsWith("fonts"))
            {
                MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Try Again");
                return;
            }
            lastOpenDir = dialog.SelectedPath;

            if (lastOpenDir == null) return;
            string[] foundFnts = Directory.GetFiles(lastOpenDir, "*_EN.fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++)
            {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);
                FNT originalFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);

                openedFnts.Add(newFnt);
                initialFntsOpenedState.Add(originalFnt);
            }

            MessageBox.Show("Pick the 'PRS_VOICE_E.afs' file extracted from Shadow The Hedgehog.", "Step 2");
            var dialog2 = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "AFS files (*.afs)|*.afs|All files (*.*)|*.*"
            };
            if (dialog2.ShowDialog() == false)
            {
                return;
            }
            if (!dialog2.FileName.ToLower().EndsWith(".afs"))
            {
                MessageBox.Show("Pick an 'AFS' file", "Try Again");
                return;
            }
            var data = File.ReadAllBytes(dialog2.FileName);
            if (AfsArchive.TryFromFile(data, out var afsArchive))
            {
                currentAfs = afsArchive;
                data = null; // for GC purpose
            };

            HashSet<int> afsIndexes = new();

            // Do actual processing
            for (int i = 0; i < openedFnts.Count; i++)
            {
                // perform checks
                for (int j = 0; j < openedFnts[i].GetEntryTableCount(); j++)
                {
                    var entry = openedFnts[i].GetTableEntry(j);
                    // if audioId = -1 skip
                    if (entry.audioId == -1)
                        continue;

                    afsIndexes.Add(entry.audioId);
                }
            }
            MessageBox.Show("Pick a folder to save the unused audio as .wav");

            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog outFolder = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (outFolder.ShowDialog() == false)
            {
                return;
            }

            // dump entries
            for (int afsIndex = 0; afsIndex < currentAfs.Files.Count; afsIndex++)
            {
                if (!afsIndexes.Contains(afsIndex))
                {
                    var decoder = new VGAudio.Containers.Adx.AdxReader();
                    var audio = decoder.Read(currentAfs.Files[afsIndex].Data);
                    var writer = new VGAudio.Containers.Wave.WaveWriter();
                    FileStream stream = new FileStream(outFolder.SelectedPath + "\\" + afsIndex + "_" + currentAfs.Files[afsIndex].Name, FileMode.CreateNew);
                    writer.WriteToStream(audio, stream);
                    stream.Close();
                }
            }
            MessageBox.Show("Done");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Button_MIT_Wordlist_Swap_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This replaces same-string length words randomly for EN fnt", "Info");
            MessageBox.Show("We use https://www.mit.edu/~ecprice/wordlist.10000 by default, but you can edit res/randomizer-wordlist.txt and change to whatever wordlist you want.");
            initialFntsOpenedState = new List<FNT>();
            openedFnts = new List<FNT>();
            // Load all target EN FNTs
            MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Step 1");
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == false)
            {
                return;
            }
            if (!dialog.SelectedPath.EndsWith("fonts"))
            {
                MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog.", "Try Again");
                return;
            }
            lastOpenDir = dialog.SelectedPath;

            if (lastOpenDir == null) return;
            string[] foundFnts = Directory.GetFiles(lastOpenDir, "*_EN.fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++)
            {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);
                FNT originalFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);

                openedFnts.Add(newFnt);
                initialFntsOpenedState.Add(originalFnt);
            }

            var wordlist_by_length = BuildMITLists();

            int seed = CalculateSeed(TextBox_Seed.Text);
            Random random = new Random(seed);

            // Do actual processing
            for (int i = 0; i < openedFnts.Count; i++)
            {
                // perform checks
                for (int j = 0; j < openedFnts[i].GetEntryTableCount(); j++)
                {
                    openedFnts[i].SetEntrySubtitle(j, MITIfySubtitle(openedFnts[i].GetEntrySubtitle(j), random, wordlist_by_length));
                }
            }
            ExportChangedFNTs();
        }

        private void ExportChangedFNTs()
        {
            List<FNT> filesToWrite = new List<FNT>();
            string filesToWriteReportingString = "";
            for (int i = 0; i < initialFntsOpenedState.Count; i++)
            {
                if (initialFntsOpenedState[i].Equals(openedFnts[i]) == false)
                {
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
            foreach (FNT fnt in filesToWrite)
            {
                try
                {
                    fnt.RecomputeAllSubtitleAddresses();
                    File.WriteAllBytes(fnt.fileName, fnt.ToBytes());
                    string prec = fnt.fileName.Remove(fnt.fileName.Length - 4);
                    File.Copy(AppDomain.CurrentDomain.BaseDirectory + "res/EN.txd", prec + ".txd", true);
                    File.Copy(AppDomain.CurrentDomain.BaseDirectory + "res/EN00.met", prec + "00.met", true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed on " + fnt.ToString(), "An Exception Occurred");
                    MessageBox.Show(ex.Message, "An Exception Occurred");
                }
            }
            initialFntsOpenedState.Clear();
            openedFnts.Clear();
            MessageBox.Show("Completed FNT Exports", "Report");
        }

        private Dictionary<int, string[]> BuildMITLists()
        {
            var resourceFile = AppDomain.CurrentDomain.BaseDirectory + "res/randomizer-wordlist.txt";
            string[] lines = File.ReadAllLines(resourceFile);
            var groupedLines = lines.GroupBy(line => line.Length);

            Dictionary<int, string[]> arraysByLength = new Dictionary<int, string[]>();
            foreach (var group in groupedLines)
            {
                arraysByLength[group.Key] = group.ToArray();
            }

            return arraysByLength;
        }

        static int CalculateSeed(string seedString)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seedString));
                // Convert the first 4 bytes of the hash to an integer for the seed
                int seed = BitConverter.ToInt32(hashBytes, 0);
                return seed;
            }
        }

        private string MITIfySubtitle(string text, Random random, Dictionary<int, string[]> wordlist_by_length)
        {
            if (text.Length == 0)
                return text;

            text = text.Replace("\0", string.Empty); // we can chuck out null terminators, ShadowFNT will auto handle this
            var words = text.Split(' ');

            // account for \n (\r\n should already be translated pre-call)
            var indexesOfNewLines = new List<int>();
            var indexesOfCommas = new List<int>();
            var indexesOfPeriods = new List<int>();
            var indexesOfExclamations = new List<int>();
            var indexesOfQuestions = new List<int>();

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Contains("\n"))
                {
                    indexesOfNewLines.Add(i);
                    words[i] = words[i].Split('\n')[0];
                }
                if (words[i].Contains("."))
                {
                    indexesOfPeriods.Add(i);
                    words[i] = words[i].Split('.')[0];
                }
                if (words[i].Contains(","))
                {
                    indexesOfCommas.Add(i);
                    words[i] = words[i].Split(',')[0];
                }
                if (words[i].Contains("!"))
                {
                    indexesOfExclamations.Add(i);
                    words[i] = words[i].Split('!')[0];
                }
                if (words[i].Contains("?"))
                {
                    indexesOfQuestions.Add(i);
                    words[i] = words[i].Split('?')[0];
                }
            }

            for (int i = 0; i < words.Length; i++)
            {
                string[] targetWordList;
                try
                {
                    targetWordList = wordlist_by_length[words[i].Length];
                } catch (Exception) {
                    targetWordList = wordlist_by_length[12]; // get index 12 instead
                }

                // Check the case of the word. - Thanks Knuxfan24 - https://github.com/Knuxfan24/Sonic-06-Randomiser-Suite/blob/master/MarathonRandomiser/Randomisers/TextRandomiser.cs#L108
                bool isAllUpper = false;
                bool isAllLower = false;
                if (words[i].ToLower() == words[i])
                    isAllLower = true;
                if (words[i].ToUpper() == words[i])
                    isAllUpper = true;

                var newWord = targetWordList[random.Next(targetWordList.Length)];
                if (isAllLower)
                    newWord = newWord.ToLower();
                else if (isAllUpper)
                    newWord = newWord.ToUpper();
                else
                    newWord = char.ToUpper(newWord[0]) + newWord[1..];
                words[i] = newWord;
            }

            foreach (var index in indexesOfCommas)
            {
                words[index] += ",";
            }

            foreach (var index in indexesOfExclamations)
            {
                words[index] += "!";
            }

            foreach (var index in indexesOfPeriods)
            {
                words[index] += ".";
            }


            foreach (var index in indexesOfQuestions)
            {
                words[index] += "?";
            }

            // restore \n
            foreach (var index in indexesOfNewLines)
            {
                words[index] += "\n";
            }

            return String.Join(" ", words);
        }

        private void Button_Randomize_FNTs_Click(object sender, RoutedEventArgs e)
        {
            LoadFNTs(true);

            int seed = CalculateSeed(TextBox_Seed.Text);
            Random random = new Random(seed);

            for (int i = 0; i < openedFnts.Count; i++)
            {
                for (int j = 0; j < openedFnts[i].GetEntryTableCount(); j++)
                {
                    // for now we simply swap everything without caring. We probably have to be careful about final entry etc.
                    // Chained entries not accounted for, so may produce wacky results
                    int donorFNTIndex = random.Next(0, openedFnts.Count - 1);
                    int donotFNTEntryIndex = random.Next(0, initialFntsOpenedState[donorFNTIndex].GetEntryTableCount() - 1);
                    if (initialFntsOpenedState[donorFNTIndex].GetEntryAudioId(donotFNTEntryIndex) == -1)
                    {
                        int audio = random.Next(0, currentAfs.Files.Count - 1);
                        openedFnts[i].SetEntryAudioId(j, audio);
                    }
                    else
                    {
                        openedFnts[i].SetEntryAudioId(j, initialFntsOpenedState[donorFNTIndex].GetEntryAudioId(donotFNTEntryIndex));
                    }

                    openedFnts[i].SetEntrySubtitle(j, initialFntsOpenedState[donorFNTIndex].GetEntrySubtitle(donotFNTEntryIndex));
                    openedFnts[i].SetEntrySubtitleActiveTime(j, initialFntsOpenedState[donorFNTIndex].GetEntrySubtitleActiveTime(donotFNTEntryIndex));
                }
            }
            ExportChangedFNTs();
        }
    }
}
