using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VocabCardGame.Combat;
using VocabCardGame.Core;
using VocabCardGame.Data;
using VocabCardGame.Learning;

namespace VocabCardGame.Rest
{
    /// <summary>
    /// 休息站管理器（流程邏輯，UI 由外部接入）
    /// </summary>
    public class RestSiteManager : MonoBehaviour
    {
        [Header("Config")]
        public RestSiteConfig config;

        public event Action<RestOption[]> OnOptionsGenerated;
        public event Action<RestOptionType> OnOptionSelected;
        public event Action<int> OnHealApplied;
        public event Action<List<CardData>> OnUpgradeCandidatesGenerated;
        public event Action<RestUpgradeResult> OnUpgradeCompleted;
        public event Action<List<WordData>> OnLearnCandidatesGenerated;
        public event Action<RestLearnResult> OnLearnCompleted;

        private CardData upgradeCard;
        private int upgradeRemaining;
        private int upgradeCorrectCount;

        private void Start()
        {
            EnsureConfig();
        }

        public RestOption[] BuildOptions()
        {
            EnsureConfig();
            var options = new List<RestOption>
            {
                new RestOption
                {
                    type = RestOptionType.Heal,
                    title = "回血",
                    description = $"回復 {Mathf.RoundToInt(config.healPercent * 100)}% 最大 HP"
                },
                new RestOption
                {
                    type = RestOptionType.UpgradeCard,
                    title = "升級卡牌",
                    description = $"選 1 張卡，答題 {config.upgradeQuizCount} 次升級熟練度"
                },
                new RestOption
                {
                    type = RestOptionType.LearnNewWord,
                    title = "學新字",
                    description = $"從未學池 {config.newWordOptionCount} 選 1"
                }
            };

            if (GameManager.Instance != null && GameManager.Instance.HasRelic("relic_un"))
            {
                options.Add(new RestOption
                {
                    type = RestOptionType.Soup,
                    title = "喝湯",
                    description = $"回復 {Mathf.RoundToInt(config.soupHealPercent * 100)}% HP + 學 1 新字"
                });
            }

            var result = options.ToArray();
            OnOptionsGenerated?.Invoke(result);
            return result;
        }

        public void SelectOption(RestOptionType option)
        {
            EnsureConfig();
            OnOptionSelected?.Invoke(option);

            switch (option)
            {
                case RestOptionType.Heal:
                    ApplyHeal(config.healPercent);
                    break;
                case RestOptionType.UpgradeCard:
                    GenerateUpgradeCandidates();
                    break;
                case RestOptionType.LearnNewWord:
                    GenerateLearnCandidates(config.newWordOptionCount);
                    break;
                case RestOptionType.Soup:
                    ApplyHeal(config.soupHealPercent);
                    GenerateLearnCandidates(config.soupNewWordOptionCount);
                    break;
            }
        }

        private void ApplyHeal(float percent)
        {
            var combatManager = GameManager.Instance?.combatManager;
            if (combatManager == null) return;

            var player = combatManager.GetOrCreatePlayerEntity();
            int amount = Mathf.CeilToInt(player.maxHp * percent);
            player.Heal(amount);
            OnHealApplied?.Invoke(amount);
        }

        public void GenerateUpgradeCandidates()
        {
            var combatManager = GameManager.Instance?.combatManager;
            if (combatManager == null)
            {
                OnUpgradeCandidatesGenerated?.Invoke(new List<CardData>());
                return;
            }

            var candidates = combatManager.GetRunDeckCards();
            OnUpgradeCandidatesGenerated?.Invoke(candidates);
        }

        public void StartUpgrade(CardData card)
        {
            EnsureConfig();
            if (card == null) return;

            upgradeCard = card;
            upgradeRemaining = Mathf.Max(1, config.upgradeQuizCount);
            upgradeCorrectCount = 0;

            StartNextUpgradeQuiz();
        }

        private void StartNextUpgradeQuiz()
        {
            if (upgradeRemaining <= 0)
            {
                CompleteUpgrade();
                return;
            }

            if (QuizManager.Instance == null)
            {
                Debug.LogWarning("[RestSite] QuizManager not found.");
                upgradeRemaining = 0;
                CompleteUpgrade();
                return;
            }

            QuizManager.Instance.StartQuiz(upgradeCard, OnUpgradeQuizCompleted);
        }

        private void OnUpgradeQuizCompleted(bool isCorrect, int quality)
        {
            upgradeRemaining--;
            if (isCorrect) upgradeCorrectCount++;

            GameManager.Instance?.learningManager?.OnAnswerResult(isCorrect);

            if (upgradeRemaining > 0)
            {
                StartNextUpgradeQuiz();
            }
            else
            {
                CompleteUpgrade();
            }
        }

        private void CompleteUpgrade()
        {
            var learningManager = GameManager.Instance?.learningManager;
            RestUpgradeResult result = new RestUpgradeResult
            {
                wordId = upgradeCard != null ? upgradeCard.wordId : string.Empty,
                correctCount = upgradeCorrectCount,
                outcome = RestUpgradeOutcome.None
            };

            if (learningManager != null && upgradeCard != null)
            {
                result.outcome = learningManager.ApplyRestUpgradeResult(upgradeCard.wordId, upgradeCorrectCount, config.upgradeQuizCount);
            }

            OnUpgradeCompleted?.Invoke(result);
        }

        public void GenerateLearnCandidates(int count)
        {
            EnsureConfig();
            var candidates = GetNewWordCandidates(count);
            OnLearnCandidatesGenerated?.Invoke(candidates);
        }

        private List<WordData> GetNewWordCandidates(int count)
        {
            var wordDb = GameManager.Instance?.dataManager?.GetWordDatabase();
            if (wordDb == null) return new List<WordData>();

            var learningManager = GameManager.Instance?.learningManager;
            var learned = learningManager != null
                ? new HashSet<string>(learningManager.wordProgressMap.Keys)
                : new HashSet<string>();

            var pool = wordDb.words
                .Where(w => !learned.Contains(w.id))
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(Mathf.Max(0, count))
                .ToList();

            return pool;
        }

        public void StartLearnNewWord(WordData word)
        {
            EnsureConfig();
            if (word == null) return;

            var card = GameManager.Instance?.dataManager?.GetCard(word.id);
            if (card == null)
            {
                Debug.LogWarning($"[RestSite] Card not found for word: {word.id}");
                return;
            }

            if (QuizManager.Instance == null)
            {
                Debug.LogWarning("[RestSite] QuizManager not found.");
                return;
            }

            QuizManager.Instance.StartQuiz(card, (isCorrect, quality) =>
            {
                var learningManager = GameManager.Instance?.learningManager;
                bool unlocked = false;

                if (learningManager != null)
                {
                    learningManager.OnAnswerResult(isCorrect);
                    if (!config.unlockWordRequiresCorrect || isCorrect)
                    {
                        unlocked = learningManager.UnlockWord(word.id);
                    }
                }

                if (unlocked)
                {
                    GameManager.Instance?.combatManager?.AddCardToDeck(card);
                }

                OnLearnCompleted?.Invoke(new RestLearnResult
                {
                    wordId = word.id,
                    isCorrect = isCorrect,
                    unlocked = unlocked
                });
            });
        }

        private void EnsureConfig()
        {
            if (config == null)
            {
                config = GameManager.Instance?.dataManager?.GetRestSiteConfig() ?? new RestSiteConfig();
            }
        }
    }
}
