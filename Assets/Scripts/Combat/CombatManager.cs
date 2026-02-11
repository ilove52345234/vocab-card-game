using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VocabCardGame.Data;
using VocabCardGame.Core;
using VocabCardGame.Learning;

namespace VocabCardGame.Combat
{
    /// <summary>
    /// 戰鬥管理器
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [Header("Combat State")]
        public CombatState currentState = CombatState.NotInCombat;
        public int currentEnergy;
        public int maxEnergy = 3;
        public int turnNumber = 0;

        [Header("Player")]
        public CombatEntity player;
        public List<CardData> drawPile = new List<CardData>();
        public List<CardData> hand = new List<CardData>();
        public List<CardData> discardPile = new List<CardData>();
        public List<CardData> exhaustPile = new List<CardData>();

        [Header("Enemies")]
        public List<EnemyInstance> enemies = new List<EnemyInstance>();

        [Header("Combo Tracking")]
        public List<string> cardsPlayedThisTurn = new List<string>();
        public List<string> cardsPlayedThisCombat = new List<string>();
        private int consecutiveAttacks = 0;
        private int consecutiveSkills = 0;
        private int correctAnswersThisTurn = 0;
        private readonly Dictionary<string, int> resourcePool = new Dictionary<string, int>();
        private readonly HashSet<CardEffectType> resourceEffectWhitelist = new HashSet<CardEffectType>();
        private ResourceMediatorConfig resourceConfig;
        private readonly Dictionary<Dimension, int> dimensionCounts = new Dictionary<Dimension, int>();
        private readonly HashSet<Dimension> dimensionCoverage = new HashSet<Dimension>();
        private bool dimensionCoverageRewarded3 = false;
        private bool dimensionCoverageRewarded4 = false;
        private readonly HashSet<CardEffectType> dimensionEffectWhitelist = new HashSet<CardEffectType>();
        private DimensionChainConfig dimensionConfig;
        private readonly Dictionary<Element, int> elementCounts = new Dictionary<Element, int>();
        private readonly Dictionary<Element, int> elementResonanceTier = new Dictionary<Element, int>();
        private ElementResonanceConfig elementConfig;
        private int attackBonusThisTurn = 0;
        private int pendingCostReductionCount = 0;
        private KnowledgeResonanceConfig knowledgeConfig;
        private int insightTokensThisTurn = 0;
        private bool lastCardWasHighProficiency = false;
        private const string InkStainCardId = "ink_stain";
        private readonly HashSet<string> activeRelicIds = new HashSet<string>();
        private readonly Dictionary<string, RelicEffectEntry> activeRelicEffects = new Dictionary<string, RelicEffectEntry>();
        private bool relicReUsed = false;
        private bool relicDisUsed = false;
        private bool relicErUsed = false;
        private bool relicTractUsedThisTurn = false;
        private bool relicPortUsedThisTurn = false;
        private bool relicStructUsedThisTurn = false;

        // 事件
        public event Action<CombatState> OnCombatStateChanged;
        public event Action<CardData> OnCardPlayed;
        public event Action<CardData> OnCardDrawn;
        public event Action<EnemyInstance, int> OnEnemyDamaged;
        public event Action<int> OnPlayerDamaged;
        public event Action<Stance> OnStanceChanged;
        public event Action<ComboData> OnComboTriggered;
        public event Action OnTurnStart;
        public event Action OnTurnEnd;
        public event Action<bool> OnCombatEnd; // true = victory
        public event Action<InsightRewardOption[]> OnInsightRewardAvailable;

        /// <summary>
        /// 初始化一輪遊戲
        /// </summary>
        public void InitializeRun()
        {
            var gameManager = GameManager.Instance;

            // 初始化玩家戰鬥狀態
            player = new CombatEntity
            {
                maxHp = gameManager.playerData.MaxHp,
                currentHp = gameManager.playerData.MaxHp
            };

            maxEnergy = gameManager.playerData.BaseEnergy;

            // 從學習進度建立牌組
            BuildDeck();
        }

        /// <summary>
        /// 建立牌組
        /// </summary>
        private void BuildDeck()
        {
            drawPile.Clear();
            hand.Clear();
            discardPile.Clear();
            exhaustPile.Clear();

            var gameManager = GameManager.Instance;
            var cards = gameManager.dataManager.GetAllCards();

            foreach (var card in cards)
            {
                // 確保有學習進度（MVP 直接初始化為 New）
                card.progress = gameManager.learningManager.EnsureProgress(card.wordId, ProficiencyLevel.New);
                drawPile.Add(card);
            }
        }

