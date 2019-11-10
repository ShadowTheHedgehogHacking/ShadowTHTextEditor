namespace ShadowFNT.Structures {

    struct SubtitleTableEntry {
        public int startingPosition;
        public int externalAddress;
        public int textType; // enumify later if going to make this modifiable
        public int subtitleActiveTime;
        public int audioId;
    }
}
