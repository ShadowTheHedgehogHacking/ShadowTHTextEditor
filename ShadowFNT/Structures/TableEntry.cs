using System;

namespace ShadowFNT.Structures {

    public struct TableEntry {
        public int subtitleAddress;
        public int messageIdBranchSequence;
        public EntryType entryType;
        public int subtitleActiveTime;
        public int audioId;
        public string subtitle;

        public override string ToString() {
            if (subtitle.Equals("\0"))
                return "NULL SUBTITLE | Address: " + subtitleAddress.ToString();
            return subtitle;
        }
    }
}