        /// <summary>
        /// 開始戰鬥
        /// </summary>
        public void StartCombat(List<EnemyData> enemyDatas)
        {
            currentState = CombatState.PlayerTurn;
            turnNumber = 0;
            cardsPlayedThisCombat.Clear();
            InitializeResourceMediator();
            InitializeDimensionChain();
            InitializeElementResonance();
            InitializeKnowledgeResonance();
            InitializeRelicEffects();

            // 創建敵人實例
            enemies.Clear();
            foreach (var data in enemyDatas)
            {
                enemies.Add(new EnemyInstance(data));
            }

            // 決定敵人意圖
            foreach (var enemy in enemies)
            {
                enemy.DecideNextAction();
            }

            ApplyRelicStartCombatEffects();

            // 洗牌
            ShuffleDeck();

            // 開始第一回合
            StartPlayerTurn();
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        private void ShuffleDeck()
        {
            // Fisher-Yates shuffle
            for (int i = drawPile.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
            }
        }

        /// <summary>
        /// 開始玩家回合
        /// </summary>
        public void StartPlayerTurn()
        {
            turnNumber++;
            currentState = CombatState.PlayerTurn;
            cardsPlayedThisTurn.Clear();
            consecutiveAttacks = 0;
            consecutiveSkills = 0;
            correctAnswersThisTurn = 0;
            ResetResourcePool();
            ResetSynergyTurnState();
            ResetRelicTurnState();

            // 重置能量
            currentEnergy = maxEnergy;

            // 處理回合開始效果
            player.ProcessTurnStart();

            // 抽牌
            int drawCount = GameManager.Instance.playerData.StartingHandSize;
            for (int i = 0; i < drawCount; i++)
            {
                DrawCard();
            }

            OnTurnStart?.Invoke();
            OnCombatStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// 抽一張牌
        /// </summary>
        public void DrawCard()
        {
            if (drawPile.Count == 0)
            {
                // 洗棄牌堆
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
            }

            if (drawPile.Count > 0)
            {
                var card = drawPile[0];
                drawPile.RemoveAt(0);
                hand.Add(card);
                OnCardDrawn?.Invoke(card);
            }
        }

        /// <summary>
        /// 嘗試打出卡牌
        /// </summary>
        public void TryPlayCard(CardData card, EnemyInstance target = null)
        {
            if (currentState != CombatState.PlayerTurn) return;
            int effectiveCost = GetEffectiveEnergyCost(card);
            if (currentEnergy < effectiveCost) return;
            if (!hand.Contains(card)) return;

            // 檢查是否需要答題
            if (card.RequiresQuiz())
            {
                currentState = CombatState.AnsweringQuiz;
                OnCombatStateChanged?.Invoke(currentState);

                // 觸發答題 UI（由 UI 層處理）
                // 答題結果會回呼 OnQuizAnswered
                QuizManager.Instance.StartQuiz(card, (isCorrect, quality) =>
                {
                    OnQuizAnswered(card, target, isCorrect, quality, effectiveCost);
                });
            }
            else
            {
                // 直接執行卡牌效果
                ExecuteCard(card, target, 1f, effectiveCost);
            }
        }

        /// <summary>
        /// 答題結果回呼
        /// </summary>
        private void OnQuizAnswered(CardData card, EnemyInstance target, bool isCorrect, int quality, int energyCostSpent)
        {
            currentState = CombatState.PlayerTurn;

            bool isCorrectForEffect = isCorrect;
            bool countForProgress = isCorrect;

            if (!isCorrect && HasRelic("relic_mis"))
            {
                var effect = GetRelicEffect("relic_mis");
                if (effect != null && effect.type == RelicEffectType.MistakeConvert)
                {
                    if (UnityEngine.Random.value < effect.floatValue)
                    {
                        isCorrectForEffect = true;
                        countForProgress = false;
                    }
                }
            }

            if (countForProgress)
            {
                card.progress?.UpdateProgress(isCorrect, quality);
                GameManager.Instance.learningManager.OnAnswerResult(isCorrect);
            }

            if (isCorrectForEffect)
            {
                correctAnswersThisTurn++;
                GameManager.Instance.AddExperience(GetExpForQuizMode(card.GetQuizMode()));

                // 執行卡牌效果
                ExecuteCard(card, target, card.GetEffectMultiplier(), energyCostSpent);

                // 檢查專注姿態
                CheckFocusedStance();

                ApplyRelicOnCorrect(target);
            }
            else
            {
                // 答錯處理
                ApplyBookwormWrongAnswer();
                WeakenSleepingGargoyleOnCorrect(false);

                float penalty = GameManager.Instance.playerData.GetGamePhase() == GamePhase.Tutorial
                    ? 1f  // 教學期不懲罰
                    : GameManager.Instance.playerData.GetGamePhase() == GamePhase.Beginner
                        ? 0.7f  // 初級期輕微懲罰
                        : GetWrongAnswerPenalty(card.progress?.level ?? ProficiencyLevel.New);

                if (penalty > 0)
                {
                    ExecuteCard(card, target, penalty, energyCostSpent);
                }
                else
                {
                    // 卡牌失效，只扣能量
                    currentEnergy -= energyCostSpent;
                    hand.Remove(card);
                    discardPile.Add(card);
                    ConsumeCostReduction();
                    ConsumePortReduction(card);
                }

                // 離開專注姿態
                if (player.currentStance == Stance.Focused)
                {
                    ChangeStance(Stance.None);
                }
            }

            if (isCorrectForEffect)
            {
                WeakenSleepingGargoyleOnCorrect(true);
                RemoveInkStainOnCorrect();
            }

            OnCombatStateChanged?.Invoke(currentState);
        }

        private int GetExpForQuizMode(QuizMode mode)
        {
            return mode switch
            {
                QuizMode.RecognitionEasy or QuizMode.RecognitionMedium or QuizMode.RecognitionHard => 1,
                QuizMode.ListeningEasy or QuizMode.ListeningMedium or QuizMode.ListeningHard => 2,
                QuizMode.SpellingEasy or QuizMode.SpellingMedium or QuizMode.SpellingHard => 3,
                _ => 1
            };
        }

        private float GetWrongAnswerPenalty(ProficiencyLevel level)
        {
            return level switch
            {
                ProficiencyLevel.New => 0f,         // 失效
                ProficiencyLevel.Known => 0.5f,     // 效果減半
                ProficiencyLevel.Familiar => 0.7f,
                ProficiencyLevel.Remembered => 0.8f,
                _ => 0.8f
            };
        }

        /// <summary>
        /// 執行卡牌效果
        /// </summary>
        private void ExecuteCard(CardData card, EnemyInstance target, float multiplier, int energyCostSpent)
        {
            currentEnergy -= energyCostSpent;
            hand.Remove(card);
            ConsumeCostReduction();
            ConsumePortReduction(card);

            // 記錄打出的卡牌
            cardsPlayedThisTurn.Add(card.wordId);
            cardsPlayedThisCombat.Add(card.wordId);

            // 追蹤連續攻擊/技能
            if (card.cardType == CardType.Attack)
            {
                consecutiveAttacks++;
                consecutiveSkills = 0;
            }
            else if (card.cardType == CardType.Skill)
            {
                consecutiveSkills++;
                consecutiveAttacks = 0;
            }

            // 取得卡牌元素（用於元素弱點/抗性計算）
            Element? cardElement = card.wordData?.element;

            // 協同倍率：維度連鎖 + 知識共振
            float dimensionBonus = ApplyDimensionChain(card);
            float knowledgeBonus = ApplyKnowledgeResonance(card);

            // 先消費資源媒介，再執行效果
            float resourceBonus = ConsumeResources(card);
            float lexiconBonus = GetLexiconBonus(card);

            // 執行效果
            foreach (var effect in card.effects)
            {
                ExecuteEffect(effect, target, multiplier, resourceBonus, dimensionBonus, knowledgeBonus, lexiconBonus, cardElement, card);
            }

            // 檢查姿態觸發
            CheckStanceTriggers(card);

            // 檢查 Combo
            CheckComboTriggers();

            // 檢查連鎖
            CheckChainTriggers(card);

            // 產出資源媒介
            ProduceResources(card);

            // 元素共鳴
            ApplyElementResonance(card);

            // 精英怪機制
            ApplyBookwormNonAttack(card);

            // 卡牌進入棄牌堆或消耗堆
            if (card.effects.Any(e => e.type == CardEffectType.Exhaust))
            {
                exhaustPile.Add(card);
            }
            else
            {
                discardPile.Add(card);
            }

            // 遺物：re- 首張回手
            ApplyRelicReturnFirstCard(card);

            OnCardPlayed?.Invoke(card);

            // 檢查戰鬥結束
            CheckCombatEnd();
        }

        /// <summary>
        /// 執行單個效果
        /// </summary>
        private void ExecuteEffect(CardEffect effect, EnemyInstance target, float multiplier, float resourceBonus, float dimensionBonus, float knowledgeBonus, float lexiconBonus, Element? cardElement = null, CardData card = null)
        {
            int value = Mathf.RoundToInt(effect.value * multiplier);

            if (resourceBonus > 0f && ShouldApplyResourceBonus(effect.type))
            {
                value = Mathf.RoundToInt(value * (1f + resourceBonus));
            }

            if (dimensionBonus > 0f && ShouldApplyDimensionBonus(effect.type))
            {
                value = Mathf.RoundToInt(value * (1f + dimensionBonus));
            }

            if (knowledgeBonus > 0f && ShouldApplyKnowledgeBonus(effect.type))
            {
                value = Mathf.RoundToInt(value * (1f + knowledgeBonus));
            }

            if (lexiconBonus > 0f && ShouldApplyLexiconBonus(effect.type))
            {
                value = Mathf.RoundToInt(value * (1f + lexiconBonus));
            }

            if (card != null)
            {
                value = ApplyRelicEffectValueBonus(card, effect.type, value);
            }

            // 計算姿態加成
            value = ApplyStanceModifier(effect.type, value);

            // 計算元素弱點/抗性
            if (target != null && cardElement.HasValue)
            {
                value = ApplyElementModifier(effect.type, value, target, cardElement.Value);
            }

            switch (effect.type)
            {
                case CardEffectType.Damage:
                    if (target != null)
                    {
                        int finalValue = value + attackBonusThisTurn;
                        target.entity.TakeDamage(finalValue);
                        OnEnemyDamaged?.Invoke(target, finalValue);
                    }
                    break;

                case CardEffectType.DamageAll:
                    foreach (var enemy in enemies.Where(e => e.entity.IsAlive))
                    {
                        int baseValue = value;
                        int dmg = cardElement.HasValue
                            ? ApplyElementModifier(effect.type, baseValue, enemy, cardElement.Value)
                            : baseValue;
                        dmg += attackBonusThisTurn;
                        enemy.entity.TakeDamage(dmg);
                        OnEnemyDamaged?.Invoke(enemy, dmg);
                    }
                    break;

                case CardEffectType.Block:
                    AddBlockWithRelic(value);
                    break;

                case CardEffectType.Heal:
                    player.Heal(value);
                    break;

                case CardEffectType.DrawCard:
                    for (int i = 0; i < value; i++) DrawCard();
                    break;

                case CardEffectType.GainEnergy:
                    currentEnergy += value;
                    break;

                case CardEffectType.ApplyBurning:
                case CardEffectType.ApplyFrozen:
                case CardEffectType.ApplyPoison:
                case CardEffectType.ApplyWet:
                case CardEffectType.ApplyBleeding:
                    if (target != null)
                    {
                        var statusType = effect.type switch
                        {
                            CardEffectType.ApplyBurning => StatusEffectType.Burning,
                            CardEffectType.ApplyFrozen => StatusEffectType.Frozen,
                            CardEffectType.ApplyPoison => StatusEffectType.Poisoned,
                            CardEffectType.ApplyWet => StatusEffectType.Wet,
                            CardEffectType.ApplyBleeding => StatusEffectType.Bleeding,
                            _ => StatusEffectType.Burning
                        };
                        target.entity.ApplyStatus(statusType, value, effect.duration);

                        // 檢查狀態互動
                        CheckStatusInteractions(target);
                    }
                    break;

                case CardEffectType.ApplyStrength:
                    player.ApplyStatus(StatusEffectType.Strength, value, effect.duration);
                    break;

                case CardEffectType.ApplyDexterity:
                    player.ApplyStatus(StatusEffectType.Dexterity, value, effect.duration);
                    break;
            }
        }

        private int ApplyStanceModifier(CardEffectType effectType, int value)
        {
            if (player.currentStance == Stance.Offensive &&
                (effectType == CardEffectType.Damage || effectType == CardEffectType.DamageAll))
            {
                return Mathf.RoundToInt(value * 1.5f);
            }

            if (player.currentStance == Stance.Defensive && effectType == CardEffectType.Block)
            {
                return Mathf.RoundToInt(value * 1.5f);
            }

            return value;
        }

        /// <summary>
        /// 計算元素弱點/抗性倍率
        /// 弱點：傷害 ×1.5、抗性：傷害 ×0.5
        /// </summary>
        private int ApplyElementModifier(CardEffectType effectType, int value, EnemyInstance enemy, Element cardElement)
        {
            if (effectType != CardEffectType.Damage && effectType != CardEffectType.DamageAll)
                return value;

            var enemyData = enemy.data;
            if (enemyData == null) return value;

            // 弱點：+50% 傷害
            if (enemyData.weakness.HasValue && enemyData.weakness.Value == cardElement)
            {
                return Mathf.RoundToInt(value * 1.5f);
            }

            // 抗性：-50% 傷害
            if (enemyData.resistance.HasValue && enemyData.resistance.Value == cardElement)
            {
                return Mathf.RoundToInt(value * 0.5f);
            }

            return value;
        }

        /// <summary>
        /// 檢查狀態互動
        /// </summary>
        private void CheckStatusInteractions(EnemyInstance target)
        {
            var status = target.entity.statusEffects;

            // 燃燒 + 油膩 = 爆炸
            if (status.ContainsKey(StatusEffectType.Burning) && status.ContainsKey(StatusEffectType.Oiled))
            {
                target.entity.TakeDamage(20);
                status.Remove(StatusEffectType.Burning);
                status.Remove(StatusEffectType.Oiled);
            }

            // 潮濕 + 冰凍 = 深度冰凍
            if (status.ContainsKey(StatusEffectType.Wet) && status.ContainsKey(StatusEffectType.Frozen))
            {
                status[StatusEffectType.Frozen].duration += 1;
                status.Remove(StatusEffectType.Wet);
            }

            // 燃燒 + 冰凍 = 互相抵消
            if (status.ContainsKey(StatusEffectType.Burning) && status.ContainsKey(StatusEffectType.Frozen))
            {
                status.Remove(StatusEffectType.Burning);
                status.Remove(StatusEffectType.Frozen);
            }

            // 中毒 + 流血 = 惡化
            if (status.ContainsKey(StatusEffectType.Poisoned) && status.ContainsKey(StatusEffectType.Bleeding))
            {
                status[StatusEffectType.Poisoned].value = Mathf.RoundToInt(status[StatusEffectType.Poisoned].value * 1.5f);
            }
        }

        /// <summary>
        /// 檢查姿態觸發
        /// </summary>
        private void CheckStanceTriggers(CardData card)
        {
            // 連續 3 張攻擊卡 → 攻擊姿態
            if (consecutiveAttacks >= 3 && player.currentStance != Stance.Offensive)
            {
                ChangeStance(Stance.Offensive);
            }

            // 連續 2 張技能卡 → 防禦姿態
            if (consecutiveSkills >= 2 && player.currentStance != Stance.Defensive)
            {
                ChangeStance(Stance.Defensive);
            }

            // 單回合打出 5 張卡 → 狂亂姿態
            if (cardsPlayedThisTurn.Count >= 5 && player.currentStance != Stance.Frenzy)
            {
                ChangeStance(Stance.Frenzy);
            }
        }

        private void CheckFocusedStance()
        {
            // 同回合答對 3 題 → 專注姿態
            if (correctAnswersThisTurn >= 3 && player.currentStance != Stance.Focused)
            {
                ChangeStance(Stance.Focused);
            }
        }

        private void ChangeStance(Stance newStance)
        {
            player.currentStance = newStance;
            OnStanceChanged?.Invoke(newStance);
        }

        /// <summary>
        /// 檢查 Combo 觸發
        /// </summary>
        private void CheckComboTriggers()
        {
            // TODO: 檢查已打出的卡牌是否符合任何 Combo
        }

        /// <summary>
        /// 檢查連鎖觸發
        /// </summary>
        private void CheckChainTriggers(CardData playedCard)
        {
            // TODO: 檢查是否觸發連鎖效果
        }

        /// <summary>
        /// 初始化維度連鎖協同設定
        /// </summary>
        private void InitializeDimensionChain()
        {
            var config = GameManager.Instance?.dataManager?.GetSynergyConfig();
            dimensionConfig = config?.dimensionChain ?? new DimensionChainConfig();

            dimensionEffectWhitelist.Clear();
            if (dimensionConfig.applyToEffects != null && dimensionConfig.applyToEffects.Length > 0)
            {
                foreach (var name in dimensionConfig.applyToEffects)
                {
                    if (Enum.TryParse(name, out CardEffectType effectType))
                    {
                        dimensionEffectWhitelist.Add(effectType);
                    }
                }
            }
            else
            {
                dimensionEffectWhitelist.Add(CardEffectType.Damage);
                dimensionEffectWhitelist.Add(CardEffectType.Block);
                dimensionEffectWhitelist.Add(CardEffectType.Heal);
                dimensionEffectWhitelist.Add(CardEffectType.DrawCard);
                dimensionEffectWhitelist.Add(CardEffectType.GainEnergy);
            }
        }

        /// <summary>
        /// 初始化元素共鳴協同設定
        /// </summary>
        private void InitializeElementResonance()
        {
            var config = GameManager.Instance?.dataManager?.GetSynergyConfig();
            elementConfig = config?.elementResonance ?? new ElementResonanceConfig();
        }

        /// <summary>
        /// 初始化知識共振協同設定
        /// </summary>
        private void InitializeKnowledgeResonance()
        {
            var config = GameManager.Instance?.dataManager?.GetSynergyConfig();
            knowledgeConfig = config?.knowledgeResonance ?? new KnowledgeResonanceConfig();
        }

        /// <summary>
        /// 回合開始重置協同狀態
        /// </summary>
        private void ResetSynergyTurnState()
        {
            dimensionCounts.Clear();
            dimensionCoverage.Clear();
            dimensionCoverageRewarded3 = false;
            dimensionCoverageRewarded4 = false;
            elementCounts.Clear();
            elementResonanceTier.Clear();
            attackBonusThisTurn = 0;
            pendingCostReductionCount = 0;
            insightTokensThisTurn = 0;
            lastCardWasHighProficiency = false;
        }

        /// <summary>
        /// 維度連鎖：回傳本張卡倍率加成
        /// </summary>
        private float ApplyDimensionChain(CardData card)
        {
            if (dimensionConfig == null) InitializeDimensionChain();
            if (!dimensionCounts.TryGetValue(card.dimension, out int count))
            {
                count = 0;
            }
            count++;
            dimensionCounts[card.dimension] = count;

            if (!dimensionCoverage.Contains(card.dimension))
            {
                dimensionCoverage.Add(card.dimension);
                int coverageCount = dimensionCoverage.Count;
                if (!dimensionCoverageRewarded3 && dimensionConfig.coverageDrawAt > 0 && coverageCount >= dimensionConfig.coverageDrawAt)
                {
                    DrawCard();
                    dimensionCoverageRewarded3 = true;
                }
                if (!dimensionCoverageRewarded4 && dimensionConfig.coverageDrawAndEnergyAt > 0 && coverageCount >= dimensionConfig.coverageDrawAndEnergyAt)
                {
                    DrawCard();
                    currentEnergy += 1;
                    dimensionCoverageRewarded4 = true;
                }
            }

            if (count == 2) return dimensionConfig.secondCardBonus;
            if (count == 3) return dimensionConfig.thirdCardBonus;
            return 0f;
        }

        private bool ShouldApplyDimensionBonus(CardEffectType effectType)
        {
            return dimensionEffectWhitelist.Contains(effectType);
        }

        /// <summary>
        /// 元素共鳴：依元素累計觸發小效果
        /// </summary>
        private void ApplyElementResonance(CardData card)
        {
            if (elementConfig == null) InitializeElementResonance();
            if (card.wordData == null) return;
            Element element = card.wordData.element;

            elementCounts.TryGetValue(element, out int count);
            count++;
            elementCounts[element] = count;

            elementResonanceTier.TryGetValue(element, out int tier);
            if (count == 2 && tier < 2)
            {
                ApplyElementResonanceTier(element, 2);
                elementResonanceTier[element] = 2;
            }
            else if (count == 3 && tier < 3)
            {
                ApplyElementResonanceTier(element, 3);
                elementResonanceTier[element] = 3;
            }
        }

        private void ApplyElementResonanceTier(Element element, int tier)
        {
            switch (element)
            {
                case Element.Life:
                    int healValue = tier == 2 ? elementConfig.lifeHeal2 : elementConfig.lifeHeal3 - elementConfig.lifeHeal2;
                    if (healValue > 0) player.Heal(healValue);
                    break;
                case Element.Force:
                    int addAttack = tier == 2 ? elementConfig.forceAttackBonus2 : elementConfig.forceAttackBonus3 - elementConfig.forceAttackBonus2;
                    if (addAttack > 0) attackBonusThisTurn += addAttack;
                    break;
                case Element.Mind:
                    int drawCount = tier == 2 ? elementConfig.mindDraw2 : elementConfig.mindDraw3 - elementConfig.mindDraw2;
                    for (int i = 0; i < drawCount; i++) DrawCard();
                    break;
                case Element.Matter:
                    int blockValue = tier == 2 ? elementConfig.matterBlock2 : elementConfig.matterBlock3 - elementConfig.matterBlock2;
                    if (blockValue > 0) AddBlockWithRelic(blockValue);
                    break;
                case Element.Abstract:
                    int reductionCount = tier == 2 ? elementConfig.abstractCostReduction2 : elementConfig.abstractCostReduction3;
                    pendingCostReductionCount = Mathf.Max(pendingCostReductionCount, reductionCount);
                    break;
            }
        }

        /// <summary>
        /// 知識共振：回傳本張卡倍率加成
        /// </summary>
        private float ApplyKnowledgeResonance(CardData card)
        {
            if (knowledgeConfig == null) InitializeKnowledgeResonance();
            if (card.progress == null) return 0f;

            bool isHigh = card.progress.level >= ProficiencyLevel.Proficient;
            bool isLow = card.progress.level == ProficiencyLevel.New || card.progress.level == ProficiencyLevel.Known;

            float bonus = 0f;
            if (lastCardWasHighProficiency && isLow)
            {
                bonus = knowledgeConfig.lowLevelBonus;
            }

            if (isHigh)
            {
                insightTokensThisTurn += 1;
            }

            lastCardWasHighProficiency = isHigh;
            return bonus;
        }

        private bool ShouldApplyKnowledgeBonus(CardEffectType effectType)
        {
            return effectType == CardEffectType.Damage
                || effectType == CardEffectType.DamageAll
                || effectType == CardEffectType.Block
                || effectType == CardEffectType.Heal
                || effectType == CardEffectType.DrawCard
                || effectType == CardEffectType.GainEnergy;
        }

        /// <summary>
        /// 洞察獎勵：回合結束觸發（MVP 預設抽牌）\n        /// </summary>
        private void ResolveInsightReward()
        {
            if (knowledgeConfig == null) InitializeKnowledgeResonance();
            if (insightTokensThisTurn < knowledgeConfig.insightThreshold) return;

            InsightRewardOption[] options =
            {
                InsightRewardOption.Damage,
                InsightRewardOption.Block,
                InsightRewardOption.Draw
            };

            OnInsightRewardAvailable?.Invoke(options);

            // MVP 預設：直接抽牌，後續 UI 再提供選擇
            for (int i = 0; i < knowledgeConfig.insightRewardDraw; i++) DrawCard();

            insightTokensThisTurn = 0;
        }

        /// <summary>
        /// 抽象共鳴：下一張卡費用降低
        /// </summary>
        private int GetEffectiveEnergyCost(CardData card)
        {
            int reduction = 0;

            if (pendingCostReductionCount > 0)
            {
                reduction += 1;
            }

            if (!relicPortUsedThisTurn && card != null && card.dimension == Dimension.Warp && HasRelic("relic_port"))
            {
                var effect = GetRelicEffect("relic_port");
                if (effect != null && effect.type == RelicEffectType.FirstDimensionCostReduction
                    && IsRelicDimensionMatch(effect, card.dimension))
                {
                    reduction += Mathf.Max(1, effect.intValue);
                }
            }

            return Mathf.Max(0, card.energyCost - reduction);
        }

        private void ConsumeCostReduction()
        {
            if (pendingCostReductionCount > 0)
            {
                pendingCostReductionCount--;
            }
        }

        private void ConsumePortReduction(CardData card)
        {
            if (relicPortUsedThisTurn) return;
            if (card == null || card.dimension != Dimension.Warp) return;
            if (!HasRelic("relic_port")) return;
            var effect = GetRelicEffect("relic_port");
            if (effect == null || effect.type != RelicEffectType.FirstDimensionCostReduction) return;

            relicPortUsedThisTurn = true;
        }

        private void InitializeRelicEffects()
        {
            activeRelicIds.Clear();
            activeRelicEffects.Clear();
            var gameManager = GameManager.Instance;
            foreach (var relicId in gameManager.GetActiveRelics())
            {
                if (string.IsNullOrWhiteSpace(relicId)) continue;
                activeRelicIds.Add(relicId);
                var effect = gameManager.GetRelicEffect(relicId);
                if (effect != null)
                {
                    activeRelicEffects[relicId] = effect;
                }
            }

            relicReUsed = false;
            relicDisUsed = false;
            relicErUsed = false;
        }

        private void ResetRelicTurnState()
        {
            relicTractUsedThisTurn = false;
            relicPortUsedThisTurn = false;
            relicStructUsedThisTurn = false;
        }

        private bool HasRelic(string relicId)
        {
            return activeRelicIds.Contains(relicId);
        }

        private RelicEffectEntry GetRelicEffect(string relicId)
        {
            activeRelicEffects.TryGetValue(relicId, out var effect);
            return effect;
        }

        private void ApplyRelicStartCombatEffects()
        {
            if (HasRelic("relic_ness"))
            {
                var effect = GetRelicEffect("relic_ness");
                if (effect != null && effect.type == RelicEffectType.StartBlock)
                {
                    player.AddBlock(effect.intValue);
                }
            }

            if (HasRelic("relic_pre"))
            {
                var effect = GetRelicEffect("relic_pre");
                if (effect != null && effect.type == RelicEffectType.PreviewEnemyActions)
                {
                    foreach (var enemy in enemies)
                    {
                        enemy.PreparePreview(effect.intValue);
                    }
                }
            }
        }

        private void ApplyRelicOnCorrect(EnemyInstance target)
        {
            if (HasRelic("relic_ly"))
            {
                var effect = GetRelicEffect("relic_ly");
                if (effect != null && effect.type == RelicEffectType.HealOnCorrect && effect.intValue > 0)
                {
                    player.Heal(effect.intValue);
                }
            }

            if (HasRelic("relic_rupt"))
            {
                var effect = GetRelicEffect("relic_rupt");
                if (effect != null && effect.type == RelicEffectType.RemoveEnemyBlockOnCorrect)
                {
                    if (UnityEngine.Random.value < effect.floatValue)
                    {
                        var enemy = target ?? enemies.FirstOrDefault(e => e.entity.IsAlive);
                        if (enemy != null)
                        {
                            enemy.entity.block = Mathf.Max(0, enemy.entity.block - effect.intValue);
                        }
                    }
                }
            }
        }

        private void ApplyRelicReturnFirstCard(CardData card)
        {
            if (relicReUsed) return;
            if (!HasRelic("relic_re")) return;
            var effect = GetRelicEffect("relic_re");
            if (effect == null || effect.type != RelicEffectType.ReturnFirstCard) return;

            relicReUsed = true;

            if (discardPile.Remove(card) || exhaustPile.Remove(card))
            {
                hand.Add(card);
            }
        }

        private int ApplyRelicEffectValueBonus(CardData card, CardEffectType effectType, int value)
        {
            if (card == null) return value;

            if (HasRelic("relic_er") && !relicErUsed && card.cardType == CardType.Attack &&
                (effectType == CardEffectType.Damage || effectType == CardEffectType.DamageAll))
            {
                var effect = GetRelicEffect("relic_er");
                if (effect != null && effect.type == RelicEffectType.FirstAttackBonus)
                {
                    value += effect.intValue;
                    relicErUsed = true;
                }
            }

            if (HasRelic("relic_less") && card.dimension == Dimension.Strike &&
                (effectType == CardEffectType.Damage || effectType == CardEffectType.DamageAll))
            {
                var effect = GetRelicEffect("relic_less");
                if (effect != null && effect.type == RelicEffectType.DimensionDamageBonus
                    && IsRelicDimensionMatch(effect, card.dimension))
                {
                    value += effect.intValue;
                }
            }

            if (HasRelic("relic_ful") && card.dimension == Dimension.Guard &&
                effectType == CardEffectType.Block)
            {
                var effect = GetRelicEffect("relic_ful");
                if (effect != null && effect.type == RelicEffectType.DimensionBlockBonus
                    && IsRelicDimensionMatch(effect, card.dimension))
                {
                    value += effect.intValue;
                }
            }

            if (HasRelic("relic_tract") && !relicTractUsedThisTurn && card.dimension == Dimension.Boost)
            {
                var effect = GetRelicEffect("relic_tract");
                if (effect != null && effect.type == RelicEffectType.DimensionDrawOncePerTurn
                    && IsRelicDimensionMatch(effect, card.dimension))
                {
                    for (int i = 0; i < Mathf.Max(1, effect.intValue); i++) DrawCard();
                    relicTractUsedThisTurn = true;
                }
            }

            return value;
        }

        private int ApplyStructBlockBonus(int value)
        {
            if (relicStructUsedThisTurn) return value;
            if (!HasRelic("relic_struct")) return value;
            var effect = GetRelicEffect("relic_struct");
            if (effect == null || effect.type != RelicEffectType.BlockPerTurn) return value;

            relicStructUsedThisTurn = true;
            return value + effect.intValue;
        }

        private void AddBlockWithRelic(int value)
        {
            player.AddBlock(ApplyStructBlockBonus(value));
        }

        private float GetLexiconBonus(CardData card)
        {
            if (card == null || card.wordData == null || string.IsNullOrEmpty(card.wordData.english)) return 0f;
            float maxBonus = 0f;
            foreach (var relicId in activeRelicIds)
            {
                var effect = GetRelicEffect(relicId);
                if (effect == null || effect.lexiconBonus <= 0f) continue;

                var relicData = GameManager.Instance.dataManager.GetRelic(relicId);
                if (relicData == null || string.IsNullOrEmpty(relicData.affix)) continue;

                if (card.wordData.english.IndexOf(relicData.affix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (effect.lexiconBonus > maxBonus) maxBonus = effect.lexiconBonus;
                }
            }
            return maxBonus;
        }

        private bool ShouldApplyLexiconBonus(CardEffectType effectType)
        {
            return effectType == CardEffectType.Damage
                || effectType == CardEffectType.DamageAll
                || effectType == CardEffectType.Block
                || effectType == CardEffectType.Heal
                || effectType == CardEffectType.DrawCard
                || effectType == CardEffectType.GainEnergy;
        }

        private bool IsRelicDimensionMatch(RelicEffectEntry effect, Dimension dimension)
        {
            if (effect == null || string.IsNullOrEmpty(effect.dimension)) return true;
            return Enum.TryParse(effect.dimension, out Dimension parsed) && parsed == dimension;
        }

        private void DiscardCard(CardData card)
        {
            if (card == null) return;
            discardPile.Add(card);
            if (HasRelic("relic_ject"))
            {
                var effect = GetRelicEffect("relic_ject");
                if (effect != null && effect.type == RelicEffectType.DamageOnDiscard)
                {
                    var enemy = enemies.FirstOrDefault(e => e.entity.IsAlive);
                    if (enemy != null)
                    {
                        enemy.entity.TakeDamage(effect.intValue);
                        OnEnemyDamaged?.Invoke(enemy, effect.intValue);
                    }
                }
            }
        }

        private bool IsBookworm(EnemyInstance enemy)
        {
            return enemy != null && enemy.data != null && enemy.data.id == "elite_bookworm";
        }

        private bool IsGargoyle(EnemyInstance enemy)
        {
            return enemy != null && enemy.data != null && enemy.data.id == "elite_gargoyle";
        }

        private bool IsSleepingGargoyle(EnemyInstance enemy)
        {
            return IsGargoyle(enemy) && enemy.sleepTurnsRemaining > 0;
        }

        private bool IsInkBlob(EnemyInstance enemy)
        {
            return enemy != null && enemy.data != null && enemy.data.id == "elite_inkblob";
        }

        private void ApplyBookwormNonAttack(CardData card)
        {
            if (card.cardType == CardType.Attack) return;
            foreach (var enemy in enemies.Where(e => e.entity.IsAlive))
            {
                if (IsBookworm(enemy))
                {
                    enemy.entity.ApplyStatus(StatusEffectType.Strength, 1, 999);
                }
            }
        }

        private void ApplyBookwormWrongAnswer()
        {
            foreach (var enemy in enemies.Where(e => e.entity.IsAlive))
            {
                if (IsBookworm(enemy))
                {
                    enemy.entity.ApplyStatus(StatusEffectType.Strength, 1, 999);
                }
            }
        }

        private void ApplyGargoyleSleepTick(EnemyInstance enemy)
        {
            enemy.entity.ApplyStatus(StatusEffectType.Strength, 3, 999);
            enemy.sleepTurnsRemaining = Mathf.Max(0, enemy.sleepTurnsRemaining - 1);
        }

        private void WeakenSleepingGargoyleOnCorrect(bool isCorrect)
        {
            if (!isCorrect) return;
            foreach (var enemy in enemies.Where(e => e.entity.IsAlive))
            {
                if (!IsSleepingGargoyle(enemy)) continue;
                if (enemy.entity.statusEffects.TryGetValue(StatusEffectType.Strength, out var strength))
                {
                    strength.value = Mathf.Max(0, strength.value - 1);
                }
            }
        }

        private int GetEnemyStrengthBonus(EnemyInstance enemy)
        {
            if (enemy == null) return 0;
            if (enemy.entity.statusEffects.TryGetValue(StatusEffectType.Strength, out var strength))
            {
                return Mathf.Max(0, strength.value);
            }
            return 0;
        }

        private void ExecuteEnemyAttack(EnemyInstance enemy, int value)
        {
            int times = IsGargoyle(enemy) && enemy.sleepTurnsRemaining == 0 ? 2 : 1;
            for (int i = 0; i < times; i++)
            {
                int reflectDamage = 0;
                if (HasRelic("relic_over") && player.block > value)
                {
                    var effect = GetRelicEffect("relic_over");
                    if (effect != null && effect.type == RelicEffectType.OverblockReflect)
                    {
                        int overflow = player.block - value;
                        reflectDamage = Mathf.RoundToInt(overflow * effect.floatValue);
                    }
                }

                player.TakeDamage(value);
                OnPlayerDamaged?.Invoke(value);

                if (reflectDamage > 0 && enemy != null)
                {
                    enemy.entity.TakeDamage(reflectDamage);
                    OnEnemyDamaged?.Invoke(enemy, reflectDamage);
                }
            }
        }

        private void AddInkStainToDiscard()
        {
            discardPile.Add(CreateInkStainCard());
        }

        private void RemoveInkStainOnCorrect()
        {
            int idx = hand.FindIndex(c => c.wordId == InkStainCardId);
            if (idx >= 0)
            {
                var card = hand[idx];
                hand.RemoveAt(idx);
                exhaustPile.Add(card);
            }
        }

        private CardData CreateInkStainCard()
        {
            return new CardData
            {
                wordId = InkStainCardId,
                cardType = CardType.Skill,
                energyCost = 0,
                effects = new List<CardEffect>(),
                dimension = Dimension.Warp,
                produces = Array.Empty<string>(),
                consumes = Array.Empty<string>(),
                deviation = "standard",
                balanceNote = "墨漬：0費無效果佔手牌位",
                wordData = new WordData
                {
                    id = InkStainCardId,
                    english = "ink stain",
                    chinese = "墨漬",
                    partOfSpeech = "n.",
                    element = Element.Abstract,
                    tribe = "Status",
                    keywords = Array.Empty<string>(),
                    exampleSentences = Array.Empty<string>(),
                    confusables = Array.Empty<string>(),
                    audioPath = "",
                    rarity = Rarity.Common,
                    difficulty = 1
                },
                progress = new WordProgress
                {
                    wordId = InkStainCardId,
                    level = ProficiencyLevel.Internalized
                }
            };
        }

        /// <summary>
        /// 初始化資源媒介協同設定
        /// </summary>
        private void InitializeResourceMediator()
        {
            var config = GameManager.Instance?.dataManager?.GetSynergyConfig();
            resourceConfig = config?.resourceMediator ?? new ResourceMediatorConfig();

            resourceEffectWhitelist.Clear();
            if (resourceConfig.applyToEffects != null && resourceConfig.applyToEffects.Length > 0)
            {
                foreach (var name in resourceConfig.applyToEffects)
                {
                    if (Enum.TryParse(name, out CardEffectType effectType))
                    {
                        resourceEffectWhitelist.Add(effectType);
                    }
                }
            }
            else
            {
                resourceEffectWhitelist.Add(CardEffectType.Damage);
                resourceEffectWhitelist.Add(CardEffectType.Block);
                resourceEffectWhitelist.Add(CardEffectType.Heal);
                resourceEffectWhitelist.Add(CardEffectType.DrawCard);
                resourceEffectWhitelist.Add(CardEffectType.GainEnergy);
            }
        }

        /// <summary>
        /// 回合開始重置資源池
        /// </summary>
        private void ResetResourcePool()
        {
            resourcePool.Clear();
        }

        /// <summary>
        /// 消費資源媒介並回傳本張卡的倍率加成
        /// </summary>
        private float ConsumeResources(CardData card)
        {
            if (resourceConfig == null) InitializeResourceMediator();
            if (card.consumes == null || card.consumes.Length == 0) return 0f;

            if (resourceConfig.maxTokensPerTag <= 0 || resourceConfig.bonusPerToken <= 0f)
            {
                return 0f;
            }

            float bonus = 0f;
            foreach (var tag in card.consumes)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;
                if (!resourcePool.TryGetValue(tag, out int available)) continue;
                if (available <= 0) continue;

                int consumeCount = Mathf.Min(available, resourceConfig.maxTokensPerTag);
                resourcePool[tag] = available - consumeCount;
                bonus += consumeCount * resourceConfig.bonusPerToken;

                if (bonus >= resourceConfig.maxTotalBonus && resourceConfig.maxTotalBonus > 0f)
                {
                    bonus = resourceConfig.maxTotalBonus;
                    break;
                }
            }

            return bonus;
        }

        /// <summary>
        /// 產出資源媒介（每回合有上限）
        /// </summary>
        private void ProduceResources(CardData card)
        {
            if (resourceConfig == null) InitializeResourceMediator();
            if (card.produces == null || card.produces.Length == 0) return;
            if (resourceConfig.maxTokensPerTag <= 0) return;

            foreach (var tag in card.produces)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;
                resourcePool.TryGetValue(tag, out int current);
                int next = Mathf.Min(current + 1, resourceConfig.maxTokensPerTag);
                resourcePool[tag] = next;
            }
        }

        private bool ShouldApplyResourceBonus(CardEffectType effectType)
        {
            return resourceEffectWhitelist.Contains(effectType);
        }

        /// <summary>
        /// 結束玩家回合
        /// </summary>
        public void EndPlayerTurn()
        {
            if (currentState != CombatState.PlayerTurn) return;

            // 狂亂姿態：回合結束棄 2 牌
            if (player.currentStance == Stance.Frenzy && hand.Count > 0)
            {
                for (int i = 0; i < 2 && hand.Count > 0; i++)
                {
                    int idx = UnityEngine.Random.Range(0, hand.Count);
                    var card = hand[idx];
                    hand.RemoveAt(idx);
                    DiscardCard(card);
                }
            }

            // 手牌進入棄牌堆
            foreach (var card in hand.ToList())
            {
                DiscardCard(card);
            }
            hand.Clear();

            // 處理玩家回合結束效果
            player.ProcessTurnEnd();

            // 知識共振：洞察獎勵
            ResolveInsightReward();

            OnTurnEnd?.Invoke();

            // 敵人回合
            StartEnemyTurn();
        }

        /// <summary>
        /// 敵人回合
        /// </summary>
        private void StartEnemyTurn()
        {
            currentState = CombatState.EnemyTurn;
            OnCombatStateChanged?.Invoke(currentState);

            foreach (var enemy in enemies.Where(e => e.entity.IsAlive))
            {
                if (IsInkBlob(enemy))
                {
                    AddInkStainToDiscard();
                }

                // 處理敵人回合開始效果
                enemy.entity.ProcessTurnStart();

                // 執行敵人行動
                if (IsSleepingGargoyle(enemy))
                {
                    ApplyGargoyleSleepTick(enemy);
                }
                else
                {
                    ExecuteEnemyAction(enemy);
                }

                // 決定下一回合行動
                enemy.DecideNextAction();

                // 處理敵人回合結束效果
                enemy.entity.ProcessTurnEnd();
            }

            // 檢查戰鬥結束
            if (CheckCombatEnd()) return;

            // 開始下一個玩家回合
            StartPlayerTurn();
        }

        /// <summary>
        /// 執行敵人行動
        /// </summary>
        private void ExecuteEnemyAction(EnemyInstance enemy)
        {
            var action = enemy.currentAction;
            if (action == null) return;

            // 冰凍狀態跳過行動
            if (enemy.entity.statusEffects.ContainsKey(StatusEffectType.Frozen))
            {
                enemy.entity.statusEffects.Remove(StatusEffectType.Frozen);
                return;
            }

            int value = action.value;
            value += GetEnemyStrengthBonus(enemy);

            // 攻擊姿態：玩家受傷 +25%
            if (player.currentStance == Stance.Offensive && action.intent == EnemyIntent.Attack)
            {
                value = Mathf.RoundToInt(value * 1.25f);
            }

            switch (action.intent)
            {
                case EnemyIntent.Attack:
                    ExecuteEnemyAttack(enemy, value);
                    break;

                case EnemyIntent.Defend:
                    enemy.entity.AddBlock(value);
                    break;

                case EnemyIntent.Buff:
                    enemy.entity.ApplyStatus(StatusEffectType.Strength, value, 999);
                    break;

                case EnemyIntent.Debuff:
                    // 對玩家施加負面狀態
                    if (action.statusEffect.HasValue)
                    {
                        player.ApplyStatus(action.statusEffect.Value, value, action.statusDuration);
                    }
                    break;

                case EnemyIntent.AttackDebuff:
                    ExecuteEnemyAttack(enemy, value);
                    if (action.statusEffect.HasValue)
                    {
                        player.ApplyStatus(action.statusEffect.Value, value, action.statusDuration);
                    }
                    break;
            }
        }

        /// <summary>
        /// 檢查戰鬥結束
        /// </summary>
        private bool CheckCombatEnd()
        {
            // 玩家死亡
            if (!player.IsAlive)
            {
                if (!relicDisUsed && HasRelic("relic_dis"))
                {
                    var effect = GetRelicEffect("relic_dis");
                    if (effect != null && effect.type == RelicEffectType.SurviveOnce)
                    {
                        player.currentHp = Mathf.Max(1, effect.intValue);
                        relicDisUsed = true;
                        return false;
                    }
                }

                currentState = CombatState.Defeat;
                OnCombatStateChanged?.Invoke(currentState);
                OnCombatEnd?.Invoke(false);
                return true;
            }

            // 所有敵人死亡
            if (enemies.All(e => !e.entity.IsAlive))
            {
                currentState = CombatState.Victory;
                OnCombatStateChanged?.Invoke(currentState);
                OnCombatEnd?.Invoke(true);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 戰鬥狀態
    /// </summary>
        public enum CombatState
    {
        NotInCombat,
        PlayerTurn,
        AnsweringQuiz,
        EnemyTurn,
        Victory,
        Defeat
    }

    /// <summary>
    /// 洞察獎勵選項
    /// </summary>
    public enum InsightRewardOption
    {
        Damage,
        Block,
        Draw
    }

    /// <summary>
    /// 敵人實例（戰鬥中）
    /// </summary>
    public class EnemyInstance
    {
        public EnemyData data;
        public CombatEntity entity;
        public EnemyAction currentAction;
        public int sleepTurnsRemaining;
        public List<EnemyAction> previewActions = new List<EnemyAction>();

        public EnemyInstance(EnemyData data)
        {
            this.data = data;
            entity = new CombatEntity
            {
                maxHp = data.maxHp,
                currentHp = data.maxHp
            };
            sleepTurnsRemaining = data != null && data.id == "elite_gargoyle" ? 3 : 0;
        }

        public void DecideNextAction()
        {
            if (data.actions.Count == 0) return;
            currentAction = RollAction();
        }

        public void PreparePreview(int count)
        {
            previewActions.Clear();
            if (data.actions.Count == 0) return;

            for (int i = 0; i < count; i++)
            {
                previewActions.Add(RollAction());
            }
        }

        private EnemyAction RollAction()
        {
            int totalWeight = data.actions.Sum(a => a.weight);
            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;

            foreach (var action in data.actions)
            {
                cumulative += action.weight;
                if (roll < cumulative)
                {
                    return action;
                }
            }

            return data.actions[0];
        }
    }
}
