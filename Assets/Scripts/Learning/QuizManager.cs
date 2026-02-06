using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VocabCardGame.Data;
using VocabCardGame.Core;

namespace VocabCardGame.Learning
{
    /// <summary>
    /// 答題管理器
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        [Header("Current Quiz")]
        public CardData currentCard;
        public QuizMode currentMode;
        public float timeRemaining;
        public bool isQuizActive;

        [Header("Quiz Options")]
        public List<QuizOption> currentOptions = new List<QuizOption>();
        public int correctOptionIndex;

        // 回呼
        private Action<bool, int> onQuizComplete;

        // 事件
        public event Action<QuizMode, float> OnQuizStarted;
        public event Action<List<QuizOption>, int> OnOptionsGenerated;
        public event Action<float> OnTimeUpdated;
        public event Action<bool> OnQuizEnded;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (isQuizActive)
            {
                timeRemaining -= Time.deltaTime;
                OnTimeUpdated?.Invoke(timeRemaining);

                if (timeRemaining <= 0)
                {
                    // 時間到，視為答錯
                    SubmitAnswer(-1);
                }
            }
        }

        /// <summary>
        /// 開始答題
        /// </summary>
        public void StartQuiz(CardData card, Action<bool, int> callback)
        {
            currentCard = card;
            onQuizComplete = callback;
            currentMode = card.GetQuizMode();

            // 設定答題時間
            timeRemaining = GameManager.Instance.GetQuizTime(currentMode);

            // 生成選項
            GenerateOptions();

            isQuizActive = true;
            OnQuizStarted?.Invoke(currentMode, timeRemaining);
        }

        /// <summary>
        /// 生成答題選項
        /// </summary>
        private void GenerateOptions()
        {
            currentOptions.Clear();
            var wordDb = GameManager.Instance.dataManager.GetWordDatabase();
            var targetWord = wordDb.GetWord(currentCard.wordId);

            if (targetWord == null)
            {
                Debug.LogError($"Word not found: {currentCard.wordId}");
                return;
            }

            switch (currentMode)
            {
                case QuizMode.RecognitionEasy:
                    GenerateRecognitionOptions(targetWord, OptionDifficulty.Easy);
                    break;

                case QuizMode.RecognitionMedium:
                    GenerateRecognitionOptions(targetWord, OptionDifficulty.Medium);
                    break;

                case QuizMode.RecognitionHard:
                    GenerateRecognitionOptions(targetWord, OptionDifficulty.Hard);
                    break;

                case QuizMode.ListeningEasy:
                case QuizMode.ListeningMedium:
                case QuizMode.ListeningHard:
                    GenerateListeningOptions(targetWord);
                    break;

                case QuizMode.SpellingEasy:
                case QuizMode.SpellingMedium:
                case QuizMode.SpellingHard:
                    // 拼字模式不需要選項
                    correctOptionIndex = -1;
                    break;
            }

            OnOptionsGenerated?.Invoke(currentOptions, correctOptionIndex);
        }

        private void GenerateRecognitionOptions(WordData targetWord, OptionDifficulty difficulty)
        {
            var wordDb = GameManager.Instance.dataManager.GetWordDatabase();
            var allWords = wordDb.words;

            // 正確答案
            currentOptions.Add(new QuizOption
            {
                text = targetWord.chinese,
                isCorrect = true
            });

            // 錯誤選項
            List<WordData> distractors;

            switch (difficulty)
            {
                case OptionDifficulty.Easy:
                    // 簡單：不同詞性/類別
                    distractors = allWords
                        .Where(w => w.id != targetWord.id && w.element != targetWord.element)
                        .OrderBy(_ => UnityEngine.Random.value)
                        .Take(3)
                        .ToList();
                    break;

                case OptionDifficulty.Medium:
                    // 中等：同類別
                    distractors = allWords
                        .Where(w => w.id != targetWord.id && w.tribe == targetWord.tribe)
                        .OrderBy(_ => UnityEngine.Random.value)
                        .Take(3)
                        .ToList();

                    // 不夠則補其他
                    if (distractors.Count < 3)
                    {
                        var additional = allWords
                            .Where(w => w.id != targetWord.id && !distractors.Contains(w))
                            .OrderBy(_ => UnityEngine.Random.value)
                            .Take(3 - distractors.Count);
                        distractors.AddRange(additional);
                    }
                    break;

                case OptionDifficulty.Hard:
                    // 困難：易混淆詞
                    if (targetWord.confusables != null && targetWord.confusables.Length > 0)
                    {
                        distractors = targetWord.confusables
                            .Select(id => wordDb.GetWord(id))
                            .Where(w => w != null)
                            .Take(3)
                            .ToList();
                    }
                    else
                    {
                        // 沒有定義易混淆詞，用同元素
                        distractors = allWords
                            .Where(w => w.id != targetWord.id && w.element == targetWord.element)
                            .OrderBy(_ => UnityEngine.Random.value)
                            .Take(3)
                            .ToList();
                    }
                    break;

                default:
                    distractors = allWords
                        .Where(w => w.id != targetWord.id)
                        .OrderBy(_ => UnityEngine.Random.value)
                        .Take(3)
                        .ToList();
                    break;
            }

            foreach (var word in distractors)
            {
                currentOptions.Add(new QuizOption
                {
                    text = word.chinese,
                    isCorrect = false
                });
            }

            // 隨機排序
            currentOptions = currentOptions.OrderBy(_ => UnityEngine.Random.value).ToList();
            correctOptionIndex = currentOptions.FindIndex(o => o.isCorrect);
        }

        private void GenerateListeningOptions(WordData targetWord)
        {
            var wordDb = GameManager.Instance.dataManager.GetWordDatabase();

            // 正確答案
            currentOptions.Add(new QuizOption
            {
                text = targetWord.english,
                isCorrect = true
            });

            // 發音相似的詞（如果有定義）
            List<WordData> distractors;
            if (targetWord.confusables != null && targetWord.confusables.Length > 0)
            {
                distractors = targetWord.confusables
                    .Select(id => wordDb.GetWord(id))
                    .Where(w => w != null)
                    .Take(3)
                    .ToList();
            }
            else
            {
                // 選擇長度相近的詞
                int targetLength = targetWord.english.Length;
                distractors = wordDb.words
                    .Where(w => w.id != targetWord.id &&
                               Math.Abs(w.english.Length - targetLength) <= 2)
                    .OrderBy(_ => UnityEngine.Random.value)
                    .Take(3)
                    .ToList();
            }

            foreach (var word in distractors)
            {
                currentOptions.Add(new QuizOption
                {
                    text = word.english,
                    isCorrect = false
                });
            }

            currentOptions = currentOptions.OrderBy(_ => UnityEngine.Random.value).ToList();
            correctOptionIndex = currentOptions.FindIndex(o => o.isCorrect);
        }

        /// <summary>
        /// 提交答案（選擇題）
        /// </summary>
        public void SubmitAnswer(int selectedIndex)
        {
            if (!isQuizActive) return;
            isQuizActive = false;

            bool isCorrect = selectedIndex == correctOptionIndex;
            int quality = CalculateQuality(isCorrect, timeRemaining);

            OnQuizEnded?.Invoke(isCorrect);
            onQuizComplete?.Invoke(isCorrect, quality);
        }

        /// <summary>
        /// 提交答案（拼字）
        /// </summary>
        public void SubmitSpelling(string input)
        {
            if (!isQuizActive) return;
            isQuizActive = false;

            var wordDb = GameManager.Instance.dataManager.GetWordDatabase();
            var targetWord = wordDb.GetWord(currentCard.wordId);

            bool isCorrect = input.Trim().ToLower() == targetWord.english.ToLower();
            int quality = CalculateQuality(isCorrect, timeRemaining);

            OnQuizEnded?.Invoke(isCorrect);
            onQuizComplete?.Invoke(isCorrect, quality);
        }

        /// <summary>
        /// 計算答題品質（SM-2 用）
        /// </summary>
        private int CalculateQuality(bool isCorrect, float remainingTime)
        {
            if (!isCorrect) return 2; // 答錯

            float totalTime = GameManager.Instance.GetQuizTime(currentMode);
            float timeRatio = remainingTime / totalTime;

            // 根據剩餘時間比例給分
            if (timeRatio > 0.7f) return 5;      // 快速答對
            if (timeRatio > 0.4f) return 4;      // 正常答對
            return 3;                             // 勉強答對
        }

        /// <summary>
        /// 播放單字發音
        /// </summary>
        public void PlayWordAudio()
        {
            if (currentCard == null) return;

            var wordDb = GameManager.Instance.dataManager.GetWordDatabase();
            var word = wordDb.GetWord(currentCard.wordId);

            if (word != null && !string.IsNullOrEmpty(word.audioPath))
            {
                GameManager.Instance.audioManager.PlayWordAudio(word.audioPath);
            }
        }
    }

    /// <summary>
    /// 答題選項
    /// </summary>
    [Serializable]
    public class QuizOption
    {
        public string text;
        public bool isCorrect;
    }

    public enum OptionDifficulty
    {
        Easy,
        Medium,
        Hard
    }
}
