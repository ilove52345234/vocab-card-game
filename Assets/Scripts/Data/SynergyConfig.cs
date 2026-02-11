using System;
using UnityEngine;

namespace VocabCardGame.Data
{
    /// <summary>
    /// 協同系統設定
    /// </summary>
    [Serializable]
    public class SynergyConfig
    {
        public ResourceMediatorConfig resourceMediator;
        public DimensionChainConfig dimensionChain;
        public ElementResonanceConfig elementResonance;
        public KnowledgeResonanceConfig knowledgeResonance;
    }

    /// <summary>
    /// 資源媒介協同設定（produces/consumes）
    /// </summary>
    [Serializable]
    public class ResourceMediatorConfig
    {
        public float bonusPerToken = 0.1f;
        public int maxTokensPerTag = 2;
        public float maxTotalBonus = 0.3f;
        public string[] applyToEffects = Array.Empty<string>();
    }

    /// <summary>
    /// 維度連鎖協同設定
    /// </summary>
    [Serializable]
    public class DimensionChainConfig
    {
        public float secondCardBonus = 0.2f;
        public float thirdCardBonus = 0.4f;
        public int coverageDrawAt = 3;
        public int coverageDrawAndEnergyAt = 4;
        public string[] applyToEffects = Array.Empty<string>();
    }

    /// <summary>
    /// 元素共鳴協同設定
    /// </summary>
    [Serializable]
    public class ElementResonanceConfig
    {
        public int lifeHeal2 = 2;
        public int lifeHeal3 = 4;
        public int forceAttackBonus2 = 2;
        public int forceAttackBonus3 = 4;
        public int mindDraw2 = 1;
        public int mindDraw3 = 2;
        public int matterBlock2 = 3;
        public int matterBlock3 = 6;
        public int abstractCostReduction2 = 1;
        public int abstractCostReduction3 = 2;
    }

    /// <summary>
    /// 知識共振協同設定
    /// </summary>
    [Serializable]
    public class KnowledgeResonanceConfig
    {
        public int insightThreshold = 3;
        public int insightRewardDamage = 3;
        public int insightRewardBlock = 3;
        public int insightRewardDraw = 1;
        public float lowLevelBonus = 0.3f;
    }
}
