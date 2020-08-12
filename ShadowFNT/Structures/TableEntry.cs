using System;

namespace ShadowFNT.Structures {

    public struct TableEntry {
        public int subtitleAddress;
        public int messageIdBranchSequence;
        public EntryType entryType;
        public int subtitleActiveTime;
        public int audioId;
        public String subtitle;

        public override string ToString() {
            if (subtitle.Equals("\0"))
                return subtitleAddress.ToString();
            return subtitle;
        }
    }
}
