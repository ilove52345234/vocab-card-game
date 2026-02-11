# 實作依賴關係與進度報告

*建立日期：2026-02-11*
*最後更新：2026-02-11（完成順序 1-9，下一步為順序 10：詞族進化）*
*本文件追蹤所有系統的依賴關係和實作進度。每次開發 session 結束時更新。*

---

## 一、依賴關係鏈

```
Layer 0 ─ 基礎定義
  │  Enums、資料結構（WordData/CardData/CombatData）、元素、詞性
  │
Layer 1 ─ 數值基準
  │  標準值平衡體系、維度分配規則
  │  ⛔ 依賴 Layer 0
  │
Layer 2 ─ 核心機制
  │  卡牌效果執行、答題系統、SM-2 熟練度、資源媒介標籤讀取
  │  ⛔ 依賴 Layer 0-1
  │
Layer 3 ─ 戰鬥循環
  │  回合流程、敵人AI、狀態效果、元素弱點/抗性計算
  │  ⛔ 依賴 Layer 2
  │
Layer 4 ─ 協同系統
  │  4a. 資源媒介協同（produces/consumes 運行時追蹤）
  │  4b. 湧現式三軸協同（維度連鎖/元素共鳴/知識共振）
  │  ⛔ 依賴 Layer 2-3
  │
Layer 5 ─ 內容填充
  │  普通敵人完整化、精英怪特殊機制、Boss 多階段
  │  ⛔ 依賴 Layer 3-4
  │
Layer 6 ─ 遺物系統
  │  18 個遺物的效果觸發（字首觸發型/字尾增幅型/字根基石型+詞庫連動）
  │  ⛔ 依賴 Layer 3-4
  │
Layer 7 ─ 地圖系統
  │  路線生成演算法、7 種房間、收束發散、三章節遞進
  │  ⛔ 依賴 Layer 5-6（所有房間內容系統須存在）
  │
Layer 8 ─ 場所系統
  │  8a. 休息站（三選一：回血/升級答題/學新字 + Lv.5 進化入口）
  │  8b. 書房（LP 貨幣、預習/暫放/進化/深化/筆記五項服務）
  │  ⛔ 依賴 Layer 7（地圖中的房間節點）+ Layer 4（知識共振等協同）
  │
Layer 9 ─ 詞族進化
  │  Lv.5 進化三叉選擇、同字深化、詞族數據建立
  │  ⛔ 依賴 Layer 8（休息站/書房為進化場所）+ 熟練度系統完整運作
  │
Layer 10 ─ UI 全套
  │  主選單、地圖畫面、戰鬥畫面、答題畫面、休息站畫面、書房畫面、遺物欄
  │  ⛔ 依賴所有底層系統
```

---

## 二、實作優先順序

| 順序 | Layer | 系統 | 依賴 | 預估規模 | 關鍵產出 |
|------|-------|------|------|---------|---------|
| **1** | 2 | CardData 讀取新欄位 | Layer 0-1 ✅ | 小 | `produces`/`consumes`/`deviation`/`dimension` 從 JSON 正確載入 |
| **2** | 3 | 元素弱點/抗性計算 | Layer 2 | 小 | 傷害計算納入 weakness/resistance 倍率 |
| **3** | 4a | 資源媒介協同 | Layer 2 ✅ | 中 | 運行時追蹤 produces/consumes 標籤，觸發協同效果 |
| **4** | 4b | 湧現式三軸協同 | Layer 3 ✅ | 大 | 維度連鎖、元素共鳴、知識共振的完整實作 |
| **5** | 5 | 精英怪特殊機制 | Layer 3-4 ✅ | 中 | 書蟲/石像鬼/墨水怪的獨特 AI 邏輯 |
| **6** | 6 | 遺物效果實作 | Layer 3-4 ✅ | 中 | 18 個遺物的觸發/增幅/基石效果 + 詞庫連動 |
| **7** | 7 | 地圖生成 | Layer 5-6 ✅ | 大 | 路線生成演算法、7 種房間分佈、9 條硬性規則 |
| **8** | 8a | 休息站系統 | Layer 7 ✅ | 中 | 三選一 + 升級答題流程 + 進化入口 |
| **9** | 8b | 書房系統 | Layer 7 ✅ | 中 | LP 貨幣追蹤 + 5 項服務邏輯 |
| **10** | 9 | 詞族進化 | Layer 8 | 大 | 詞族數據建立 + 進化/深化邏輯 + 新卡生成 |
| **11** | 10 | UI 全套 | 全部 | 大 | 地圖/休息站/書房/遺物欄/主選單畫面 |

