using System;

namespace VocabCardGame.Rest
{
    public enum RestOptionType
    {
        Heal,
        UpgradeCard,
        LearnNewWord,
        Soup
    }

    public enum RestUpgradeOutcome
    {
        None,
        Perfect,
        Good,
        Retry,
        Fail
    }

    [Serializable]
    public class RestOption
    {
        public RestOptionType type;
        public string title;
        public string description;
    }

    [Serializable]
    public class RestUpgradeResult
    {
        public string wordId;
        public int correctCount;
        public RestUpgradeOutcome outcome;
    }

    [Serializable]
    public class RestLearnResult
    {
        public string wordId;
        public bool isCorrect;
        public bool unlocked;
    }
}
