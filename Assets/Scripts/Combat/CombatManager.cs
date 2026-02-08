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
            if (currentEnergy < card.energyCost) return;
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
                    OnQuizAnswered(card, target, isCorrect, quality);
                });
            }
            else
            {
                // 直接執行卡牌效果
                ExecuteCard(card, target, 1f);
            }
        }

        /// <summary>
        /// 答題結果回呼
        /// </summary>
        private void OnQuizAnswered(CardData card, EnemyInstance target, bool isCorrect, int quality)
        {
            currentState = CombatState.PlayerTurn;

            // 更新學習進度
            card.progress?.UpdateProgress(isCorrect, quality);
            GameManager.Instance.learningManager.OnAnswerResult(isCorrect);

            if (isCorrect)
            {
                correctAnswersThisTurn++;
                GameManager.Instance.AddExperience(GetExpForQuizMode(card.GetQuizMode()));

                // 執行卡牌效果
                ExecuteCard(card, target, card.GetEffectMultiplier());

                // 檢查專注姿態
                CheckFocusedStance();
            }
            else
            {
                // 答錯處理
                float penalty = GameManager.Instance.playerData.GetGamePhase() == GamePhase.Tutorial
                    ? 1f  // 教學期不懲罰
                    : GameManager.Instance.playerData.GetGamePhase() == GamePhase.Beginner
                        ? 0.7f  // 初級期輕微懲罰
                        : GetWrongAnswerPenalty(card.progress?.level ?? ProficiencyLevel.New);

                if (penalty > 0)
                {
                    ExecuteCard(card, target, penalty);
                }
                else
                {
                    // 卡牌失效，只扣能量
                    currentEnergy -= card.energyCost;
                    hand.Remove(card);
                    discardPile.Add(card);
                }

                // 離開專注姿態
                if (player.currentStance == Stance.Focused)
                {
                    ChangeStance(Stance.None);
                }
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
        private void ExecuteCard(CardData card, EnemyInstance target, float multiplier)
        {
            currentEnergy -= card.energyCost;
            hand.Remove(card);

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

            // 執行效果
            foreach (var effect in card.effects)
            {
                ExecuteEffect(effect, target, multiplier);
            }

            // 檢查姿態觸發
            CheckStanceTriggers(card);

            // 檢查 Combo
            CheckComboTriggers();

            // 檢查連鎖
            CheckChainTriggers(card);

            // 卡牌進入棄牌堆或消耗堆
            if (card.effects.Any(e => e.type == CardEffectType.Exhaust))
            {
                exhaustPile.Add(card);
            }
            else
            {
                discardPile.Add(card);
            }

            OnCardPlayed?.Invoke(card);

            // 檢查戰鬥結束
            CheckCombatEnd();
        }

        /// <summary>
        /// 執行單個效果
        /// </summary>
        private void ExecuteEffect(CardEffect effect, EnemyInstance target, float multiplier)
        {
            int value = Mathf.RoundToInt(effect.value * multiplier);

            // 計算姿態加成
            value = ApplyStanceModifier(effect.type, value);

            // 計算元素相剋
            if (target != null)
            {
                value = ApplyElementModifier(effect.type, value, target);
            }

            switch (effect.type)
            {
                case CardEffectType.Damage:
                    if (target != null)
                    {
                        target.entity.TakeDamage(value);
                        OnEnemyDamaged?.Invoke(target, value);
                    }
                    break;

                case CardEffectType.DamageAll:
                    foreach (var enemy in enemies.Where(e => e.entity.IsAlive))
                    {
                        int dmg = ApplyElementModifier(effect.type, value, enemy);
                        enemy.entity.TakeDamage(dmg);
                        OnEnemyDamaged?.Invoke(enemy, dmg);
                    }
                    break;

                case CardEffectType.Block:
                    player.AddBlock(value);
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

        private int ApplyElementModifier(CardEffectType effectType, int value, EnemyInstance enemy)
        {
            if (effectType != CardEffectType.Damage && effectType != CardEffectType.DamageAll)
                return value;

            // TODO: 取得卡牌元素，計算相剋
            // 暫時返回原值
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
                    discardPile.Add(hand[idx]);
                    hand.RemoveAt(idx);
                }
            }

            // 手牌進入棄牌堆
            discardPile.AddRange(hand);
            hand.Clear();

            // 處理玩家回合結束效果
            player.ProcessTurnEnd();

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
                // 處理敵人回合開始效果
                enemy.entity.ProcessTurnStart();

                // 執行敵人行動
                ExecuteEnemyAction(enemy);

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

            // 攻擊姿態：玩家受傷 +25%
            if (player.currentStance == Stance.Offensive && action.intent == EnemyIntent.Attack)
            {
                value = Mathf.RoundToInt(value * 1.25f);
            }

            switch (action.intent)
            {
                case EnemyIntent.Attack:
                    player.TakeDamage(value);
                    OnPlayerDamaged?.Invoke(value);
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
                    player.TakeDamage(value);
                    OnPlayerDamaged?.Invoke(value);
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
    /// 敵人實例（戰鬥中）
    /// </summary>
    public class EnemyInstance
    {
        public EnemyData data;
        public CombatEntity entity;
        public EnemyAction currentAction;

        public EnemyInstance(EnemyData data)
        {
            this.data = data;
            entity = new CombatEntity
            {
                maxHp = data.maxHp,
                currentHp = data.maxHp
            };
        }

        public void DecideNextAction()
        {
            if (data.actions.Count == 0) return;

            // 根據權重隨機選擇
            int totalWeight = data.actions.Sum(a => a.weight);
            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;

            foreach (var action in data.actions)
            {
                cumulative += action.weight;
                if (roll < cumulative)
                {
                    currentAction = action;
                    return;
                }
            }

            currentAction = data.actions[0];
        }
    }
}