---

## 三、當前進度

### 總覽

| 面向 | 進度 | 說明 |
|------|------|------|
| 設計文檔 | █████████░ 95% | 8 份設計文檔 + 規則文件全部完成，僅缺詞族進化的具體數據表 |
| 數據定義 | ██████░░░░ 60% | words/cards/enemies/relics JSON 齊全，缺詞族數據 |
| 程式架構 | █████░░░░░ 45% | 核心管理器+新資料結構（Dimension/EnemyCategory/RelicMorphType） |
| 系統實作 | ██░░░░░░░░ 25% | +元素弱點/抗性計算完成，協同/地圖/遺物/場所待實作 |
| UI 實作 | █░░░░░░░░░ 10% | 僅戰鬥+答題基礎 UI |
| 可遊玩度 | █░░░░░░░░░ 10% | 能打一場固定戰鬥+答題，無地圖無 run 循環 |

### 各系統細部狀態

| 系統 | 設計 | 數據 | 程式碼 | 說明 |
|------|------|------|--------|------|
| **Enums/資料結構** | ✅ | ✅ | ✅ | +Dimension/EnemyCategory/RelicMorphType enums |
| **標準值平衡** | ✅ | ✅ | ✅ | cards.json 已校準；CardData 讀取 deviation/produces/consumes/dimension |
| **維度系統** | ✅ | ✅ | ⚠️ 部分 | Dimension enum + CardData.dimension 已載入；協同計算待實作 |
| **SM-2 熟練度** | ✅ | ✅ | ✅ | WordProgress + LearningManager 完整實作 |
| **答題系統** | ✅ | ✅ | ✅ | QuizManager 支援識讀/聽力/拼字，缺音訊實作 |
| **戰鬥核心** | ✅ | ✅ | ✅ | CombatManager 745 行，回合流程/出牌/敵人AI |
| **卡牌效果** | ✅ | ✅ | ⚠️ 60% | 基礎效果（傷害/格擋/回血/抽牌/狀態）已寫；資源媒介未實作 |
| **狀態效果** | ✅ | ✅ | ⚠️ 70% | Burning/Poisoned/Bleeding 已寫；4 種狀態互動已寫 |
| **元素弱點/抗性** | ✅ | ✅ | ✅ | 弱點 ×1.5、抗性 ×0.5，透過卡牌元素傳遞計算 |
| **資源媒介協同** | ✅ | ✅ | ✅ | 回合資源池 + consumes/produces 倍率加成已實作 |
| **維度連鎖** | ✅ | ✅ | ✅ | 回合內連鎖倍率與覆蓋獎勵已實作 |
| **元素共鳴** | ✅ | ✅ | ✅ | 同元素 2/3 張觸發小效果已實作 |
| **知識共振** | ✅ | ✅ | ✅ | 洞察與低熟練卡加成已實作 |
| **普通敵人** | ✅ | ✅ | ⚠️ 部分 | 5 隻普通敵 JSON 齊全；AI 邏輯基礎版 |
| **精英怪** | ✅ | ✅ | ✅ | 書蟲/石像鬼/墨水怪特殊機制已實作 |
| **Boss** | ✅ | ✅ | ❌ | 1 隻 Boss JSON 有數據；多階段未實作 |
| **遺物效果** | ✅ | ✅ | ✅ | 戰鬥/答題遺物效果已實作；休息站相關待系統完成 |
| **地圖生成** | ✅ | ✅ | ✅ | map_config 驅動規則與房間分佈，地圖節點與連線已生成 |
| **休息站** | ✅ | ✅ | ✅ | 三選一流程完成；升級答題/學新字可觸發 |
| **書房** | ✅ | ✅ | ✅ | LP 設定/服務流程完成，支援預習/暫放/進化/深化/筆記 |
| **詞族進化** | ✅ | ❌ | ❌ | 設計完成；缺詞族數據表；零程式碼 |
| **戰鬥 UI** | — | — | ⚠️ 基礎 | CombatUIController 279 行，手牌/能量/HP 基礎版 |
| **答題 UI** | — | — | ⚠️ 基礎 | QuizUIController 220 行，選項/計時/拼字基礎版 |
| **地圖 UI** | — | — | ❌ | 零程式碼 |
| **休息站 UI** | — | — | ❌ | 零程式碼 |
| **書房 UI** | — | — | ❌ | 零程式碼 |
| **主選單 UI** | — | — | ❌ | 零程式碼 |

