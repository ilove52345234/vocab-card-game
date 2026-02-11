using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VocabCardGame.Data;

namespace VocabCardGame.Core
{
    /// <summary>
    /// 資料管理器
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        [Header("Database")]
        private WordDatabase wordDatabase;
        private List<CardData> cardDatabase;
        private List<EnemyData> enemyDatabase;
        private List<ComboData> comboDatabase;
        private List<RelicData> relicDatabase;
        private SynergyConfig synergyConfig;
        private RelicEffectConfig relicEffectConfig;
        private VocabCardGame.Map.MapConfig mapConfig;

        private string SavePath => Path.Combine(Application.persistentDataPath, "save");

        private void Awake()
        {
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }

            LoadAllData();
        }

        /// <summary>
        /// 載入所有資料
        /// </summary>
        private void LoadAllData()
        {
            LoadWordDatabase();
            LoadCardDatabase();
            LoadEnemyDatabase();
            LoadComboDatabase();
            LoadRelicDatabase();
            LoadSynergyConfig();
            LoadRelicEffectConfig();
            LoadMapConfig();
        }

        #region Word Database

        private void LoadWordDatabase()
        {
            var json = Resources.Load<TextAsset>("Data/words");
            if (json != null)
            {
                wordDatabase = JsonUtility.FromJson<WordDatabase>(json.text);
                wordDatabase.BuildLookup();
            }
            else
            {
                wordDatabase = new WordDatabase();
                Debug.LogWarning("Word database not found, using empty database");
            }
        }

        public WordDatabase GetWordDatabase()
        {
            return wordDatabase;
        }

        #endregion

        #region Map Config

        private void LoadMapConfig()
        {
            var json = Resources.Load<TextAsset>("Data/map_config");
            if (json != null)
            {
                mapConfig = JsonUtility.FromJson<VocabCardGame.Map.MapConfig>(json.text);
            }
            else
            {
                mapConfig = new VocabCardGame.Map.MapConfig();
                Debug.LogWarning("Map config not found, using defaults");
            }
        }

        public VocabCardGame.Map.MapConfig GetMapConfig()
        {
            return mapConfig ?? new VocabCardGame.Map.MapConfig();
        }

        #endregion

        #region Relic Effect Config

        private void LoadRelicEffectConfig()
        {
            var json = Resources.Load<TextAsset>("Data/relic_effects");
            if (json != null)
            {
                relicEffectConfig = JsonUtility.FromJson<RelicEffectConfig>(json.text);
            }
            else
            {
                relicEffectConfig = new RelicEffectConfig();
                Debug.LogWarning("Relic effect config not found, using defaults");
            }
        }

        public RelicEffectEntry GetRelicEffect(string relicId)
        {
            if (relicEffectConfig == null || relicEffectConfig.effects == null) return null;
            foreach (var effect in relicEffectConfig.effects)
            {
                if (effect.id == relicId) return effect;
            }
            return null;
        }

        #endregion

        #region Synergy Config

        private void LoadSynergyConfig()
        {
            var json = Resources.Load<TextAsset>("Data/synergy_config");
            if (json != null)
            {
                synergyConfig = JsonUtility.FromJson<SynergyConfig>(json.text);
            }
            else
            {
                synergyConfig = new SynergyConfig();
                Debug.LogWarning("Synergy config not found, using defaults");
            }
        }

        public SynergyConfig GetSynergyConfig()
        {
            return synergyConfig ?? new SynergyConfig();
        }

        #endregion

        #region Card Database

        private void LoadCardDatabase()
        {
            var json = Resources.Load<TextAsset>("Data/cards");
            if (json != null)
            {
                var wrapper = JsonUtility.FromJson<CardDatabaseWrapper>(json.text);
                cardDatabase = wrapper.cards;

                // 連結 WordData
                foreach (var card in cardDatabase)
                {
                    card.wordData = wordDatabase.GetWord(card.wordId);
                }
            }
            else
            {
                cardDatabase = new List<CardData>();
                Debug.LogWarning("Card database not found, using empty database");
            }
        }

        public CardData GetCard(string wordId)
        {
            return cardDatabase.Find(c => c.wordId == wordId);
        }

        public List<CardData> GetAllCards()
        {
            return cardDatabase ?? new List<CardData>();
        }

        public List<CardData> GetCardsByElement(Element element)
        {
            return cardDatabase.FindAll(c => c.wordData?.element == element);
        }

        public List<CardData> GetCardsByDimension(Dimension dimension)
        {
            return cardDatabase.FindAll(c => c.dimension == dimension);
        }

        #endregion

        #region Enemy Database

        private void LoadEnemyDatabase()
        {
            var json = Resources.Load<TextAsset>("Data/enemies");
            if (json != null)
            {
                var wrapper = JsonUtility.FromJson<EnemyDatabaseWrapper>(json.text);
                enemyDatabase = wrapper.enemies;
            }
            else
            {
                enemyDatabase = new List<EnemyData>();
                Debug.LogWarning("Enemy database not found, using empty database");
            }
        }

        public EnemyData GetEnemy(string id)
        {
            return enemyDatabase.Find(e => e.id == id);
        }

        public List<EnemyData> GetEnemiesByCategory(EnemyCategory category)
        {
            return enemyDatabase.FindAll(e => e.category == category);
        }

        public List<EnemyData> GetEnemiesForFloor(int floor, GameMode mode)
        {
            // TODO: 根據層數和模式選擇合適的敵人
            return enemyDatabase.Take(2).ToList();
        }

        #endregion

        #region Combo Database

        private void LoadComboDatabase()
        {
            var json = Resources.Load<TextAsset>("Data/combos");
            if (json != null)
            {
                var wrapper = JsonUtility.FromJson<ComboDatabaseWrapper>(json.text);
                comboDatabase = wrapper.combos;
            }
            else
            {
                comboDatabase = new List<ComboData>();
            }
        }

        public ComboData CheckCombo(List<string> playedCards)
        {
            foreach (var combo in comboDatabase)
            {
                if (IsComboMatch(playedCards, combo.requiredCards))
                {
                    return combo;
                }
            }
            return null;
        }

        private bool IsComboMatch(List<string> played, string[] required)
        {
            if (played.Count < required.Length) return false;

            // 檢查最後 N 張牌是否符合
            int startIndex = played.Count - required.Length;
            for (int i = 0; i < required.Length; i++)
            {
                if (played[startIndex + i] != required[i])
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Relic Database

        private void LoadRelicDatabase()
        {
            var json = Resources.Load<TextAsset>("Data/relics");
            if (json != null)
            {
                var wrapper = JsonUtility.FromJson<RelicDatabaseWrapper>(json.text);
                relicDatabase = wrapper.relics;
            }
            else
            {
                relicDatabase = new List<RelicData>();
            }
        }

        public RelicData GetRelic(string id)
        {
            return relicDatabase.Find(r => r.id == id);
        }

        #endregion

        #region Player Save Data

        public void SavePlayerData(PlayerData data)
        {
            string path = Path.Combine(SavePath, "player.json");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        public PlayerData LoadPlayerData()
        {
            string path = Path.Combine(SavePath, "player.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<PlayerData>(json);
            }
            return null;
        }

        #endregion

        #region Word Progress Save Data

        public void SaveWordProgress(Dictionary<string, WordProgress> progressMap)
        {
            string path = Path.Combine(SavePath, "word_progress.json");
            var wrapper = new WordProgressWrapper
            {
                items = new List<WordProgress>(progressMap.Values)
            };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(path, json);
        }

        public Dictionary<string, WordProgress> LoadWordProgress()
        {
            string path = Path.Combine(SavePath, "word_progress.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var wrapper = JsonUtility.FromJson<WordProgressWrapper>(json);
                var dict = new Dictionary<string, WordProgress>();
                foreach (var item in wrapper.items)
                {
                    dict[item.wordId] = item;
                }
                return dict;
            }
            return new Dictionary<string, WordProgress>();
        }

        #endregion
    }

    // JSON 包裝類別（Unity JsonUtility 需要）
    [Serializable]
    public class CardDatabaseWrapper
    {
        public List<CardData> cards;
    }

    [Serializable]
    public class EnemyDatabaseWrapper
    {
        public List<EnemyData> enemies;
    }

    [Serializable]
    public class ComboDatabaseWrapper
    {
        public List<ComboData> combos;
    }

    [Serializable]
    public class RelicDatabaseWrapper
    {
        public List<RelicData> relics;
    }

    [Serializable]
    public class WordProgressWrapper
    {
        public List<WordProgress> items;
    }
}
