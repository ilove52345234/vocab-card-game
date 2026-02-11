using System;
using UnityEngine;

namespace VocabCardGame.Data
{
    /// <summary>
    /// 遺物效果設定
    /// </summary>
    [Serializable]
    public class RelicEffectConfig
    {
        public RelicEffectEntry[] effects = Array.Empty<RelicEffectEntry>();
    }

    /// <summary>
    /// 單一遺物效果
    /// </summary>
    [Serializable]
    public class RelicEffectEntry
    {
        public string id;
        public RelicEffectType type;
        public string dimension;
        public int intValue;
        public float floatValue;
        public float lexiconBonus;
    }

    public enum RelicEffectType
    {
        ReturnFirstCard,
        RestSiteOption,
        SurviveOnce,
        PreviewEnemyActions,
        OverblockReflect,
        MistakeConvert,
        QuizTimeBonus,
        StartBlock,
        FirstAttackBonus,
        DimensionBlockBonus,
        DimensionDamageBonus,
        HealOnCorrect,
        FirstDimensionCostReduction,
        DamageOnDiscard,
        BlockPerTurn,
        QuizHintFirstLetter,
        DimensionDrawOncePerTurn,
        RemoveEnemyBlockOnCorrect
    }
}
