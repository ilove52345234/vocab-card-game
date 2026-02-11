using System;

namespace VocabCardGame.StudyRoom
{
    public enum StudyRoomOptionType
    {
        Preview,
        Stash,
        Evolve,
        Deepen,
        Notes
    }

    [Serializable]
    public class StudyRoomOption
    {
        public StudyRoomOptionType type;
        public string title;
        public string description;
        public int cost;
        public bool isAvailable;
    }
}
