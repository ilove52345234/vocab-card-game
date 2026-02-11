using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VocabCardGame.Data;
using VocabCardGame.Core;

namespace VocabCardGame.Learning
{
    /// <summary>
    /// 學習進度管理器
    /// </summary>
    public class LearningManager : MonoBehaviour
    {
        [Header("Progress Data")]
        public Dictionary<string, WordProgress> wordProgressMap = new Dictionary<string, WordProgress>();

        [Header("Daily Limits")]
        public int dailyNewWordsLearned = 0;
        public int maxDailyNewWords = 10;
        public DateTime lastResetDate;

        [Header("Statistics")]
        public int todayCorrectCount = 0;
        public int todayWrongCount = 0;
        public int currentStreak = 0;
        public int bestStreak = 0;

        // 事件
        public event Action<string, ProficiencyLevel> OnWordLevelUp;
        public event Action<string, ProficiencyLevel> OnWordLevelDown;
        public event Action<int> OnStreakUpdated;
        public event Action<string> OnWordDeepened;

        private void Start()
        {
            CheckDailyReset();
        }

        /// <summary>
        /// 檢查每日重置
        /// </summary>
        private void CheckDailyReset()
        {
            if (DateTime.Now.Date > lastResetDate.Date)
            {
                dailyNewWordsLearned = 0;
                todayCorrectCount = 0;
                todayWrongCount = 0;
                lastResetDate = DateTime.Now;
            }
        }

        /// <summary>
        /// 取得每日新詞上限
        /// </summary>
        public int GetDailyNewWordLimit()
        {
            var phase = GameManager.Instance.playerData.GetGamePhase();
            int playDays = GameManager.Instance.playerData.totalPlayDays;

            if (playDays <= 7) return 10;       // 新手期 5-10
            if (playDays <= 30) return 15;      // 成長期 10-15
            return 20;                          // 穩定期 15-20
        }

        /// <summary>
        /// 是否可以學習新詞
        /// </summary>
        public bool CanLearnNewWord()
        {
            return dailyNewWordsLearned < GetDailyNewWordLimit();
        }

        /// <summary>
        /// 解鎖新單字
        /// </summary>
        public bool UnlockWord(string wordId)
        {
            if (!CanLearnNewWord()) return false;

            if (!wordProgressMap.ContainsKey(wordId))
            {
                wordProgressMap[wordId] = new WordProgress
                {
                    wordId = wordId,
                    level = ProficiencyLevel.New,
                    lastReviewTime = DateTime.Now,
                    nextReviewTime = DateTime.Now.AddHours(4)
                };

                dailyNewWordsLearned++;
                GameManager.Instance.playerData.totalWordsLearned++;
                GameManager.Instance.AddExperience(10); // 學習新詞經驗

                return true;
            }

            return false;
        }

