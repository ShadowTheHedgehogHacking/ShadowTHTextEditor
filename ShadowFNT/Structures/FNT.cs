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
            list of TableEntry
            list of UTF-16 Strings (Note: for convenience and eq(), sticking Strings inside TableEntry, but will be written per original file)
        }

        TableEntry {
            0x00: Text line internal address (subtitleList entry start position)
            0x04: Unknown. External address?
            0x08: Text type. Byte. See Text types table.
            0x0C: Subtitle active time?
            0x10: AFS Audio ID | -1 for cutscene lines.
        }
    */
    public struct FNT {
        public String fileName;
        public String filterString;
        public int numberOfEntries;
        public List<TableEntry> subtitleTable;

        public FNT(String fileName, ref byte[] file) {
            this = ParseFNTFile(fileName, ref file);
            this.fileName = fileName;
        }

        public override string ToString() {
            if (filterString != "")
                return fileName.Split(filterString + '\\')[1];
            return fileName;
        }

        public override bool Equals(object obj) {
            //don't forget to modify later if going to support adding new entries
            FNT compareFnt = (FNT)obj;
            for (int i = 0; i < subtitleTable.Count; i++) {
                if (subtitleTable[i].subtitle != compareFnt.subtitleTable[i].subtitle)
                    return false;
                if (subtitleTable[i].subtitleAddress != compareFnt.subtitleTable[i].subtitleAddress)
                    return false;
                if (subtitleTable[i].messageIdBranchSequence != compareFnt.subtitleTable[i].messageIdBranchSequence)
                    return false;
                if (subtitleTable[i].entryType != compareFnt.subtitleTable[i].entryType)
                    return false;
                if (subtitleTable[i].subtitleActiveTime != compareFnt.subtitleTable[i].subtitleActiveTime)
                    return false;
                if (subtitleTable[i].audioId != compareFnt.subtitleTable[i].audioId)
                    return false;
            }
            return true;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <summary>
        /// Parses a .fnt file
        /// </summary>
        /// <param name="fileName">Full name (not SafeName) retrieved via a FilePicker</param>
        /// <param name="file">Bytes of file to parse</param>
        /// <param name="filterString">String to remove when ToString() is called</param>
        /// <returns>FNT</returns>
        public static FNT ParseFNTFile(String fileName, ref byte[] file, String filterString = "") {
            FNT fnt = new FNT();

            fnt.fileName = fileName;
            fnt.filterString = filterString;

            fnt.numberOfEntries = BitConverter.ToInt32(file, 0);
            int positionIndex = 4; // position of byte to read

            fnt.subtitleTable = new List<TableEntry>();

            // Length of single table entry
            int subtitleTableEntryStructSize = 0x14;

            // read table entries
            for (int i = 0; i < fnt.numberOfEntries; i++) {
                TableEntry entry = new TableEntry {
                    subtitleAddress = BitConverter.ToInt32(file, positionIndex),
                    messageIdBranchSequence = BitConverter.ToInt32(file, positionIndex + 4),
                    entryType = (EntryType)BitConverter.ToInt32(file, positionIndex + 8),
                    subtitleActiveTime = BitConverter.ToInt32(file, positionIndex + 12),
                    audioId = BitConverter.ToInt32(file, positionIndex + 16)
                };

                fnt.subtitleTable.Add(entry);
                positionIndex += subtitleTableEntryStructSize;
            }

            // read UTF-16 strings
            for (int i = 0; i < fnt.numberOfEntries; i++) {
                int subtitleLength;
                String subtitle;
                if (i == fnt.subtitleTable.Count - 1) {
                    // if last subtitleTable entry, size is originalFilesize - entry index
                    // however the original .fnt files sometimes have junk strings at the end
                    // end at first "\0"
                    subtitleLength = file.Length - fnt.subtitleTable[i].subtitleAddress;
                    subtitle = Encoding.Unicode.GetString(file, positionIndex, subtitleLength).Split("\0")[0];
                } else {
                    // otherwise calculate based on next entry in list
                    subtitleLength = fnt.subtitleTable[i + 1].subtitleAddress - fnt.subtitleTable[i].subtitleAddress;
                    subtitle = Encoding.Unicode.GetString(file, positionIndex, subtitleLength);
                }
                fnt.UpdateSubtitle(i, subtitle);
                positionIndex += subtitleLength;
            }

            return fnt;
        }

        public int GetSubtitleAddress(int subtitleEntry) {
            return subtitleTable[subtitleEntry].subtitleAddress;
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
                BitConverter.GetBytes(subtitleTable[i].subtitleAddress).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].messageIdBranchSequence).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes((int)subtitleTable[i].entryType).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].subtitleActiveTime).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(subtitleTable[i].audioId).ToList().ForEach(b => { fntFile.Add(b); });
            }

            // write UTF-16 entries
            for (int i = 0; i < numberOfEntries; i++)
                Encoding.Unicode.GetBytes(subtitleTable[i].subtitle).ToList().ForEach(b => { fntFile.Add(b); });

            return fntFile;
        }

        public String GetSubtitle(int entryNumber) {
            return subtitleTable[entryNumber].subtitle;
        }

        /// <summary>
        /// Update an entry's subtitle.
        /// Performs a safe expand/shrink of all succeeding entries.
        /// 
        /// </summary>
        /// <param name="entryNumber">Index of the entry to update</param>
        /// <param name="updatedText">Null terminated string</param>
        public void UpdateSubtitle(int entryNumber, String updatedText) {
            TableEntry entry = subtitleTable[entryNumber];
            int characterSizeDifference = 0;

            if (entry.subtitle != null) {
                characterSizeDifference = updatedText.Length - entry.subtitle.Length;
            }

            entry.subtitle = updatedText;
            subtitleTable[entryNumber] = entry;

            // Update TableEntry of all succeeding elements to account for length change
            if (characterSizeDifference != 0) {
                for (int i = entryNumber + 1; i < subtitleTable.Count; i++) {
                    TableEntry succeedingEntry = subtitleTable[i];
                    succeedingEntry.subtitleAddress = subtitleTable[i].subtitleAddress + characterSizeDifference * 2;
                    subtitleTable[i] = succeedingEntry;
                }
            }
        }

        /// <summary>
        /// Sets the audioID of a subtitle
        /// </summary>
        /// <param name="tableEntry">Index of subtitle to update associated audio</param>
        /// <param name="audioId">Audio id to play (index in the AFS)</param>
        public void UpdateSubtitleAudioID(int tableEntry, int audioId) {
            TableEntry updatedEntry = subtitleTable[tableEntry];
            updatedEntry.audioId = audioId;
            subtitleTable[tableEntry] = updatedEntry;
        }
        
        /// <summary>
        /// Getter for audioID of passed in subtitleEntry index
        /// </summary>
        /// <param name="subtitleEntry">Index of subtitle to get audioID</param>
        /// <returns>int</returns>
        public int GetSubtitleAudioID(int subtitleEntry) {
            return subtitleTable[subtitleEntry].audioId;             
        }

        public int GetEntryMessageIdBranchSequence(int subtitleEntry) {
            return subtitleTable[subtitleEntry].messageIdBranchSequence;
        }

        public void UpdateSubtitleExternalAddress(int subtitleEntry, int externalAddressValue) {
            TableEntry updatedEntry = subtitleTable[subtitleEntry];
            updatedEntry.messageIdBranchSequence = externalAddressValue;
            subtitleTable[subtitleEntry] = updatedEntry;
        }

        public EntryType GetEntryType(int subtitleEntry) {
            return subtitleTable[subtitleEntry].entryType;
        }

        public void UpdateSubtitleTextType(int subtitleEntry, int enumIndex) {
            TableEntry updatedEntry = subtitleTable[subtitleEntry];
            EntryType temp = EntryType.BACKGROUND_VOICE; //stub entry to get enum values
            updatedEntry.entryType = (EntryType)(Enum.GetValues(temp.GetType())).GetValue(enumIndex);
            subtitleTable[subtitleEntry] = updatedEntry;
        }

        /// <summary>
        /// Getter for subtitleActiveTime of passed in subtitleEntry index
        /// </summary>
        /// <param name="subtitleEntry">Index of subtitle to get subtitleActiveTime</param>
        /// <returns>int</returns>
        public int GetSubtitleActiveTime(int subtitleEntry) {
            return subtitleTable[subtitleEntry].subtitleActiveTime;
        }

        public void UpdateSubtitleActiveTime(int subtitleEntry, int activeTime) {
            TableEntry updatedEntry = subtitleTable[subtitleEntry];
            updatedEntry.subtitleActiveTime = activeTime;
            subtitleTable[subtitleEntry] = updatedEntry;
        }
    }
}