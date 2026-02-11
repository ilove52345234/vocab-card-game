using System;
using System.Collections.Generic;

namespace VocabCardGame.Evolution
{
    [Serializable]
    public class EvolutionConfig
    {
        public List<EvolutionEntry> entries = new List<EvolutionEntry>();
    }

    [Serializable]
    public class EvolutionEntry
    {
        public string wordId;
        public List<EvolutionOption> options = new List<EvolutionOption>();
    }

    public enum EvolutionOptionType
    {
        Evolve,
        Deepen,
        Continue
    }

    [Serializable]
    public class EvolutionOption
    {
        public EvolutionOptionType type;
        public string targetWordId;
        public string title;
        public string description;
    }
}
