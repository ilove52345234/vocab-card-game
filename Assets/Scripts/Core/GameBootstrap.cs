using UnityEngine;
using VocabCardGame.Combat;
using VocabCardGame.Learning;

namespace VocabCardGame.Core
{
    /// <summary>
    /// 遊戲啟動器 - 在場景中創建所有必要的管理器
    /// 將此腳本掛載到場景中的空物件上
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Auto Create Managers")]
        public bool autoCreateManagers = true;

        private void Awake()
        {
            if (autoCreateManagers)
            {
                CreateManagers();
            }
        }

        private void CreateManagers()
        {
            // 創建 GameManager
            if (GameManager.Instance == null)
            {
                var gmObj = new GameObject("GameManager");
                var gm = gmObj.AddComponent<GameManager>();

                // 添加子管理器
                gm.dataManager = gmObj.AddComponent<DataManager>();
                gm.combatManager = gmObj.AddComponent<CombatManager>();
                gm.learningManager = gmObj.AddComponent<LearningManager>();
                gm.audioManager = CreateAudioManager();

                DontDestroyOnLoad(gmObj);
                Debug.Log("[GameBootstrap] GameManager created successfully");
            }

            // 創建 QuizManager
            if (QuizManager.Instance == null)
            {
                var qmObj = new GameObject("QuizManager");
                qmObj.AddComponent<QuizManager>();
                DontDestroyOnLoad(qmObj);
                Debug.Log("[GameBootstrap] QuizManager created successfully");
            }
        }

        private AudioManager CreateAudioManager()
        {
            var audioObj = new GameObject("AudioManager");
            audioObj.transform.SetParent(transform);

            var am = audioObj.AddComponent<AudioManager>();

            // 創建 AudioSource
            am.musicSource = CreateAudioSource(audioObj, "MusicSource");
            am.sfxSource = CreateAudioSource(audioObj, "SFXSource");
            am.voiceSource = CreateAudioSource(audioObj, "VoiceSource");

            return am;
        }

        private AudioSource CreateAudioSource(GameObject parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform);
            return obj.AddComponent<AudioSource>();
        }

        /// <summary>
        /// 測試用：開始一場戰鬥
        /// </summary>
        [ContextMenu("Test Start Combat")]
        public void TestStartCombat()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not initialized!");
                return;
            }

            var enemies = GameManager.Instance.dataManager.GetEnemiesForFloor(1, Data.GameMode.Adventure);
            GameManager.Instance.combatManager.StartCombat(enemies);
            Debug.Log("[Test] Combat started with " + enemies.Count + " enemies");
        }

        /// <summary>
        /// 測試用：顯示資料庫狀態
        /// </summary>
        [ContextMenu("Test Show Database Info")]
        public void TestShowDatabaseInfo()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not initialized!");
                return;
            }

            var wordDb = GameManager.Instance.dataManager.GetWordDatabase();
            Debug.Log($"[Test] Word Database: {wordDb.words.Count} words loaded");

            foreach (var word in wordDb.words)
            {
                Debug.Log($"  - {word.english} ({word.chinese}) [{word.element}]");
            }
        }
    }
}
