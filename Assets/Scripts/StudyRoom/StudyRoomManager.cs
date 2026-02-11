using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VocabCardGame.Combat;
using VocabCardGame.Core;
using VocabCardGame.Data;
using VocabCardGame.Evolution;
using VocabCardGame.Learning;

namespace VocabCardGame.StudyRoom
{
    /// <summary>
    /// 書房管理器（流程邏輯，UI 由外部接入）
    /// </summary>
    public class StudyRoomManager : MonoBehaviour
    {
        [Header("Config")]
        public StudyRoomConfig config;

        public event Action<StudyRoomOption[]> OnOptionsGenerated;
        public event Action<StudyRoomOptionType> OnOptionSelected;
        public event Action<int> OnLearningPointsChanged;
        public event Action<List<CardData>> OnStashCandidatesGenerated;
        public event Action<string> OnCardStashed;
        public event Action<List<WordData>> OnPreviewCandidatesGenerated;
        public event Action<string, bool> OnPreviewCompleted;
        public event Action<List<CardData>> OnEvolutionCandidatesGenerated;
        public event Action<List<CardData>> OnDeepenCandidatesGenerated;
        public event Action<CardData> OnEvolutionRequested;
        public event Action<CardData> OnDeepenRequested;
        public event Action<string> OnNoteUpdated;

        private void Start()
        {
            EnsureConfig();
        }

        public StudyRoomOption[] BuildOptions()
        {
            EnsureConfig();
            int lp = GameManager.Instance != null ? GameManager.Instance.GetLearningPoints() : 0;

            var options = new List<StudyRoomOption>
            {
                new StudyRoomOption
                {
                    type = StudyRoomOptionType.Preview,
                    title = "預習",
                    description = $"花費 {config.previewCost} LP，預習新單字",
                    cost = config.previewCost,
                    isAvailable = lp >= config.previewCost
                },
                new StudyRoomOption
                {
                    type = StudyRoomOptionType.Stash,
                    title = "暫放",
                    description = $"花費 {config.stashCost} LP，暫放 1 張卡（本次 Run 移出）",
                    cost = config.stashCost,
                    isAvailable = lp >= config.stashCost
                },
                new StudyRoomOption
                {
                    type = StudyRoomOptionType.Evolve,
                    title = "進化",
                    description = $"花費 {config.evolveCost} LP，詞族進化",
                    cost = config.evolveCost,
                    isAvailable = lp >= config.evolveCost
                },
                new StudyRoomOption
                {
                    type = StudyRoomOptionType.Deepen,
                    title = "深化",
                    description = $"花費 {config.deepenCost} LP，解鎖 Lv.8-9",
                    cost = config.deepenCost,
                    isAvailable = lp >= config.deepenCost
                },
                new StudyRoomOption
                {
                    type = StudyRoomOptionType.Notes,
                    title = "筆記",
                    description = $"花費 {config.noteCost} LP，新增或修改單字筆記",
                    cost = config.noteCost,
                    isAvailable = lp >= config.noteCost
                }
            };

            var result = options.ToArray();
            OnOptionsGenerated?.Invoke(result);
            return result;
        }

        public void SelectOption(StudyRoomOptionType option)
        {
            EnsureConfig();
            OnOptionSelected?.Invoke(option);

            switch (option)
            {
                case StudyRoomOptionType.Preview:
                    GeneratePreviewCandidates();
                    break;
                case StudyRoomOptionType.Stash:
                    GenerateStashCandidates();
                    break;
                case StudyRoomOptionType.Evolve:
                    GenerateEvolutionCandidates();
                    break;
                case StudyRoomOptionType.Deepen:
                    GenerateDeepenCandidates();
                    break;
                case StudyRoomOptionType.Notes:
                    // Notes are handled by UI, no auto generation here
                    break;
            }
        }

        public void GeneratePreviewCandidates()
        {
            EnsureConfig();
            var wordDb = GameManager.Instance?.dataManager?.GetWordDatabase();
            if (wordDb == null)
            {
                OnPreviewCandidatesGenerated?.Invoke(new List<WordData>());
                return;
            }

            var learningManager = GameManager.Instance?.learningManager;
            var learned = learningManager != null
                ? new HashSet<string>(learningManager.wordProgressMap.Keys)
                : new HashSet<string>();

            var candidates = wordDb.words
                .Where(w => !learned.Contains(w.id))
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(Mathf.Max(0, config.previewOptionCount))
                .ToList();

            OnPreviewCandidatesGenerated?.Invoke(candidates);
        }

