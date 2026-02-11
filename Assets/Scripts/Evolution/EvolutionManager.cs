using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VocabCardGame.Combat;
using VocabCardGame.Core;
using VocabCardGame.Data;
using VocabCardGame.Learning;

namespace VocabCardGame.Evolution
{
    /// <summary>
    /// 詞族進化管理器（流程邏輯，UI 由外部接入）
    /// </summary>
    public class EvolutionManager : MonoBehaviour
    {
        [Header("Config")]
        public EvolutionConfig config;

        public event Action<string, List<EvolutionOption>> OnOptionsGenerated;
        public event Action<string, EvolutionOption> OnEvolutionStarted;
        public event Action<string, EvolutionOption, bool> OnEvolutionCompleted;

        private void Start()
        {
            EnsureConfig();
        }

        public List<EvolutionOption> GetOptionsForWord(string wordId)
        {
            EnsureConfig();
            var options = new List<EvolutionOption>();

            if (config != null && config.entries != null)
            {
                var entry = config.entries.FirstOrDefault(e => e.wordId == wordId);
                if (entry != null && entry.options != null)
                {
                    options.AddRange(entry.options.Where(o => o != null));
                }
            }

            if (!options.Any(o => o.type == EvolutionOptionType.Continue))
            {
                options.Add(new EvolutionOption
                {
                    type = EvolutionOptionType.Continue,
                    title = "繼續升級",
                    description = "保持原卡並繼續升級"
                });
            }

            var progress = GameManager.Instance?.learningManager?.GetProgress(wordId);
            if (progress != null && progress.level >= ProficiencyLevel.Proficient && !progress.isDeepened)
            {
                if (!options.Any(o => o.type == EvolutionOptionType.Deepen))
                {
                    options.Add(new EvolutionOption
                    {
                        type = EvolutionOptionType.Deepen,
                        title = "同字深化",
                        description = "解鎖 Lv.8-9 專精上限"
                    });
                }
            }

            OnOptionsGenerated?.Invoke(wordId, options);
            return options;
        }

        public void ExecuteEvolution(string baseWordId, EvolutionOption option)
        {
            EnsureConfig();
            if (option == null || string.IsNullOrWhiteSpace(baseWordId)) return;

            OnEvolutionStarted?.Invoke(baseWordId, option);

            switch (option.type)
            {
                case EvolutionOptionType.Continue:
                    OnEvolutionCompleted?.Invoke(baseWordId, option, true);
                    return;
                case EvolutionOptionType.Deepen:
                    ApplyDeepen(baseWordId, option);
                    return;
                case EvolutionOptionType.Evolve:
                    StartEvolutionQuiz(baseWordId, option);
                    return;
            }
        }

        private void ApplyDeepen(string baseWordId, EvolutionOption option)
        {
            var learningManager = GameManager.Instance?.learningManager;
            if (learningManager == null)
            {
                OnEvolutionCompleted?.Invoke(baseWordId, option, false);
                return;
            }

            learningManager.MarkWordDeepened(baseWordId);
            OnEvolutionCompleted?.Invoke(baseWordId, option, true);
        }

        private void StartEvolutionQuiz(string baseWordId, EvolutionOption option)
        {
            if (string.IsNullOrWhiteSpace(option.targetWordId))
            {
                OnEvolutionCompleted?.Invoke(baseWordId, option, false);
                return;
            }

            var dataManager = GameManager.Instance?.dataManager;
            var combatManager = GameManager.Instance?.combatManager;
            var learningManager = GameManager.Instance?.learningManager;
            if (dataManager == null || combatManager == null || learningManager == null)
            {
                OnEvolutionCompleted?.Invoke(baseWordId, option, false);
                return;
            }

            var targetCard = dataManager.GetCard(option.targetWordId);
            if (targetCard == null || QuizManager.Instance == null)
            {
                OnEvolutionCompleted?.Invoke(baseWordId, option, false);
                return;
            }

            QuizManager.Instance.StartQuiz(targetCard, (isCorrect, quality) =>
            {
                learningManager.OnAnswerResult(isCorrect);

                bool unlocked = false;
                if (isCorrect)
                {
                    unlocked = learningManager.UnlockWordForEvolution(option.targetWordId);
                }

                if (unlocked)
                {
                    combatManager.RemoveCardFromDeck(baseWordId);
                    combatManager.AddCardToDeck(targetCard);
                }

                OnEvolutionCompleted?.Invoke(baseWordId, option, unlocked);
            });
        }

        private void EnsureConfig()
        {
            if (config == null)
            {
                config = GameManager.Instance?.dataManager?.GetEvolutionConfig() ?? new EvolutionConfig();
            }
        }
    }
}
