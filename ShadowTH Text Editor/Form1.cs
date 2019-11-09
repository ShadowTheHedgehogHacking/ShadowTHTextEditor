using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShadowTH_Text_Editor {
    public partial class Form1 : Form {
        private Byte[] originalFile;
        private List<SubtitleTableEntry> subtitleTable;
        private List<String> subtitleList;
        private int selected = -1;

        private int numberOfSubtitles = 0;
        private int subtitleTableEntryStructSize = 0x14;

        private bool clearing = false;

        public Form1() {
            InitializeComponent();
        }

        private void buttonOpen_Click(object sender, EventArgs e) {
            /*
                .fnt file layout
                Everything is little endian.
                ================
                Header: length 0x4. Number of entries.

                Entry info:
                0x00: Text line internal address - Modify based on length of replacement text
                0x04: External address? - Retain original value
                0x08: Text type - Retain original value
                0x0C: Subtitle active time - Retain original value
                0x10: Subtitle ID - Retain original value
             */
            clearData();

            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            originalFile = File.ReadAllBytes(openFileDialog.FileName);

            // TODO: Determine if reverse endian is necessary, Seems to vary per .fnt?? Need to double check
            numberOfSubtitles = BitConverter.ToInt32(originalFile, 0);
            //int numberOfSubtitles = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(originalFile, 0));

            int positionIndex = 4; // skip header

            subtitleTable = new List<SubtitleTableEntry>();
            subtitleList = new List<String>();

            // read table entries
            for (int i = 0; i < numberOfSubtitles; i++) {
                SubtitleTableEntry entry = new SubtitleTableEntry {
                    startingPosition = BitConverter.ToInt32(originalFile, positionIndex),
                    externalAddress = BitConverter.ToInt32(originalFile, positionIndex + 4),
                    textType = BitConverter.ToInt32(originalFile, positionIndex + 8),
                    subtitleActiveTime = BitConverter.ToInt32(originalFile, positionIndex + 12),
                    subtitleId = BitConverter.ToInt32(originalFile, positionIndex + 16)
                };
                //Save entry in a table holding entries which map to UTF-16 entries below
                subtitleTable.Add(entry);
                subtitleTable_ListBox.Items.Add("Subtitle " + i);
                positionIndex += subtitleTableEntryStructSize;
            }

            // read UTF-16 entries
            for (int i = 0; i < numberOfSubtitles; i++) {
                int subtitleLength = calculateSize(i);
                String test = Encoding.Unicode.GetString(originalFile, positionIndex, subtitleLength);
                subtitleList.Add(test);
                positionIndex += subtitleLength;
            }

        }

        private int calculateSize(int entryIndex) {
            // if last subtitleTable entry, size is originalFilesize - entry index
            if (entryIndex == subtitleTable.Count - 1) {
                return originalFile.Length - subtitleTable[entryIndex].startingPosition;
            }

            // otherwise calculate based on next entry in list
            return subtitleTable[entryIndex + 1].startingPosition - subtitleTable[entryIndex].startingPosition;
        }

        struct SubtitleTableEntry {
            public int startingPosition;
            public int externalAddress;
            public int textType; // enumify later if going to make this modifiable
            public int subtitleActiveTime;
            public int subtitleId;
        };

        private void subtitleTable_ListBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (clearing)
                return;
            if (selected != -1)
                updateSubtitles();
            subtitleTextBox.Text = subtitleList[subtitleTable_ListBox.SelectedIndex].Replace("\n", "\r\n");
            selected = subtitleTable_ListBox.SelectedIndex;
        }

        private void buttonSave_Click(object sender, EventArgs e) {
            if (selected == -1)
                return;

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;
            
            updateSubtitles();
            List<byte> updatedFile = new List<byte>();

            // write header
            BitConverter.GetBytes(numberOfSubtitles).ToList().ForEach(b => { updatedFile.Add(b); });

            // write table entries
            for (int i = 0; i < numberOfSubtitles; i++) {
                BitConverter.GetBytes(subtitleTable[i].startingPosition).ToList().ForEach(b => { updatedFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].externalAddress).ToList().ForEach(b => { updatedFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].textType).ToList().ForEach(b => { updatedFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].subtitleActiveTime).ToList().ForEach(b => { updatedFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].subtitleId).ToList().ForEach(b => { updatedFile.Add(b); });
            }

            // write UTF-16 entries
            for (int i = 0; i < numberOfSubtitles; i++)
                Encoding.Unicode.GetBytes(subtitleList[i]).ToList().ForEach(b => { updatedFile.Add(b); });

            File.WriteAllBytes(saveFileDialog.FileName, updatedFile.ToArray());
            
        }

        private void updateSubtitles() {
            String updatedSubtitle = subtitleTextBox.Text.Replace("\r\n", "\n") + "\0";
            int characterSizeDifference = updatedSubtitle.Length - subtitleList[selected].Length;
            subtitleList[selected] = updatedSubtitle;

            // Update SubtitleTableEntry of all succeeding elements to account for length change
            if (characterSizeDifference != 0) {
                for (int i = selected + 1; i < subtitleList.Count; i++) {
                    SubtitleTableEntry updatedEntry = subtitleTable[i];
                    updatedEntry.startingPosition = subtitleTable[i].startingPosition + characterSizeDifference * 2;
                    subtitleTable[i] = updatedEntry;
                }
            }
        }

        private void clearData() {
            clearing = true;
            numberOfSubtitles = 0;
            selected = -1;
            try {
                subtitleList.Clear();
                subtitleTable.Clear();
            } catch (NullReferenceException) {
                //eat the exception; we don't care if clear fails when null
            }
            originalFile = null;

            subtitleTable_ListBox.ClearSelected();
            subtitleTable_ListBox.Items.Clear();

            subtitleTextBox.Text = "";
            clearing = false;
        }

        private void aboutButton_Click(object sender, EventArgs e) {
            MessageBox.Show("Created by dreamsyntax\n" +
                ".fnt struct reversal done by LimblessVector\n\n" +
                "This program does NOT modify the .met nor .txd " +
                "associated with the .fnt you edit. " +
                "This program assumes you already are using a universal .met/.txd" +
                "\n\n" +
                "https://github.com/ShadowTheHedgehogHacking\n\nto check for non-prototype version of this software", "About ShadowTH Text Editor Prototype");
        }
    }
}