        public void StartPreview(WordData word)
        {
            EnsureConfig();
            if (word == null) return;

            if (!SpendLearningPoints(config.previewCost))
            {
                OnPreviewCompleted?.Invoke(word.id, false);
                return;
            }

            var card = GameManager.Instance?.dataManager?.GetCard(word.id);
            if (card == null || QuizManager.Instance == null)
            {
                OnPreviewCompleted?.Invoke(word.id, false);
                return;
            }

            QuizManager.Instance.StartQuiz(card, (isCorrect, quality) =>
            {
                var learningManager = GameManager.Instance?.learningManager;
                bool unlocked = false;

                if (learningManager != null)
                {
                    learningManager.OnAnswerResult(isCorrect);
                    if (isCorrect)
                    {
                        unlocked = learningManager.UnlockWord(word.id);
                    }
                }

                if (unlocked)
                {
                    GameManager.Instance?.combatManager?.AddCardToDeck(card);
                }

                OnPreviewCompleted?.Invoke(word.id, unlocked);
            });
        }

        public void GenerateStashCandidates()
        {
            EnsureConfig();
            var combatManager = GameManager.Instance?.combatManager;
            if (combatManager == null)
            {
                OnStashCandidatesGenerated?.Invoke(new List<CardData>());
                return;
            }

            OnStashCandidatesGenerated?.Invoke(combatManager.GetRunDeckCards());
        }

        public void StashCard(CardData card)
        {
            EnsureConfig();
            if (card == null) return;

            if (!SpendLearningPoints(config.stashCost)) return;

            var combatManager = GameManager.Instance?.combatManager;
            if (combatManager == null) return;

            combatManager.RemoveCardFromDeck(card.wordId);
            GameManager.Instance?.AddStashedCard(card.wordId);
            OnCardStashed?.Invoke(card.wordId);
        }

        public void GenerateEvolutionCandidates()
        {
            var combatManager = GameManager.Instance?.combatManager;
            if (combatManager == null)
            {
                OnEvolutionCandidatesGenerated?.Invoke(new List<CardData>());
                return;
            }

            var candidates = combatManager.GetRunDeckCards();
            OnEvolutionCandidatesGenerated?.Invoke(candidates);
        }

        public void RequestEvolution(CardData card)
        {
            EnsureConfig();
            if (card == null) return;
            if (!SpendLearningPoints(config.evolveCost)) return;
            OnEvolutionRequested?.Invoke(card);
        }

        public void ExecuteEvolutionOption(string baseWordId, EvolutionOption option)
        {
            var evolutionManager = GameManager.Instance?.evolutionManager;
            if (evolutionManager == null) return;
            evolutionManager.ExecuteEvolution(baseWordId, option);
        }

        public void GenerateDeepenCandidates()
        {
            var combatManager = GameManager.Instance?.combatManager;
            if (combatManager == null)
            {
                OnDeepenCandidatesGenerated?.Invoke(new List<CardData>());
                return;
            }

            var candidates = combatManager.GetRunDeckCards();
            OnDeepenCandidatesGenerated?.Invoke(candidates);
        }

        public void RequestDeepen(CardData card)
        {
            EnsureConfig();
            if (card == null) return;
            if (!SpendLearningPoints(config.deepenCost)) return;

            var learningManager = GameManager.Instance?.learningManager;
            if (learningManager != null)
            {
                learningManager.MarkWordDeepened(card.wordId);
            }

            OnDeepenRequested?.Invoke(card);
        }

        public void UpdateNote(string wordId, string note)
        {
            EnsureConfig();
            if (string.IsNullOrWhiteSpace(wordId)) return;
            if (!SpendLearningPoints(config.noteCost)) return;

            GameManager.Instance?.SetWordNote(wordId, note);
            OnNoteUpdated?.Invoke(wordId);
        }

        private bool SpendLearningPoints(int amount)
        {
            var gm = GameManager.Instance;
            if (gm == null) return false;
            bool success = gm.SpendLearningPoints(amount);
            if (success)
            {
                OnLearningPointsChanged?.Invoke(gm.GetLearningPoints());
            }
            return success;
        }

        private void EnsureConfig()
        {
            if (config == null)
            {
                config = GameManager.Instance?.dataManager?.GetStudyRoomConfig() ?? new StudyRoomConfig();
            }
        }
    }
}
