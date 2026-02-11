# 四維度卡牌系統與精英怪設計

*建立日期：2026-02-11*

---

## 一、四維度系統

### 設計理念

將英文詞性（Part of Speech）對應戰鬥維度，讓學英文與理解卡牌功能合一。

### 維度定義

| 維度 | 英文 | 對應詞性 | 預估比例 | 戰鬥角色 |
|------|------|---------|---------|---------|
| **攻擊** | Strike | 動詞（動作類）+ 有生/武器名詞 | ~30% | 造成傷害、施加負面狀態 |
| **防禦** | Guard | 物品/建築/食物/植物名詞 | ~25% | 格擋、回血、生存 |
| **增幅** | Boost | 形容詞 + 副詞 + 感官/心智動詞 | ~28% | 增益、弱化、抽牌 |
| **操控** | Warp | 抽象名詞 + 數詞 | ~17% | 能量、費用、牌組操控 |

### 詞性→維度映射規則

**動詞 (Verb)：**
- 動作/戰鬥動詞 → Strike（attack, strike, run, jump, charge）
- 心智/感官動詞 → Boost（think, listen）
- 防護動詞 → Guard（defend）

**名詞 (Noun)：** 依 Tribe 語意分類
- Animals, Nature（有能量的） → Strike（wolf, cat, fire）
- Food, Plants → Guard（apple, bread, tree, flower）
- Weapons, Tools → Strike（sword, hammer）
- Shields, Armor, Buildings → Guard（shield, armor, wall）
- 保護性自然 → Guard（water）
- Emotions, Senses, Communication → Boost
- Numbers, Time, Shapes, Concepts → Warp

**形容詞 (Adjective)：** → Boost（happy, calm, sad）

**副詞 (Adverb)：** → Boost

**數詞 (Number)：** → Warp（one, two, three）

### 擴展到 10,000 字的預估分佈

| 詞性 | 佔總詞彙 | 主要維度 | 維度比例 |
|------|---------|---------|---------|
| 動詞 ~14% | → Strike ~10%, Guard ~2%, Boost ~2% | Strike為主 |
| 名詞 ~55% | → Strike ~15%, Guard ~25%, Warp ~15% | 依語意拆分 |
| 形容詞 ~23% | → Boost ~23% | Boost 為主 |
| 副詞 ~6% | → Boost ~5%, 其他 ~1% | Boost 為主 |
| 數詞 ~2% | → Warp ~2% | Warp |

**最終維度比例：Strike ~25%, Guard ~27%, Boost ~30%, Warp ~18%**

### 現有 30 張卡牌維度標籤

**Strike 攻擊（10 張）：**
| 卡牌 | 詞性 | 映射理由 |
|------|------|---------|
| attack | v. (Actions) | 動作動詞 |
| strike | v. (Actions) | 動作動詞 |
| fire | n. (Nature) | 有能量的自然名詞 |
| charge | v. (Actions) | 動作動詞 |
| run | v. (Actions) | 動作動詞 |
| jump | v. (Actions) | 動作動詞 |
| wolf | n. (Animals) | 有生名詞 |
| cat | n. (Animals) | 有生名詞 |
| sword | n. (Objects) | 武器名詞 |
| hammer | n. (Tools) | 武器/工具名詞 |

**Guard 防禦（9 張）：**
| 卡牌 | 詞性 | 映射理由 |
|------|------|---------|
| defend | v. (Actions) | 防護動詞 |
| shield | n. (Objects) | 防具名詞 |
| armor | n. (Objects) | 防具名詞 |
| wall | n. (Buildings) | 建築名詞 |
| water | n. (Nature) | 保護性自然名詞 |
| apple | n. (Food) | 食物名詞 |
| bread | n. (Food) | 食物名詞 |
| tree | n. (Plants) | 植物名詞 |
| flower | n. (Plants) | 植物名詞 |

**Boost 增幅（5 張）：**
| 卡牌 | 詞性 | 映射理由 |
|------|------|---------|
| happy | adj. (Emotions) | 形容詞 |
| calm | adj. (Emotions) | 形容詞 |
| sad | adj. (Emotions) | 形容詞 |
| think | v. (Communication) | 心智動詞 |
| listen | v. (Senses) | 感官動詞 |

