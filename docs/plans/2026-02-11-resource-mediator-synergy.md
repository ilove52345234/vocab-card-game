# 資源媒介協同設計（produces/consumes）

*建立日期：2026-02-11*
*本文件遵循 `docs/DESIGN_RULES.md`，屬於三軸協同的互動管道（非新增協同軸）。*

---

## 一、設計目標

1. **讓牌組互動更有方向性**：透過資源媒介標籤（produces/consumes）建立「出牌順序」與「構築方向」的微弱但穩定收益。
2. **不新增第四協同軸**：資源媒介僅是互動管道，不獨立計分，不與三軸協同衝突。
3. **小而可控的收益**：協同只提供小幅倍率加成，避免壓過熟練度與三軸協同。
4. **全程資料驅動**：所有數值與上限由 JSON 設定，避免硬編碼。

---

## 二、規則對照（絕對規則）

- **A2 戰鬥力來源分佈**：資源媒介屬於「策略操作 35%」的小幅加成。
- **A3 熟練度即力量**：資源媒介只做「小倍率疊加」，主倍率仍由熟練度決定。
- **A5 關鍵詞協同**：只使用 `produces/consumes` 標籤，不出現卡對卡。
- **A6 三軸封頂**：資源媒介不新增新軸，僅作為互動管道。
- **G10 資料驅動**：所有數值來自 `Assets/Resources/Data/synergy_config.json`。

---

## 三、運作規則

### 3.1 資源池

- 每回合開始時清空資源池。
- 出牌時：
  1. **先消費** `consumes` 對應的資源
  2. **再產出** `produces` 對應的資源
- 目的：避免「同一張牌自產自用」造成無條件加成。

### 3.2 消費加成

- 每消費 1 個資源媒介 token，該卡主效果倍率 +`bonusPerToken`。
- 每個標籤單回合可累積的 token 上限為 `maxTokensPerTag`。
- 單張卡消費後的總加成上限為 `maxTotalBonus`。
- 加成僅作用於主效果類型（`applyToEffects`）：
  - Damage, Block, Heal, DrawCard, GainEnergy

### 3.3 不適用範圍

- 狀態效果（Burning/Frozen/Poisoned/Wet/Bleeding/Strength/Dexterity）不受資源媒介加成。
- 協同加成不取代熟練度倍率與答題倍率。

---

## 四、數值配置（資料驅動）

- 設定檔：`Assets/Resources/Data/synergy_config.json`
- 欄位：
  - `bonusPerToken`：每 token 的倍率加成（預設 0.10）
  - `maxTokensPerTag`：每標籤每回合最多可持有的 token（預設 2）
  - `maxTotalBonus`：單張卡最大加成（預設 0.30）
  - `applyToEffects`：可套用加成的效果類型

---

## 五、實作位置

- `Assets/Scripts/Data/SynergyConfig.cs`：資料結構
- `Assets/Scripts/Core/DataManager.cs`：載入設定
- `Assets/Scripts/Combat/CombatManager.cs`：回合資源池、消費與產出

---

## 六、規則檢查清單

```
□ A2 戰鬥力來源仍為 60/35/5
□ A3 熟練度仍為主倍率
□ A5 協同基於關鍵詞，不是卡對卡
□ A6 不新增協同軸
□ G10 數值皆由 JSON 載入
```
