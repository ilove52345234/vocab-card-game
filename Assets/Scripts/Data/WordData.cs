using System;
using System.Collections.Generic;
using UnityEngine;

namespace VocabCardGame.Data
{
    /// <summary>
    /// 單字資料（靜態，從 JSON 載入）
    /// </summary>
    [Serializable]
    public class WordData
    {
        public string id;               // 唯一識別碼
        public string english;          // 英文單字
        public string chinese;          // 中文意思
        public string phonetic;         // 音標
        public string partOfSpeech;     // 詞性 (n./v./adj./adv.)
        public Element element;         // 所屬元素
        public string tribe;            // 所屬種族 (Animals, Food, etc.)
        public string[] keywords;       // 關鍵字標籤
        public string[] exampleSentences; // 例句
        public string[] confusables;    // 易混淆詞（用於出題）
        public string audioPath;        // 發音檔路徑
        public Rarity rarity;           // 稀有度
        public int difficulty;          // 難度等級 1-10
    }

    /// <summary>
    /// 單字學習進度（動態，存檔用）
    /// </summary>
    [Serializable]
    public class WordProgress
    {
        public string wordId;
        public ProficiencyLevel level = ProficiencyLevel.Locked;
        public int correctCount;        // 累計答對次數
        public int wrongCount;          // 累計答錯次數
        public DateTime lastReviewTime; // 上次複習時間
        public DateTime nextReviewTime; // 下次複習時間
        public float easeFactor = 2.5f; // SM-2 演算法的 EF 值

        /// <summary>
        /// 計算答題正確率
        /// </summary>
        public float Accuracy =>
            (correctCount + wrongCount) > 0
                ? (float)correctCount / (correctCount + wrongCount)
                : 0f;

        /// <summary>
        /// 是否需要複習
        /// </summary>
        public bool NeedsReview => DateTime.Now >= nextReviewTime;

        /// <summary>
        /// 更新複習進度（基於 SM-2 演算法）
        /// </summary>
        public void UpdateProgress(bool isCorrect, int quality)
        {
            lastReviewTime = DateTime.Now;

            if (isCorrect)
            {
                correctCount++;

                // SM-2 演算法調整
                easeFactor = Math.Max(1.3f, easeFactor + (0.1f - (5 - quality) * (0.08f + (5 - quality) * 0.02f)));

                // 升級檢查
                if (CanLevelUp())
                {
                    level = (ProficiencyLevel)Math.Min((int)level + 1, (int)ProficiencyLevel.Internalized);
                }
            }
            else
            {
                wrongCount++;
                easeFactor = Math.Max(1.3f, easeFactor - 0.2f);

                // 降級檢查
                if (CanLevelDown())
                {
                    level = (ProficiencyLevel)Math.Max((int)level - 1, (int)ProficiencyLevel.New);
                }
            }

            // 計算下次複習時間
            nextReviewTime = CalculateNextReview();
        }

        private bool CanLevelUp()
        {
            // 連續答對條件
            return correctCount >= GetRequiredCorrectForLevel(level);
        }

        private bool CanLevelDown()
        {
            // 答錯就降級（簡化規則）
            return true;
        }

        private int GetRequiredCorrectForLevel(ProficiencyLevel level)
        {
            return level switch
            {
                ProficiencyLevel.New => 3,
                ProficiencyLevel.Known => 5,
                ProficiencyLevel.Familiar => 7,
                ProficiencyLevel.Remembered => 10,
                ProficiencyLevel.Proficient => 15,
                ProficiencyLevel.Mastered => 20,
                _ => 999
            };
        }

        private DateTime CalculateNextReview()
        {
            // 根據熟練度決定間隔
            int hours = level switch
            {
                ProficiencyLevel.New => 4,
                ProficiencyLevel.Known => 24,
                ProficiencyLevel.Familiar => 48,
                ProficiencyLevel.Remembered => 96,
                ProficiencyLevel.Proficient => 168,
                ProficiencyLevel.Mastered => 336,
                ProficiencyLevel.Internalized => 720,
                _ => 4
            };

            return DateTime.Now.AddHours(hours * easeFactor);
        }
    }

    /// <summary>
    /// 詞庫管理器
    /// </summary>
    [Serializable]
    public class WordDatabase
    {
        public List<WordData> words = new List<WordData>();
        public Dictionary<string, WordData> wordLookup = new Dictionary<string, WordData>();

        public void BuildLookup()
        {
            wordLookup.Clear();
            foreach (var word in words)
            {
                wordLookup[word.id] = word;
            }
        }

        public WordData GetWord(string id)
        {
            return wordLookup.TryGetValue(id, out var word) ? word : null;
        }

        public List<WordData> GetWordsByElement(Element element)
        {
            return words.FindAll(w => w.element == element);
        }

        public List<WordData> GetWordsByTribe(string tribe)
        {
            return words.FindAll(w => w.tribe == tribe);
        }
    }
}