### 關鍵缺口

**最大瓶頸：Layer 4（協同系統）**
- 沒有它，戰鬥只是「打卡出牌」，沒有構築深度
- 它是讓遊戲「上頭」的核心，也是 Layer 5-9 所有系統的前置依賴
- 建議下一步開發從 Layer 2 補完 → Layer 3 補完 → Layer 4 全新實作

---

## 五、開發工具與自動化（導入計劃）

### Unity MCP（工具導入）
- **推薦方案**：CoplayDev/unity-mcp（MIT，支援 Unity 2021.3+）
- **導入時機**：核心系統與資料結構已穩定；後續進入大量 UI/內容製作與 Editor 重複操作階段
- **預期用途**：批量建立/調整 UI、Prefab 與場景物件；執行 Editor MenuItem；批次修正資料與資產；輔助回歸測試

---

## 四、程式碼檔案索引

### 現有檔案

| 路徑 | 行數 | 職責 |
|------|------|------|
| `Assets/Scripts/Core/GameManager.cs` | ~200 | 遊戲模式切換、XP/成就管理 |
| `Assets/Scripts/Core/DataManager.cs` | ~200 | JSON 載入/儲存、玩家數據持久化 |
| `Assets/Scripts/Core/AudioManager.cs` | ~100 | SFX/音樂/語音播放 |
| `Assets/Scripts/Core/GameBootstrap.cs` | ~50 | 管理器階層自動建立 |
| `Assets/Scripts/Data/Enums.cs` | ~120 | 所有列舉（Element/CardType/ProficiencyLevel 等） |
| `Assets/Scripts/Data/WordData.cs` | ~110 | 單字資料 + SM-2 學習進度 |
| `Assets/Scripts/Data/CardData.cs` | ~130 | 卡牌/效果/遺物資料結構 |
| `Assets/Scripts/Data/CombatData.cs` | ~70 | 玩家屬性/敵人/戰鬥實體 |
| `Assets/Scripts/Combat/CombatManager.cs` | ~745 | 戰鬥核心：回合/出牌/敵人AI/狀態 |
| `Assets/Scripts/Learning/LearningManager.cs` | ~350 | 學習進度/每日統計/解鎖邏輯 |
| `Assets/Scripts/Learning/QuizManager.cs` | ~275 | 答題流程/選項生成/SM-2 品質計算 |
| `Assets/Scripts/UI/CombatUIController.cs` | ~279 | 戰鬥畫面 UI |
| `Assets/Scripts/UI/QuizUIController.cs` | ~220 | 答題畫面 UI |
| `Assets/Scripts/UI/EnemyView.cs` | ~67 | 敵人顯示元件 |
| `Assets/Scripts/Editor/CLITestRunner.cs` | ~50 | 編輯器測試框架 |
| **合計** | **~3,600** | |

### 數據檔案

| 路徑 | 內容 |
|------|------|
| `Assets/Resources/Data/words.json` | 31 個單字（含 phonetic/element/tribe/confusables） |
| `Assets/Resources/Data/cards.json` | 30 張卡牌（含 effects/produces/consumes/dimension） |
| `Assets/Resources/Data/enemies.json` | 9 個敵人（5 普通 + 3 精英 + 1 Boss） |
| `Assets/Resources/Data/relics.json` | 18 個遺物（6 字首 + 6 字尾 + 6 字根） |

### 設計文檔

| 路徑 | 內容 |
|------|------|
| `docs/DESIGN_RULES.md` | 10 條絕對規則 + 10 條通用規則 |
| `docs/plans/2026-02-06-game-design.md` | 主設計文檔 v1.5（812 行） |
| `docs/plans/2026-02-11-standard-value-balance.md` | 標準值平衡體系 |
| `docs/plans/2026-02-11-map-system-design.md` | 地圖系統（7 房間 + 9 規則） |
| `docs/plans/2026-02-11-dimension-and-elite-design.md` | 四維度 + 精英怪 |
| `docs/plans/2026-02-11-relic-system-design.md` | 遺物系統 v2（三層構詞學） |
| `docs/plans/2026-02-11-card-upgrade-system.md` | 詞族進化系統 |
| `docs/plans/2026-02-11-study-room-design.md` | 書房系統 |
| `docs/plans/2026-02-11-synergy-system-design.md` | 湧現式協同（三軸） |

---

*每次開發 session 結束時，更新「三、當前進度」中的狀態欄位和百分比。*
