using System;
using System.Linq;
using UnityEngine;
using VocabCardGame.Combat;
using VocabCardGame.Data;
using VocabCardGame.Learning;

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
        public VocabCardGame.Map.MapManager mapManager;

        [Header("Current State")]
        public PlayerData playerData;
        public GameMode currentMode = GameMode.Adventure;
        public int currentDifficulty = 0;
        public int currentFloor = 1;

        // 事件
        public event Action OnGameStateChanged;
        public event Action<int> OnPlayerLevelUp;
        public event Action<string> OnAchievementUnlocked;

        private bool isInitialized = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // 確保引用存在（避免 AddComponent 順序導致為 null）
                if (dataManager == null) dataManager = GetComponent<DataManager>();
                if (combatManager == null) combatManager = GetComponent<CombatManager>();
                if (learningManager == null) learningManager = GetComponent<LearningManager>();
                if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
                if (mapManager == null) mapManager = FindObjectOfType<VocabCardGame.Map.MapManager>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (dataManager == null)
            {
                Debug.LogError("[GameManager] DataManager is missing. Initialization aborted.");
                return;
            }

            // 載入或創建玩家資料
            playerData = dataManager.LoadPlayerData() ?? new PlayerData
            {
                firstPlayDate = DateTime.Now
            };

            // 載入單字學習進度
            if (learningManager != null)
            {
                var progress = dataManager.LoadWordProgress();
                learningManager.LoadProgress(progress);
            }

            // 更新遊玩天數
            UpdatePlayDays();

            isInitialized = true;
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
            mapManager?.GenerateNewMap();
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
            mapManager?.GenerateNewMap();
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
            mapManager?.GenerateNewMap();
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
        /// 取得目前啟用中的遺物列表
        /// </summary>
        public IReadOnlyList<string> GetActiveRelics()
        {
            if (playerData == null) return Array.Empty<string>();
            if (playerData.equippedRelics != null && playerData.equippedRelics.Count > 0)
            {
                return playerData.equippedRelics;
            }
            return playerData.ownedRelics ?? Array.Empty<string>();
        }

        /// <summary>
        /// 是否持有指定遺物
        /// </summary>
        public bool HasRelic(string relicId)
        {
            return GetActiveRelics().Contains(relicId);
        }

        /// <summary>
        /// 取得遺物效果設定
        /// </summary>
        public RelicEffectEntry GetRelicEffect(string relicId)
        {
            return dataManager?.GetRelicEffect(relicId);
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

            // 遺物加成：-tion 之印
            if (HasRelic("relic_tion"))
            {
                var effect = GetRelicEffect("relic_tion");
                if (effect != null && effect.type == RelicEffectType.QuizTimeBonus)
                {
                    baseTime += effect.intValue;
                }
            }

            return baseTime;
        }

        private void OnApplicationQuit()
        {
            dataManager.SavePlayerData(playerData);
        }
    }
}
