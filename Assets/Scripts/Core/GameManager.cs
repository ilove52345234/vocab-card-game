using System;
using UnityEngine;
using VocabCardGame.Data;

namespace VocabCardGame.Core
{
    /// <summary>
    /// 遊戲主管理器（單例）
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        public DataManager dataManager;
        public CombatManager combatManager;
        public LearningManager learningManager;
        public AudioManager audioManager;

        [Header("Current State")]
        public PlayerData playerData;
        public GameMode currentMode = GameMode.Adventure;
        public int currentDifficulty = 0;
        public int currentFloor = 1;

        // 事件
        public event Action OnGameStateChanged;
        public event Action<int> OnPlayerLevelUp;
        public event Action<string> OnAchievementUnlocked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            // 載入或創建玩家資料
            playerData = dataManager.LoadPlayerData() ?? new PlayerData
            {
                firstPlayDate = DateTime.Now
            };

            // 更新遊玩天數
            UpdatePlayDays();
        }

        private void UpdatePlayDays()
        {
            var daysSinceStart = (DateTime.Now - playerData.firstPlayDate).Days;
            if (daysSinceStart > playerData.totalPlayDays)
            {
                playerData.totalPlayDays = daysSinceStart;
                dataManager.SavePlayerData(playerData);
            }
        }

        /// <summary>
        /// 開始冒險模式
        /// </summary>
        public void StartAdventure(int difficulty)
        {
            currentMode = GameMode.Adventure;
            currentDifficulty = difficulty;
            currentFloor = 1;
            combatManager.InitializeRun();
            OnGameStateChanged?.Invoke();
        }

        /// <summary>
        /// 開始無盡深淵
        /// </summary>
        public void StartEndlessAbyss()
        {
            currentMode = GameMode.EndlessAbyss;
            currentFloor = 1;
            combatManager.InitializeRun();
            OnGameStateChanged?.Invoke();
        }

        /// <summary>
        /// 開始每日挑戰
        /// </summary>
        public void StartDailyChallenge()
        {
            currentMode = GameMode.DailyChallenge;
            currentFloor = 1;
            combatManager.InitializeRun();
            OnGameStateChanged?.Invoke();
        }

        /// <summary>
        /// 增加經驗值
        /// </summary>
        public void AddExperience(int amount)
        {
            if (playerData.AddExperience(amount))
            {
                OnPlayerLevelUp?.Invoke(playerData.level);
            }
            dataManager.SavePlayerData(playerData);
        }

        /// <summary>
        /// 增加金幣
        /// </summary>
        public void AddGold(int amount)
        {
            playerData.gold += amount;
            dataManager.SavePlayerData(playerData);
        }

        /// <summary>
        /// 解鎖成就
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!playerData.unlockedAchievements.Contains(achievementId))
            {
                playerData.unlockedAchievements.Add(achievementId);
                dataManager.SavePlayerData(playerData);
                OnAchievementUnlocked?.Invoke(achievementId);
            }
        }

        /// <summary>
        /// 更新深淵紀錄
        /// </summary>
        public void UpdateAbyssRecord(int floor)
        {
            if (floor > playerData.highestAbyssFloor)
            {
                playerData.highestAbyssFloor = floor;
                dataManager.SavePlayerData(playerData);

                // 檢查成就
                CheckAbyssAchievements(floor);
            }
        }

        private void CheckAbyssAchievements(int floor)
        {
            if (floor >= 50) UnlockAchievement("abyss_explorer");
            if (floor >= 100) UnlockAchievement("abyss_conqueror");
            if (floor >= 200) UnlockAchievement("abyss_master");
        }

        /// <summary>
        /// 取得答題時間（含加成）
        /// </summary>
        public float GetQuizTime(QuizMode mode)
        {
            float baseTime = mode switch
            {
                QuizMode.RecognitionEasy => 5f,
                QuizMode.RecognitionMedium => 5f,
                QuizMode.RecognitionHard => 6f,
                QuizMode.ListeningEasy => 6f,
                QuizMode.ListeningMedium => 6f,
                QuizMode.ListeningHard => 7f,
                QuizMode.SpellingEasy => 8f,
                QuizMode.SpellingMedium => 10f,
                QuizMode.SpellingHard => 12f,
                _ => 5f
            };

            // 新手期加成
            if (playerData.GetGamePhase() == GamePhase.Tutorial)
            {
                baseTime *= 2f; // 無時間限制（加倍）
            }
            else if (playerData.GetGamePhase() == GamePhase.Beginner)
            {
                baseTime *= 1.5f; // 寬鬆時間
            }

            // 智力加成
            baseTime += playerData.QuizTimeBonus;

            return baseTime;
        }

        private void OnApplicationQuit()
        {
            dataManager.SavePlayerData(playerData);
        }
    }
}
