using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ShadowTH_Text_Editor
{
    public partial class Form1 : Form
    {
        private Byte[] originalFile;
        private List<SubtitleTableEntry> subtitleTable;
        private List<String> subtitleList;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
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

            originalFile = File.ReadAllBytes("X:\\Movie_EN.fnt");

            // TODO: Determine if reverse endian is necessary, Seems to vary per .fnt?? Need to double check
            int numberOfSubtitles = BitConverter.ToInt32(originalFile, 0);
            //int numberOfSubtitles = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(originalFile, 0));

            int subtitleTableEntryStructSize = 0x14;
            int positionIndex = 4; //skip header

            subtitleTable = new List<SubtitleTableEntry>();
            subtitleList = new List<String>();

            //read table entries
            for (int i = 0; i < numberOfSubtitles; i++)
            {
                SubtitleTableEntry entry = new SubtitleTableEntry
                {
                    startingPosition = BitConverter.ToInt32(originalFile, positionIndex),
                    externalAddress = BitConverter.ToInt32(originalFile, positionIndex + 4),
                    textType = BitConverter.ToInt32(originalFile, positionIndex + 8),
                    subtitleActiveTime = BitConverter.ToInt32(originalFile, positionIndex + 12),
                    subtitleId = BitConverter.ToInt32(originalFile, positionIndex + 16)
                };
                //Save entry in a table holding entries which map to UTF-16 entries below
                subtitleTable.Add(entry);
                positionIndex += subtitleTableEntryStructSize;
            }

            //read UTF-16 entries
            for (int i = 0; i < numberOfSubtitles; i++)
            {
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

            //otherwise calculate based on next entry in list
            return subtitleTable[entryIndex + 1].startingPosition - subtitleTable[entryIndex].startingPosition;
        }

        struct SubtitleTableEntry
        {
            public int startingPosition;
            public int externalAddress;
            public int textType; //enumify later if going to make this modifiable
            public int subtitleActiveTime;
            public int subtitleId;
        };
    }
}
