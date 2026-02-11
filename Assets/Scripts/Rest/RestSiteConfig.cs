using System;

namespace VocabCardGame.Rest
{
    /// <summary>
    /// 休息站設定（資料驅動）
    /// </summary>
    [Serializable]
    public class RestSiteConfig
    {
        public float healPercent = 0.3f;
        public int upgradeQuizCount = 3;
        public int newWordOptionCount = 3;
        public bool unlockWordRequiresCorrect = true;

        public float soupHealPercent = 0.15f;
        public int soupNewWordOptionCount = 1;
    }
}
