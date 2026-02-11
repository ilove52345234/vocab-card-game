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
}
