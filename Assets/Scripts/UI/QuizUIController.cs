using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VocabCardGame.Core;
using VocabCardGame.Data;
using VocabCardGame.Learning;

namespace VocabCardGame.UI
{
    /// <summary>
    /// 答題 UI 控制器（UGUI 最小可玩版）
    /// </summary>
    public class QuizUIController : MonoBehaviour
    {
        [Header("Root")]
        public CanvasGroup root;

        [Header("Texts")]
        public Text modeText;
        public Text questionText;
        public Text timerText;

        [Header("Audio")]
        public Button audioButton;

        [Header("Options")]
        public GameObject optionsContainer;
        public Button[] optionButtons;
        public Text[] optionTexts;

        [Header("Spelling")]
        public GameObject spellingContainer;
        public InputField spellingInput;
        public Button spellingSubmitButton;

        private QuizManager quizManager;

        private void Start()
        {
            quizManager = QuizManager.Instance;

            if (audioButton != null)
            {
                audioButton.onClick.AddListener(OnAudioClicked);
            }

            if (spellingSubmitButton != null)
            {
                spellingSubmitButton.onClick.AddListener(OnSpellingSubmit);
            }

            Hide();
        }

        private void OnEnable()
        {
            if (QuizManager.Instance == null) return;

            QuizManager.Instance.OnQuizStarted += OnQuizStarted;
            QuizManager.Instance.OnOptionsGenerated += OnOptionsGenerated;
            QuizManager.Instance.OnTimeUpdated += OnTimeUpdated;
            QuizManager.Instance.OnQuizEnded += OnQuizEnded;
        }

        private void OnDisable()
        {
            if (QuizManager.Instance == null) return;

            QuizManager.Instance.OnQuizStarted -= OnQuizStarted;
            QuizManager.Instance.OnOptionsGenerated -= OnOptionsGenerated;
            QuizManager.Instance.OnTimeUpdated -= OnTimeUpdated;
            QuizManager.Instance.OnQuizEnded -= OnQuizEnded;
        }

        private void OnQuizStarted(QuizMode mode, float time)
        {
            Show();
            UpdateQuestion(mode);
            UpdateTimer(time);
        }

        private void OnOptionsGenerated(List<QuizOption> options, int correctIndex)
        {
            if (optionsContainer != null)
            {
                optionsContainer.SetActive(options.Count > 0);
            }

            if (optionButtons == null || optionTexts == null) return;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < options.Count)
                {
                    optionButtons[i].gameObject.SetActive(true);
                    int index = i;
                    if (optionTexts.Length > i && optionTexts[i] != null)
                    {
                        optionTexts[i].text = options[i].text;
                    }

                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => QuizManager.Instance.SubmitAnswer(index));
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnTimeUpdated(float timeRemaining)
        {
            UpdateTimer(timeRemaining);
        }

        private void OnQuizEnded(bool isCorrect)
        {
            Hide();
        }

        private void UpdateQuestion(QuizMode mode)
        {
            var card = QuizManager.Instance.currentCard;
            var word = GameManager.Instance.dataManager.GetWordDatabase().GetWord(card.wordId);

            if (modeText != null)
            {
                modeText.text = mode.ToString();
            }

            bool isSpelling = mode == QuizMode.SpellingEasy || mode == QuizMode.SpellingMedium || mode == QuizMode.SpellingHard;
            bool isListening = mode == QuizMode.ListeningEasy || mode == QuizMode.ListeningMedium || mode == QuizMode.ListeningHard;

            if (questionText != null && word != null)
            {
                if (isSpelling)
                {
                    questionText.text = $"請拼出「{word.chinese}」的英文";
                }
                else if (isListening)
                {
                    questionText.text = "請聽發音，選擇正確單字";
                }
                else
                {
                    questionText.text = $"請選擇「{word.english}」的正確意思";
                }
            }

            if (audioButton != null)
            {
                audioButton.gameObject.SetActive(isListening || isSpelling);
            }

            if (spellingContainer != null)
            {
                spellingContainer.SetActive(isSpelling);
            }

            if (optionsContainer != null)
            {
                optionsContainer.SetActive(!isSpelling);
            }

            if (spellingInput != null)
            {
                spellingInput.text = string.Empty;
            }
        }

        private void UpdateTimer(float timeRemaining)
        {
            if (timerText != null)
            {
                timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}";
            }
        }

        private void OnAudioClicked()
        {
            QuizManager.Instance.PlayWordAudio();
        }

        private void OnSpellingSubmit()
        {
            if (spellingInput == null) return;
            QuizManager.Instance.SubmitSpelling(spellingInput.text);
        }

        private void Show()
        {
            if (root != null)
            {
                root.alpha = 1f;
                root.blocksRaycasts = true;
                root.interactable = true;
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        private void Hide()
        {
            if (root != null)
            {
                root.alpha = 0f;
                root.blocksRaycasts = false;
                root.interactable = false;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
