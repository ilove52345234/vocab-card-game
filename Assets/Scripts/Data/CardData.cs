using System;
using System.Collections.Generic;
using UnityEngine;

namespace VocabCardGame.Data
{
    /// <summary>
    /// 卡牌效果定義
    /// </summary>
    [Serializable]
    public class CardEffect
    {
        public CardEffectType type;
        public int value;
        public int duration;            // 持續回合數（狀態效果用）
        public string targetKeyword;    // 觸發條件的關鍵字
        public string description;      // 效果描述
    }

    public enum CardEffectType
    {
        // 基礎效果
        Damage,             // 造成傷害
        Block,              // 獲得格擋
        Heal,               // 回復生命
        DrawCard,           // 抽牌
        GainEnergy,         // 獲得能量

        // 狀態效果
        ApplyBurning,       // 施加燃燒
        ApplyFrozen,        // 施加冰凍
        ApplyPoison,        // 施加中毒
        ApplyWet,           // 施加潮濕
        ApplyBleeding,      // 施加流血
        ApplyStrength,      // 施加力量
        ApplyDexterity,     // 施加敏捷

        // 特殊效果
        DamageAll,          // 對全體敵人傷害
        ChainTrigger,       // 連鎖觸發
        EquipPermanent,     // 裝備（永久生效）
        DiscardDraw,        // 棄牌抽牌
        ReduceCost,         // 降低費用
        ReturnToHand,       // 返回手牌
        Exhaust,            // 消耗（本場不再使用）

        // 協同效果
        TribeSynergy,       // 種族協同加成
        ElementSynergy,     // 元素協同加成
        ComboTrigger        // Combo 觸發
    }

    /// <summary>
    /// 卡牌資料（結合單字與戰鬥效果）
    /// </summary>
    [Serializable]
    public class CardData
    {
        public string wordId;           // 對應的單字 ID
        public CardType cardType;       // 卡牌類型
        public int energyCost;          // 能量消耗
        public List<CardEffect> effects = new List<CardEffect>(); // 效果列表
        public Dimension dimension;      // 卡牌維度（Strike/Guard/Boost/Warp）
        public string[] produces;       // 產出的資源媒介標籤
        public string[] consumes;       // 消費/受益的資源媒介標籤
        public string deviation;        // 偏移方法：standard, positive, negative, condition
        public string balanceNote;      // 平衡備註（設計用，運行時不用）

        // 運行時數據（不序列化）
        [NonSerialized] public WordData wordData;
        [NonSerialized] public WordProgress progress;

        /// <summary>
        /// 取得效果倍率（基於熟練度）
        /// </summary>
        public float GetEffectMultiplier()
        {
            if (progress == null) return 1f;

            // 標準值平衡體系：Lv.1-2=×1.0, Lv.3-4=×1.1, Lv.5-6=×1.2, Lv.7=×1.3
            return progress.level switch
            {
                ProficiencyLevel.New => 1.0f,
                ProficiencyLevel.Known => 1.0f,
                ProficiencyLevel.Familiar => 1.1f,
                ProficiencyLevel.Remembered => 1.1f,
                ProficiencyLevel.Proficient => 1.2f,
                ProficiencyLevel.Mastered => 1.2f,
                ProficiencyLevel.Internalized => 1.3f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 取得答題機率
        /// </summary>
        public float GetQuizChance()
        {
            if (progress == null) return 1f;

            return progress.level switch
            {
                ProficiencyLevel.New => 1.0f,
                ProficiencyLevel.Known => 0.8f,
                ProficiencyLevel.Familiar => 0.6f,
                ProficiencyLevel.Remembered => 0.4f,
                ProficiencyLevel.Proficient => 0.2f,
                ProficiencyLevel.Mastered => 0.1f,
                ProficiencyLevel.Internalized => 0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 取得對應的答題模式
        /// </summary>
        public QuizMode GetQuizMode()
        {
            if (progress == null) return QuizMode.RecognitionEasy;

            return progress.level switch
            {
                ProficiencyLevel.New => QuizMode.RecognitionEasy,
                ProficiencyLevel.Known => QuizMode.RecognitionMedium,
                ProficiencyLevel.Familiar => UnityEngine.Random.value > 0.2f
                    ? QuizMode.RecognitionMedium
                    : QuizMode.ListeningEasy,
                ProficiencyLevel.Remembered => UnityEngine.Random.value > 0.5f
                    ? QuizMode.ListeningEasy
                    : QuizMode.ListeningMedium,
                ProficiencyLevel.Proficient => UnityEngine.Random.value > 0.1f
                    ? QuizMode.ListeningMedium
                    : QuizMode.SpellingEasy,
                ProficiencyLevel.Mastered => UnityEngine.Random.value > 0.3f
                    ? QuizMode.SpellingEasy
                    : QuizMode.SpellingMedium,
                _ => QuizMode.RecognitionEasy
            };
        }

        /// <summary>
        /// 是否需要答題
        /// </summary>
        public bool RequiresQuiz()
        {
            return UnityEngine.Random.value <= GetQuizChance();
        }
    }

    /// <summary>
    /// Combo 定義
    /// </summary>
    [Serializable]
    public class ComboData
    {
        public string id;
        public string name;             // Combo 名稱
        public string[] requiredCards;  // 需要的卡牌 ID（按順序）
        public List<CardEffect> bonusEffects; // 額外效果
        public string description;
        public bool isDiscovered;       // 是否已發現
    }

    /// <summary>
    /// 遺物資料（三層構詞學：字首/字尾/字根）
    /// </summary>
    [Serializable]
    public class RelicData
    {
        public string id;
        public string name;
        public string description;
        public RelicMorphType type;      // 構詞類型：prefix/suffix/root
        public string affix;             // 字首/字尾/字根文字
        public string affixMeaning;      // 語意
        public string[] exampleWords;    // 相關單字範例
        public Rarity rarity;
        public string category;          // trigger/amplifier/foundation
        public Dimension? dimension;     // 關聯維度（部分遺物有）
        public string lexiconBonus;      // 詞庫連動描述（字根遺物用）
    }
}
