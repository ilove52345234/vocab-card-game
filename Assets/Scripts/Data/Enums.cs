namespace VocabCardGame.Data
{
    /// <summary>
    /// 五大元素
    /// </summary>
    public enum Element
    {
        Life,       // 🌿 生命：回復、成長、數量
        Force,      // 🔥 力量：直傷、爆發、連段
        Mind,       // 💧 思維：控場、抽牌、干擾
        Matter,     // ⚙️ 物質：裝備、防禦、持續
        Abstract    // ✨ 抽象：特殊規則、費用操控
    }

    /// <summary>
    /// 卡牌類型
    /// </summary>
    public enum CardType
    {
        Attack,     // 攻擊卡
        Skill,      // 技能卡
        Power,      // 能力卡（永久效果）
        Tactical    // 戰術卡
    }

    /// <summary>
    /// 卡牌稀有度
    /// </summary>
    public enum Rarity
    {
        Common,     // 普通
        Uncommon,   // 稀有
        Rare,       // 史詩
        Legendary   // 傳說
    }

    /// <summary>
    /// 熟練度等級 (Lv.0-7)
    /// </summary>
    public enum ProficiencyLevel
    {
        Locked = 0,     // 未解鎖
        New = 1,        // 新學：100% 答題，識讀簡單
        Known = 2,      // 認識：80% 答題，識讀中等
        Familiar = 3,   // 熟悉：60% 答題，識讀+聽力
        Remembered = 4, // 記住：40% 答題，聽力為主
        Proficient = 5, // 精通：20% 答題，聽力+拼字
        Mastered = 6,   // 掌握：10% 答題，拼字為主
        Internalized = 7 // 內化：0% 答題，自動發動
    }

    /// <summary>
    /// 答題模式
    /// </summary>
    public enum QuizMode
    {
        RecognitionEasy,    // 識讀簡單：選項差異大
        RecognitionMedium,  // 識讀中等：選項相近
        RecognitionHard,    // 識讀困難：易混淆
        ListeningEasy,      // 聽力簡單：發音差異大
        ListeningMedium,    // 聽力中等：發音相似
        ListeningHard,      // 聽力困難：聽音選意
        SpellingEasy,       // 拼字簡單：3-4字母
        SpellingMedium,     // 拼字中等：5-6字母
        SpellingHard        // 拼字困難：7+字母
    }

    /// <summary>
    /// 戰鬥姿態
    /// </summary>
    public enum Stance
    {
        None,       // 無姿態
        Offensive,  // 攻擊姿態：傷害+50%，受傷+25%
        Defensive,  // 防禦姿態：格擋+50%，傷害-25%
        Focused,    // 專注姿態：答題時間+5秒
        Frenzy      // 狂亂姿態：費用-1，回合結束棄2牌
    }

    /// <summary>
    /// 狀態效果類型
    /// </summary>
    public enum StatusEffectType
    {
        // 負面狀態
        Burning,    // 燃燒：每回合傷害
        Frozen,     // 冰凍：跳過行動
        Wet,        // 潮濕：閃電傷害x2
        Oiled,      // 油膩：火焰傷害x3
        Poisoned,   // 中毒：每回合傷害，可疊加
        Bleeding,   // 流血：受傷+30%

        // 正面狀態
        Strength,   // 力量：攻擊+X
        Dexterity,  // 敏捷：格擋+X
        Regeneration, // 再生：每回合回血
        Energized   // 充能：下回合+能量
    }

    /// <summary>
    /// 敵人意圖
    /// </summary>
    public enum EnemyIntent
    {
        Attack,         // 攻擊
        Defend,         // 防禦
        Buff,           // 增益自己
        Debuff,         // 削弱玩家
        AttackDebuff,   // 攻擊+削弱
        Special         // 特殊技能
    }

    /// <summary>
    /// 遊戲模式
    /// </summary>
    public enum GameMode
    {
        Adventure,      // 冒險模式
        EndlessAbyss,   // 無盡深淵
        DailyChallenge  // 每日挑戰
    }

    /// <summary>
    /// 遊戲階段
    /// </summary>
    public enum GamePhase
    {
        Tutorial,       // 教學期 Day 1-2
        Beginner,       // 初級期 Day 3-4
        Normal          // 正常期 Day 5+
    }

    /// <summary>
    /// 卡牌維度（詞性→戰鬥角色）
    /// </summary>
    public enum Dimension
    {
        Strike,     // 動詞(動作) + 有生/武器名詞 → 造成傷害
        Guard,      // 物品/建築/食物/植物名詞 → 格擋回血
        Boost,      // 形容詞 + 副詞 + 心智動詞 → 增益弱化
        Warp        // 抽象名詞 + 數詞 → 資源操控
    }

    /// <summary>
    /// 敵人類別
    /// </summary>
    public enum EnemyCategory
    {
        Normal,     // 普通敵人
        Elite,      // 精英怪
        Boss        // Boss
    }

    /// <summary>
    /// 遺物構詞類型
    /// </summary>
    public enum RelicMorphType
    {
        Prefix,     // 字首：觸發型
        Suffix,     // 字尾：增幅型
        Root        // 字根：基石型 + 詞庫連動
    }
}