**Warp 操控（6 張）：**
| 卡牌 | 詞性 | 映射理由 |
|------|------|---------|
| one | num. (Numbers) | 數詞 |
| two | num. (Numbers) | 數詞 |
| three | num. (Numbers) | 數詞 |
| time | n. (Time) | 抽象名詞 |
| circle | n. (Shapes) | 抽象名詞 |
| square | n. (Shapes) | 抽象名詞 |

**分佈：** Strike 33% / Guard 30% / Boost 17% / Warp 20%

---

## 二、精英怪設計

### 設計哲學

每章 3 隻精英，各測試一個維度（第四維度 Warp 融入各精英的次要機制）：
- **精英 A**：測試 Strike（你的攻擊能力不足就會被拖死）
- **精英 B**：測試 Guard（你的防禦不足就會被秒殺）
- **精英 C**：測試 Boost（你的成長不足就會被汙染/壓制）

答題能力融入每隻精英的機制中，不單獨測試。

### 第一章精英

#### 書蟲 Bookworm（測試 Strike）
- **HP：** 35
- **機制：** 你每使用非攻擊卡（技能/能力），牠 +1 力量
- **答題融入：** 答錯時牠額外 +1 力量
- **意圖模式：** 攻擊(8傷, 權重3) / 格擋(6擋, 權重1)
- **考驗：** 逼你用攻擊卡快速輸出，不能慢慢成長
- **Element：** Mind (2)
- **Weakness：** Force (1)

#### 石像鬼 Gargoyle（測試 Guard）
- **HP：** 50
- **機制：** 前 3 回合沉睡（不行動但每回合 +3 力量），醒後每回合 15×2 傷害
- **答題融入：** 沉睡期間答對可「削弱」牠（每次答對 -1 累積力量）
- **意圖模式：** 沉睡時(無行動) / 醒後：重擊(15傷×2, 權重3) / 格擋(8擋, 權重1)
- **考驗：** 前 3 回合必須準備足夠防禦或在醒前擊殺
- **Element：** Matter (3)
- **Weakness：** Life (0)

#### 墨水怪 Ink Blob（測試 Boost）
- **HP：** 40
- **機制：** 每回合往你棄牌堆加 1 張「墨漬」廢牌（0費，無效果，佔手牌位）
- **答題融入：** 手中有「墨漬」時答對可消除它
- **意圖模式：** 攻擊(6傷, 權重2) / 加墨漬(權重2) / 格擋(5擋, 權重1)
- **考驗：** 需要抽牌/棄牌能力維持牌組品質，成長維度不足會被汙染拖死
- **Element：** Abstract (4)
- **Weakness：** Force (1)

### 第二章精英（測試雙維度）

#### 鏡像師 Mirror Mage（Strike + Boost）
- **HP：** 65
- **機制：** 複製你上回合打出的最後一張攻擊卡，對你使用
- **答題融入：** 答對時你的卡牌效果 +20%，拉開與鏡像的差距
- **考驗：** 需要攻擊輸出，但也需要增幅來壓過鏡像

#### 鐵甲龜 Iron Turtle（Guard + Strike）
- **HP：** 55
- **機制：** 每回合自動獲得 10 格擋 + 反擊（你攻擊牠的格擋時受到等量傷害）
- **答題融入：** 答對時無視 30% 格擋
- **考驗：** 需要足夠防禦承受反擊，且需要高傷害穿透格擋

#### 混沌蛇 Chaos Serpent（Boost + Guard）
- **HP：** 60
- **機制：** 每 2 回合隨機改變自身弱點元素
- **答題融入：** 答對時揭示下一次弱點變化
- **考驗：** 需要多元化牌組（成長維度）+ 足夠防禦應對不確定性

### 第三章精英

留待第三章開發時細化。基本原則：三機制交替，測試全維度。

### 精英獎勵

| 章節 | 遺物 | 卡牌獎勵 |
|------|------|---------|
| 第一章 | Common 遺物 | 3選1（含 Uncommon） |
| 第二章 | Uncommon 遺物 | 3選1（含 Rare） |
| 第三章 | Rare 遺物 | 3選1（含 Rare+） |

---

*本文件為精英怪與維度系統的設計依據。*