        /// <summary>
        /// 進化/書房專用：不受每日上限限制的解鎖
        /// </summary>
        public bool UnlockWordForEvolution(string wordId)
        {
            if (string.IsNullOrWhiteSpace(wordId)) return false;

            if (!wordProgressMap.ContainsKey(wordId))
            {
                wordProgressMap[wordId] = new WordProgress
                {
                    wordId = wordId,
                    level = ProficiencyLevel.New,
                    lastReviewTime = DateTime.Now,
                    nextReviewTime = DateTime.Now.AddHours(4)
                };

                GameManager.Instance.playerData.totalWordsLearned++;
                GameManager.Instance.AddExperience(10);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 取得單字進度
        /// </summary>
        public WordProgress GetProgress(string wordId)
        {
            return wordProgressMap.TryGetValue(wordId, out var progress) ? progress : null;
        }

        /// <summary>
        /// 確保單字有進度資料（MVP 初始化用，不受每日上限限制）
        /// </summary>
        public WordProgress EnsureProgress(string wordId, ProficiencyLevel defaultLevel = ProficiencyLevel.New)
        {
            if (!wordProgressMap.TryGetValue(wordId, out var progress))
            {
                progress = new WordProgress
                {
                    wordId = wordId,
                    level = defaultLevel,
                    lastReviewTime = DateTime.Now,
                    nextReviewTime = DateTime.Now.AddHours(4)
                };
                wordProgressMap[wordId] = progress;
            }

            return progress;
        }

        /// <summary>
        /// 處理答題結果
        /// </summary>
        public void OnAnswerResult(bool isCorrect)
        {
            GameManager.Instance.playerData.totalCorrectAnswers += isCorrect ? 1 : 0;

            if (isCorrect)
            {
                todayCorrectCount++;
                currentStreak++;
                if (currentStreak > bestStreak)
                {
                    bestStreak = currentStreak;
                    GameManager.Instance.playerData.maxConsecutiveCorrect = bestStreak;
                }
                GameManager.Instance.playerData.consecutiveCorrect = currentStreak;
            }
            else
            {
                todayWrongCount++;
                currentStreak = 0;
                GameManager.Instance.playerData.consecutiveCorrect = 0;
            }

            OnStreakUpdated?.Invoke(currentStreak);
            CheckStreakAchievements();
        }

        /// <summary>
        /// 更新單字進度
        /// </summary>
        public void UpdateWordProgress(string wordId, bool isCorrect, int quality)
        {
            if (!wordProgressMap.TryGetValue(wordId, out var progress)) return;

            var oldLevel = progress.level;
            progress.UpdateProgress(isCorrect, quality);
            var newLevel = progress.level;

            if (newLevel > oldLevel)
            {
                OnWordLevelUp?.Invoke(wordId, newLevel);
                GameManager.Instance.AddExperience(GetExpForLevelUp(newLevel));
            }
            else if (newLevel < oldLevel)
            {
                OnWordLevelDown?.Invoke(wordId, newLevel);
            }

            // 保存進度
            GameManager.Instance.dataManager.SaveWordProgress(wordProgressMap);
        }

        private int GetExpForLevelUp(ProficiencyLevel level)
        {
            return level switch
            {
                ProficiencyLevel.Known => 5,
                ProficiencyLevel.Familiar => 10,
                ProficiencyLevel.Remembered => 20,
                ProficiencyLevel.Proficient => 30,
                ProficiencyLevel.Mastered => 40,
                ProficiencyLevel.Internalized => 50,
                _ => 0
            };
        }

        /// <summary>
        /// 取得需要複習的單字
        /// </summary>
        public List<string> GetDueWords(int limit = 50)
        {
            return wordProgressMap
                .Where(kvp => kvp.Value.NeedsReview)
                .OrderBy(kvp => kvp.Value.nextReviewTime)
                .Take(limit)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// 取得各等級單字統計
        /// </summary>
        public Dictionary<ProficiencyLevel, int> GetLevelStatistics()
        {
            var stats = new Dictionary<ProficiencyLevel, int>();
            foreach (ProficiencyLevel level in Enum.GetValues(typeof(ProficiencyLevel)))
            {
                stats[level] = 0;
            }

            foreach (var progress in wordProgressMap.Values)
            {
                stats[progress.level]++;
            }

            return stats;
        }

        public void MarkWordDeepened(string wordId)
        {
            if (!wordProgressMap.TryGetValue(wordId, out var progress)) return;
            if (progress.isDeepened) return;

            progress.isDeepened = true;
            SaveWordProgress(wordProgressMap);
            OnWordDeepened?.Invoke(wordId);
        }

        /// <summary>
        /// 取得各元素單字統計
        /// </summary>
        public Dictionary<Element, int> GetElementStatistics()
        {
            var stats = new Dictionary<Element, int>();
            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                stats[element] = 0;
            }

            var wordDb = GameManager.Instance.dataManager.GetWordDatabase();
            foreach (var kvp in wordProgressMap)
            {
                var word = wordDb.GetWord(kvp.Key);
                if (word != null && kvp.Value.level >= ProficiencyLevel.Mastered)
                {
                    stats[word.element]++;
                }
            }

            return stats;
        }

        /// <summary>
        /// 今日正確率
        /// </summary>
        public float GetTodayAccuracy()
        {
            int total = todayCorrectCount + todayWrongCount;
            return total > 0 ? (float)todayCorrectCount / total : 0f;
        }

        private void CheckStreakAchievements()
        {
            if (currentStreak >= 10) GameManager.Instance.UnlockAchievement("streak_10");
            if (currentStreak >= 50) GameManager.Instance.UnlockAchievement("streak_50");
            if (currentStreak >= 100) GameManager.Instance.UnlockAchievement("streak_100");
            if (currentStreak >= 500) GameManager.Instance.UnlockAchievement("streak_500");
        }

        /// <summary>
        /// 載入進度
        /// </summary>
        public void LoadProgress(Dictionary<string, WordProgress> data)
        {
            wordProgressMap = data ?? new Dictionary<string, WordProgress>();
        }

        /// <summary>
        /// 休息站升級結果套用（依設計規則）
        /// </summary>
        public Rest.RestUpgradeOutcome ApplyRestUpgradeResult(string wordId, int correctCount, int totalCount)
        {
            if (!wordProgressMap.TryGetValue(wordId, out var progress))
            {
                return Rest.RestUpgradeOutcome.None;
            }

            var previousLevel = progress.level;
            var previousNextReview = progress.nextReviewTime;

            progress.lastReviewTime = DateTime.Now;
            progress.correctCount += Mathf.Max(0, correctCount);
            progress.wrongCount += Mathf.Max(0, totalCount - correctCount);

            if (correctCount >= totalCount)
            {
                progress.level = (ProficiencyLevel)Math.Min((int)progress.level + 1, (int)ProficiencyLevel.Internalized);
                progress.nextReviewTime = progress.CalculateNextReviewForLevel(progress.level);
                GameManager.Instance.dataManager.SaveWordProgress(wordProgressMap);
                return Rest.RestUpgradeOutcome.Perfect;
            }

            if (correctCount == totalCount - 1)
            {
                progress.level = (ProficiencyLevel)Math.Min((int)progress.level + 1, (int)ProficiencyLevel.Internalized);
                progress.nextReviewTime = previousNextReview;
                GameManager.Instance.dataManager.SaveWordProgress(wordProgressMap);
                return Rest.RestUpgradeOutcome.Good;
            }

            if (correctCount == 1)
            {
                progress.nextReviewTime = DateTime.Now;
                GameManager.Instance.dataManager.SaveWordProgress(wordProgressMap);
                return Rest.RestUpgradeOutcome.Retry;
            }

            if (correctCount <= 0)
            {
                progress.level = (ProficiencyLevel)Math.Max((int)progress.level - 1, (int)ProficiencyLevel.New);
                progress.nextReviewTime = DateTime.Now;
                GameManager.Instance.dataManager.SaveWordProgress(wordProgressMap);
                return Rest.RestUpgradeOutcome.Fail;
            }

            // 其他情況：保持不變
            progress.level = previousLevel;
            progress.nextReviewTime = previousNextReview;
            GameManager.Instance.dataManager.SaveWordProgress(wordProgressMap);
            return Rest.RestUpgradeOutcome.None;
        }
    }
}
