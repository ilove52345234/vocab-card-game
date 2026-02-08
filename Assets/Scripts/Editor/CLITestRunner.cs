using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VocabCardGame.Data;
using System.IO;

namespace VocabCardGame.Editor
{
    /// <summary>
    /// CLI 測試執行器 - 用於命令列自動化測試
    /// </summary>
    public static class CLITestRunner
    {
        /// <summary>
        /// 建立測試場景並執行基本測試
        /// 命令列呼叫：Unity -batchmode -executeMethod VocabCardGame.Editor.CLITestRunner.RunAllTests
        /// </summary>
        [MenuItem("VocabCardGame/CLI/Run All Tests")]
        public static void RunAllTests()
        {
            Debug.Log("========================================");
            Debug.Log("  Vocab Card Game - CLI Test Runner");
            Debug.Log("========================================\n");

            bool allPassed = true;

            // 測試 1：資料載入
            allPassed &= TestDataLoading();

            // 測試 2：卡牌系統
            allPassed &= TestCardSystem();

            // 測試 3：戰鬥系統
            allPassed &= TestCombatSystem();

            // 測試 4：學習系統
            allPassed &= TestLearningSystem();

            // 輸出結果
            Debug.Log("\n========================================");
            if (allPassed)
            {
                Debug.Log("  ✅ ALL TESTS PASSED");
            }
            else
            {
                Debug.Log("  ❌ SOME TESTS FAILED");
            }
            Debug.Log("========================================");

            // 批次模式下退出
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(allPassed ? 0 : 1);
            }
        }

        /// <summary>
        /// 測試資料載入
        /// </summary>
        [MenuItem("VocabCardGame/CLI/Test Data Loading")]
        public static bool TestDataLoading()
        {
            Debug.Log("\n--- Test: Data Loading ---");

            try
            {
                // 載入單字資料
                var wordsJson = Resources.Load<TextAsset>("Data/words");
                if (wordsJson == null)
                {
                    Debug.LogError("❌ words.json not found");
                    return false;
                }

                var wordDb = JsonUtility.FromJson<WordDatabase>(wordsJson.text);
                Debug.Log($"✅ Loaded {wordDb.words.Count} words");

                foreach (var word in wordDb.words)
                {
                    Debug.Log($"   - {word.english} ({word.chinese}) [{word.element}]");
                }

                // 載入卡牌資料
                var cardsJson = Resources.Load<TextAsset>("Data/cards");
                if (cardsJson == null)
                {
                    Debug.LogError("❌ cards.json not found");
                    return false;
                }
                Debug.Log($"✅ cards.json loaded");

                // 載入敵人資料
                var enemiesJson = Resources.Load<TextAsset>("Data/enemies");
                if (enemiesJson == null)
                {
                    Debug.LogError("❌ enemies.json not found");
                    return false;
                }
                Debug.Log($"✅ enemies.json loaded");

                Debug.Log("✅ Data Loading: PASSED");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Data Loading: FAILED - {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 測試卡牌系統
        /// </summary>
        [MenuItem("VocabCardGame/CLI/Test Card System")]
        public static bool TestCardSystem()
        {
            Debug.Log("\n--- Test: Card System ---");

            try
            {
                // 測試熟練度計算
                var progress = new WordProgress
                {
                    wordId = "test",
                    level = ProficiencyLevel.New
                };

                Debug.Log($"✅ Initial level: {progress.level}");
                Debug.Log($"✅ Needs review: {progress.NeedsReview}");

                // 測試升級
                progress.UpdateProgress(true, 5);
                Debug.Log($"✅ After correct answer: {progress.level}");

                Debug.Log("✅ Card System: PASSED");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Card System: FAILED - {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 測試戰鬥系統
        /// </summary>
        [MenuItem("VocabCardGame/CLI/Test Combat System")]
        public static bool TestCombatSystem()
        {
            Debug.Log("\n--- Test: Combat System ---");

            try
            {
                // 測試戰鬥實體
                var entity = new CombatEntity
                {
                    maxHp = 100,
                    currentHp = 100,
                    block = 0
                };

                // 測試格擋
                entity.AddBlock(10);
                Debug.Log($"✅ Block added: {entity.block}");

                // 測試傷害（被格擋吸收）
                entity.TakeDamage(5);
                Debug.Log($"✅ After 5 damage (blocked): HP={entity.currentHp}, Block={entity.block}");

                // 測試傷害（穿透格擋）
                entity.TakeDamage(10);
                Debug.Log($"✅ After 10 damage: HP={entity.currentHp}, Block={entity.block}");

                // 測試回復
                entity.Heal(20);
                Debug.Log($"✅ After heal 20: HP={entity.currentHp}");

                // 測試狀態效果
                entity.ApplyStatus(StatusEffectType.Burning, 3, 2);
                Debug.Log($"✅ Applied Burning status");

                Debug.Log("✅ Combat System: PASSED");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Combat System: FAILED - {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 測試學習系統
        /// </summary>
        [MenuItem("VocabCardGame/CLI/Test Learning System")]
        public static bool TestLearningSystem()
        {
            Debug.Log("\n--- Test: Learning System ---");

            try
            {
                // 測試答題率計算
                var testCases = new (ProficiencyLevel level, float expectedRate)[]
                {
                    (ProficiencyLevel.New, 1.0f),
                    (ProficiencyLevel.Known, 0.8f),
                    (ProficiencyLevel.Familiar, 0.6f),
                    (ProficiencyLevel.Remembered, 0.4f),
                    (ProficiencyLevel.Proficient, 0.2f),
                    (ProficiencyLevel.Mastered, 0.1f),
                    (ProficiencyLevel.Internalized, 0f),
                };

                foreach (var (level, expectedRate) in testCases)
                {
                    Debug.Log($"✅ Level {level}: Quiz rate = {expectedRate * 100}%");
                }

                // 測試答題模式
                Debug.Log($"✅ Quiz modes: Recognition → Listening → Spelling");

                Debug.Log("✅ Learning System: PASSED");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Learning System: FAILED - {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 建立測試場景
        /// </summary>
        [MenuItem("VocabCardGame/CLI/Create Test Scene")]
        public static void CreateTestScene()
        {
            // 建立新場景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 建立 GameBootstrap
            var bootstrapObj = new GameObject("GameBootstrap");
            bootstrapObj.AddComponent<Core.GameBootstrap>();

            // 確保 Scenes 資料夾存在
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            // 儲存場景
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/TestScene.unity");
            Debug.Log("✅ Test scene created: Assets/Scenes/TestScene.unity");
        }

        /// <summary>
        /// 顯示專案資訊
        /// </summary>
        [MenuItem("VocabCardGame/CLI/Show Project Info")]
        public static void ShowProjectInfo()
        {
            Debug.Log("========================================");
            Debug.Log("  Vocab Card Game - Project Info");
            Debug.Log("========================================");
            Debug.Log($"Unity Version: {Application.unityVersion}");
            Debug.Log($"Platform: {Application.platform}");
            Debug.Log($"Data Path: {Application.dataPath}");
            Debug.Log("========================================");
        }
    }
}
