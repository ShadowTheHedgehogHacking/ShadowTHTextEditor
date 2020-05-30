using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowFNT.Structures {

    /*
        .fnt file
        Everything is little endian.

        fnt {
            Header: length 0x4. Number of entries.
            list of TableEntries
            list of UTF-16 Strings
        }

        TableEntry {
            0x00: Text line internal address (subtitleList entry start position)
            0x04: Unknown. External address?
            0x08: Text type. Byte. See Text types table.
            0x0C: Subtitle active time?
            0x10: AFS Audio ID | FFFFFFFF for cutscene lines.
        }

        TextTypes {
            14 - Menu Text.
            28 - Idle lines.
            64 - Blank/skip entry?
            78 - Partner betrayal lines?
            96 - Automatic lines?
            A0 - Partner Swap lines?
            B4 - Partner objective reaction?
            C8 - Trigger object line? Always in the second-last entry ("No").
            E6 - Partner meet lines.
            FA - Cutscene lines.
            FF - Final Entry. ("Message" in No Message)
        }

     */
    public struct FNT {
        public String fileName;
        public int numberOfEntries;
        private List<SubtitleTableEntry> subtitleTable;
        public List<String> subtitleList;

        public FNT(String fileName, ref byte[] file) {
            this = ParseFNTFile(fileName, ref file);
            this.fileName = fileName;
        }

        /// <summary>
        /// Parses a .fnt file
        /// </summary>
        /// <param name="fileName">Full name (not SafeName) retrieved via a FilePicker</param>
        /// <param name="file">Bytes of file to parse</param>
        /// <returns>FNT</returns>
        public static FNT ParseFNTFile(String fileName, ref byte[] file) {
            FNT fnt = new FNT();

            fnt.fileName = fileName;

            fnt.numberOfEntries = BitConverter.ToInt32(file, 0);
            int positionIndex = 4; // position of byte to read

            fnt.subtitleTable = new List<SubtitleTableEntry>();
            fnt.subtitleList = new List<String>();

            // Length of single table entry
            int subtitleTableEntryStructSize = 0x14;

            // read table entries
            for (int i = 0; i < fnt.numberOfEntries; i++) {
                SubtitleTableEntry entry = new SubtitleTableEntry {
                    startingPosition = BitConverter.ToInt32(file, positionIndex),
                    externalAddress = BitConverter.ToInt32(file, positionIndex + 4),
                    textType = BitConverter.ToInt32(file, positionIndex + 8),
                    subtitleActiveTime = BitConverter.ToInt32(file, positionIndex + 12),
                    audioId = BitConverter.ToInt32(file, positionIndex + 16)
                };

                fnt.subtitleTable.Add(entry);
                positionIndex += subtitleTableEntryStructSize;
            }

            // read UTF-16 strings
            for (int i = 0; i < fnt.numberOfEntries; i++) {
                int subtitleLength;
                if (i == fnt.subtitleTable.Count - 1)
                    // if last subtitleTable entry, size is originalFilesize - entry index
                    subtitleLength = file.Length - fnt.subtitleTable[i].startingPosition;
                else
                    // otherwise calculate based on next entry in list
                    subtitleLength = fnt.subtitleTable[i + 1].startingPosition - fnt.subtitleTable[i].startingPosition;

                String subtitle = Encoding.Unicode.GetString(file, positionIndex, subtitleLength);
                fnt.subtitleList.Add(subtitle);
                positionIndex += subtitleLength;
            }

            return fnt;
        }

        /// <summary>
        /// Returns a .fnt built from the FNT
        /// </summary>
        public List<byte> BuildFNTFile() {
            List<byte> fntFile = new List<byte>();

            // write header
            BitConverter.GetBytes(this.numberOfEntries).ToList().ForEach(b => { fntFile.Add(b); });

            // write table entries
            for (int i = 0; i < numberOfEntries; i++) {
                BitConverter.GetBytes(subtitleTable[i].startingPosition).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].externalAddress).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].textType).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].subtitleActiveTime).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].audioId).ToList().ForEach(b => { fntFile.Add(b); });
            }

            // write UTF-16 entries
            for (int i = 0; i < numberOfEntries; i++)
                Encoding.Unicode.GetBytes(subtitleList[i]).ToList().ForEach(b => { fntFile.Add(b); });

            return fntFile;
        }

        /// <summary>
        /// Update an entry's displayed text.
        /// This function will perform a safe expand/shrink of all succeeding entries in the subtitleList.
        /// 
        /// </summary>
        /// <param name="entryNumber">Index of the subtitle to update</param>
        /// <param name="updatedText">Null terminated string</param>
        public void updateSubtitle(int entryNumber, String updatedText) {
            int characterSizeDifference = updatedText.Length - subtitleList[entryNumber].Length;
            subtitleList[entryNumber] = updatedText;

            // Update SubtitleTableEntry of all succeeding elements to account for length change
            if (characterSizeDifference != 0) {
                for (int i = entryNumber + 1; i < subtitleList.Count; i++) {
                    SubtitleTableEntry updatedEntry = subtitleTable[i];
                    updatedEntry.startingPosition = subtitleTable[i].startingPosition + characterSizeDifference * 2;
                    subtitleTable[i] = updatedEntry;
                }
            }
        }

        /// <summary>
        /// Replaces the associated audio of a subtitle
        /// </summary>
        /// <param name="subtitleEntry">Index of subtitle to update associated audio</param>
        /// <param name="audioId">Audio id to play (index in the AFS)</param>
        public void updateSubtitleAudio(int subtitleEntry, int audioId) {
            SubtitleTableEntry updatedEntry = subtitleTable[subtitleEntry];
            updatedEntry.audioId = audioId;
            subtitleTable[subtitleEntry] = updatedEntry;
        }
        
        /// <summary>
        /// Getter for AudioID of passed in subtitleEntry index
        /// </summary>
        /// <param name="subtitleEntry">Index of subtitle to get audioID</param>
        /// <returns>int</returns>
        public int getSubtitleAudioID(int subtitleEntry) {
            return subtitleTable[subtitleEntry].audioId;             
        }

        /// <summary>
        /// Getter for subtitleActiveTime of passed in subtitleEntry index
        /// </summary>
        /// <param name="subtitleEntry">Index of subtitle to get subtitleActiveTime</param>
        /// <returns>int</returns>
        public int getSubtitleActiveTime(int subtitleEntry) {
            return subtitleTable[subtitleEntry].subtitleActiveTime;
        }
    }
}