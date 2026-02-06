using System;
using System.Collections.Generic;
using UnityEngine;

namespace VocabCardGame.Data
{
    /// <summary>
    /// 玩家資料
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        // 基礎屬性
        public string name = "Player";
        public int level = 1;
        public int experience = 0;
        public int experienceToNext = 500;

        // RPG 屬性點
        public int statPoints = 0;
        public int intelligence = 0;    // 🧠 +0.3秒答題時間
        public int strength = 0;        // 💪 +1%傷害
        public int constitution = 0;    // ❤️ +2 HP
        public int agility = 0;         // 🦶 每10點+1起始手牌
        public int luck = 0;            // 🍀 +0.5%稀有掉落

        // 戰鬥屬性（計算後）
        public int MaxHp => 80 + (constitution * 2);
        public int BaseEnergy => 3;
        public int StartingHandSize => 5 + (agility / 10);
        public float DamageMultiplier => 1f + (strength * 0.01f);
        public float QuizTimeBonus => intelligence * 0.3f;
        public float RareDropBonus => luck * 0.005f;

        // 進度資料
        public int gold = 0;
        public int highestAbyssFloor = 0;
        public int highestDifficulty = 0;
        public int totalWordsLearned = 0;
        public int totalCorrectAnswers = 0;
        public int consecutiveCorrect = 0;
        public int maxConsecutiveCorrect = 0;
        public DateTime firstPlayDate;
        public int totalPlayDays = 0;

        // 遺物
        public List<string> ownedRelics = new List<string>();
        public List<string> equippedRelics = new List<string>();

        // 成就
        public List<string> unlockedAchievements = new List<string>();

        /// <summary>
        /// 取得當前遊戲階段
        /// </summary>
        public GamePhase GetGamePhase()
        {
            if (totalPlayDays <= 2) return GamePhase.Tutorial;
            if (totalPlayDays <= 4) return GamePhase.Beginner;
            return GamePhase.Normal;
        }

        /// <summary>
        /// 增加經驗值
        /// </summary>
        public bool AddExperience(int amount)
        {
            experience += amount;
            if (experience >= experienceToNext)
            {
                experience -= experienceToNext;
                level++;
                statPoints++;
                experienceToNext = CalculateExpToNext(level);
                return true; // 升級了
            }
            return false;
        }

        private int CalculateExpToNext(int level)
        {
            if (level <= 10) return 500;
            if (level <= 20) return 1000;
            if (level <= 30) return 2000;
            if (level <= 50) return 3000;
            return 5000;
        }
    }

    /// <summary>
    /// 敵人資料
    /// </summary>
    [Serializable]
    public class EnemyData
    {
        public string id;
        public string name;
        public int maxHp;
        public Element element;
        public Sprite sprite;
        public List<EnemyAction> actions = new List<EnemyAction>();
        public List<string> modifiers = new List<string>(); // 詞綴

        // 元素弱點/抗性
        public Element? weakness;       // 弱點元素（受傷 +50%）
        public Element? resistance;     // 抗性元素（受傷 -50%）
    }

    /// <summary>
    /// 敵人行動
    /// </summary>
    [Serializable]
    public class EnemyAction
    {
        public EnemyIntent intent;
        public int value;               // 傷害值/格擋值/效果值
        public StatusEffectType? statusEffect;
        public int statusDuration;
        public int weight = 1;          // 選擇權重
    }

    /// <summary>
    /// 敵人詞綴（無盡深淵用）
    /// </summary>
    [Serializable]
    public class EnemyModifier
    {
        public string id;
        public string name;
        public string description;
        public ModifierType type;
        public float value;
    }

    public enum ModifierType
    {
        HpMultiplier,       // HP 倍率
        DamageMultiplier,   // 傷害倍率
        OnHitBurn,          // 攻擊附帶燃燒
        OnHitPoison,        // 攻擊附帶中毒
        Regeneration,       // 每回合回血
        Thorns,             // 反傷
        ExtraDamageOnWrong, // 玩家答錯時額外傷害
        HealOnCorrect       // 玩家答對時回血
    }

    /// <summary>
    /// 戰鬥中的角色狀態
    /// </summary>
    [Serializable]
    public class CombatEntity
    {
        public int currentHp;
        public int maxHp;
        public int block;
        public Stance currentStance = Stance.None;
        public Dictionary<StatusEffectType, StatusEffect> statusEffects = new Dictionary<StatusEffectType, StatusEffect>();

        public bool IsAlive => currentHp > 0;

        public void TakeDamage(int damage)
        {
            // 先扣格擋
            if (block > 0)
            {
                if (block >= damage)
                {
                    block -= damage;
                    damage = 0;
                }
                else
                {
                    damage -= block;
                    block = 0;
                }
            }

            // 再扣血量
            currentHp = Mathf.Max(0, currentHp - damage);
        }

        public void Heal(int amount)
        {
            currentHp = Mathf.Min(maxHp, currentHp + amount);
        }

        public void AddBlock(int amount)
        {
            block += amount;
        }

        public void ApplyStatus(StatusEffectType type, int value, int duration)
        {
            if (statusEffects.ContainsKey(type))
            {
                // 疊加效果
                statusEffects[type].value += value;
                statusEffects[type].duration = Mathf.Max(statusEffects[type].duration, duration);
            }
            else
            {
                statusEffects[type] = new StatusEffect { type = type, value = value, duration = duration };
            }
        }

        public void ProcessTurnStart()
        {
            // 處理回合開始的狀態效果
            foreach (var effect in statusEffects.Values)
            {
                switch (effect.type)
                {
                    case StatusEffectType.Burning:
                    case StatusEffectType.Poisoned:
                        TakeDamage(effect.value);
                        break;
                    case StatusEffectType.Regeneration:
                        Heal(effect.value);
                        break;
                }
            }
        }

        public void ProcessTurnEnd()
        {
            // 減少持續時間
            var toRemove = new List<StatusEffectType>();
            foreach (var kvp in statusEffects)
            {
                kvp.Value.duration--;
                if (kvp.Value.duration <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var key in toRemove)
            {
                statusEffects.Remove(key);
            }

            // 預設：回合結束格擋歸零（防禦姿態保留50%）
            if (currentStance == Stance.Defensive)
            {
                block = block / 2;
            }
            else
            {
                block = 0;
            }
        }
    }

    /// <summary>
    /// 狀態效果實例
    /// </summary>
    [Serializable]
    public class StatusEffect
    {
        public StatusEffectType type;
        public int value;
        public int duration;
    }
}
