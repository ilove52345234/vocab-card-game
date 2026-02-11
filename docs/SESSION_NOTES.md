# 開發對話記錄

## 專案概述

**英文單字卡牌肉鴿遊戲** - 結合殺戮尖塔機制與間隔重複學習

## 已完成設計

### 核心系統
- **5 大元素**：Life, Force, Mind, Matter, Abstract
- **7 級熟練度**：Lv.0-7，整合 SM-2 記憶曲線演算法
- **3 種答題模式**：識讀 → 聽力 → 拼字（漸進式難度）
- **4 種姿態**：攻擊、防禦、專注、狂亂

### 戰鬥力分佈
- 學習相關：60%（熟練度 30% + 協同 20% + Combo 10%）
- 策略操作：35%（姿態 10% + 狀態 8% + 能量 7% + 戰術 10%）
- 純數值：5%（屬性 3% + 遺物 2%）

### 遊戲模式
- 冒險模式（難度 0-20）
- 無盡深淵（無限層數）
- 每日挑戰

### 難度曲線
- 新手保護期（Day 1-7）
- 答題率：100% → 80% → 60% → 40% → 20% → 10% → 0%

## 技術選擇

| 項目 | 選擇 |
|------|------|
| 引擎 | Unity 2022.3 LTS (C#) |
| 美術 | AI 生成 + 後製 |
| 儲存 | SQLite + JSON |

## 已建立的程式架構

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs      # 遊戲主管理器
│   ├── DataManager.cs      # 資料讀寫
│   ├── AudioManager.cs     # 音訊管理
│   └── GameBootstrap.cs    # 場景初始化
├── Data/
│   ├── Enums.cs            # 列舉定義（含 Dimension/EnemyCategory/RelicMorphType）
│   ├── WordData.cs         # 單字資料結構
│   ├── CardData.cs         # 卡牌資料結構（含 dimension/produces/consumes）
│   └── CombatData.cs       # 戰鬥資料結構（含 EnemyCategory/testsDimension）
├── Combat/
│   └── CombatManager.cs    # 戰鬥邏輯（含元素弱點/抗性計算）
├── Learning/
│   ├── LearningManager.cs  # 學習進度管理
│   └── QuizManager.cs      # 答題邏輯
└── UI/
    ├── CombatUIController.cs
    ├── QuizUIController.cs
    └── EnemyView.cs
```

## 範例資料
- 31 個單字（words.json）
- 30 張卡牌（cards.json，含標準值校準 + 維度 + 資源媒介標籤）
- 9 個敵人（enemies.json，5 普通 + 3 精英 + 1 Boss）
- 18 個遺物（relics.json，6 字首 + 6 字尾 + 6 字根）

## 設計文檔

| 文件 | 內容 |
|------|------|
| `docs/DESIGN_RULES.md` | 10 絕對規則 + 10 通用規則 |
| `docs/IMPLEMENTATION_STATUS.md` | 依賴關係鏈 + 實作進度 |
| `docs/plans/2026-02-06-game-design.md` | 主設計文檔 v1.5 |
| `docs/plans/2026-02-11-standard-value-balance.md` | 標準值平衡體系 |
| `docs/plans/2026-02-11-map-system-design.md` | 地圖系統（7 房間 + 9 規則） |
| `docs/plans/2026-02-11-dimension-and-elite-design.md` | 四維度 + 精英怪 |
| `docs/plans/2026-02-11-relic-system-design.md` | 遺物系統 v2（三層構詞學） |
| `docs/plans/2026-02-11-card-upgrade-system.md` | 詞族進化系統 |
| `docs/plans/2026-02-11-study-room-design.md` | 書房系統 |
| `docs/plans/2026-02-11-synergy-system-design.md` | 湧現式協同（三軸） |

## 重要文件

- 設計規則：`docs/DESIGN_RULES.md`
- 實作進度：`docs/IMPLEMENTATION_STATUS.md`
- 主設計文檔：`docs/plans/2026-02-06-game-design.md`

## GitHub

- Repo: https://github.com/ilove52345234/vocab-card-game
- 帳號: ilove52345234

---

*最後更新：2026-02-11*

---

## 交接記錄（2026-02-08）

本次由新 agent 接手，釐清目前 repo 狀態並整理版本控管：
- 新增 `.gitignore`（Unity 標準忽略規則）
- 追蹤 Unity 專案必需檔案（`Assets/**/*.meta`、`ProjectSettings/*.asset`、`Packages/manifest.json`、`Packages/packages-lock.json`）
- 保持不追蹤 `Library/`, `Logs/`, `UserSettings/` 等自動生成目錄

---

## 交接記錄（2026-02-08）- MVP UI/資料補齊

- 擴充單字與卡牌資料至 30 張（`words.json`, `cards.json`）
- 修正卡牌效果 type 編號與程式列舉對齊
- `CombatManager.BuildDeck()` 直接載入全部卡牌並初始化學習進度
- `GameManager` 初始化流程調整，避免管理器引用為 null
- 新增 MVP UGUI 介面腳本：CombatUIController, QuizUIController, EnemyView
- 戰鬥 UI 可自動開局並操作出牌、答題、結束回合

---

## 交接記錄（2026-02-09）- WebGL 一鍵建置與自動場景

- 自動生成 MVP 場景與 UI 的 Editor 工具
- 新增 WebGL 建置腳本
- WebGL 建置已改為關閉壓縮（避免 gzip header 問題）
- 已成功產出並可用本機 server 開啟（測試成功）

---

## 交接記錄（2026-02-11）- 系統設計大幅重構 + 開始實作

### 設計階段完成項目

1. **P0-1 標準值平衡體系**：基礎值表、三種偏移方法、答題倍率
2. **P0-2 地圖系統**：7 種房間（含書房）、15 步結構、9 條生成規則
3. **P1-1 協同系統重構**：資源媒介取代固定 Combo（produces/consumes 標籤）
4. **P1-2 四維度 + 精英怪**：Strike/Guard/Boost/Warp 對應詞性、3 隻精英
5. **P2-2 遺物系統 v2**：三層構詞學（6 字首 + 6 字尾 + 6 字根）+ 詞庫連動
6. **P3 詞族進化系統**：Lv.5 三叉選擇（進化/深化/繼續）+ 卡牌獲取方式全覽
7. **書房系統**：LP 貨幣、5 項服務（預習/暫放/進化/深化/筆記）取代商店
8. **湧現式協同**：三軸關鍵詞（維度連鎖/元素共鳴/知識共振）
9. **設計規則文件**：10 絕對規則 + 10 通用規則
10. **實作依賴與進度文件**：Layer 0-10 依賴鏈 + 各系統狀態矩陣

### 實作階段完成項目

- **順序 1：CardData 讀取新欄位** — Dimension enum、RelicMorphType enum、EnemyCategory enum 加入 Enums.cs；CardData 加入 dimension 欄位；RelicData 重寫為三層構詞學結構；EnemyData 加入 category/testsDimension/specialMechanic
- **順序 2：元素弱點/抗性計算** — ApplyElementModifier 實作完成，弱點 ×1.5、抗性 ×0.5，卡牌元素透過 ExecuteCard→ExecuteEffect 傳遞

### 下一步

- **順序 8：休息站系統**
- 詳見 `docs/IMPLEMENTATION_STATUS.md`

---

## 交接記錄（2026-02-11）- 資源媒介協同完成

- 新增設計文件：`docs/plans/2026-02-11-resource-mediator-synergy.md`
- 新增設定檔：`Assets/Resources/Data/synergy_config.json`
- 新增資料結構：`Assets/Scripts/Data/SynergyConfig.cs`
- `DataManager` 載入協同設定
- `CombatManager` 完成回合資源池、消費/產出順序、倍率加成（僅作用主效果）

---

## 交接記錄（2026-02-11）- 三軸協同完成

- 維度連鎖：同維度 2/3 張倍率、覆蓋 3/4 維度獎勵
- 元素共鳴：同元素 2/3 張觸發小效果（回復/攻擊加成/抽牌/格擋/降費）
- 知識共振：Lv.5+ 洞察累積、Lv.1-2 緊接加成
- 相關設定皆由 `Assets/Resources/Data/synergy_config.json` 驅動

---

## 交接記錄（2026-02-11）- 精英怪特殊機制完成

- 書蟲：玩家使用非攻擊牌時 +1 力量；答錯額外 +1 力量
- 石像鬼：前 3 回合沉睡不行動、每回合 +3 力量；醒後攻擊翻倍；沉睡期答對可削弱力量
- 墨水怪：每回合加入 1 張墨漬廢牌；手中有墨漬時答對可消除

---

## 交接記錄（2026-02-11）- 遺物效果實作完成

- 新增遺物效果設定：`Assets/Resources/Data/relic_effects.json`
- 新增遺物效果資料結構：`Assets/Scripts/Data/RelicEffectData.cs`
- `DataManager` 載入遺物效果設定
- 戰鬥/答題遺物已落地（re/dis/pre/over/mis/tion/ness/er/ful/less/ly/port/ject/struct/spec/tract/rupt）
- 休息站選項（relic_un）待休息站系統完成後接入

---

## 交接記錄（2026-02-11）- 地圖生成完成

- 新增地圖設定：`Assets/Resources/Data/map_config.json`
- 新增地圖結構：`Assets/Scripts/Map/MapConfig.cs`
- 新增生成器：`Assets/Scripts/Map/MapGenerator.cs`
- 新增管理器：`Assets/Scripts/Map/MapManager.cs`
- `DataManager` 載入地圖設定；`GameManager` 可取得 `MapManager`

---

## 交接記錄（2026-02-11）- Unity MCP 導入計劃（草案）

### 推薦方案
- **CoplayDev/unity-mcp**（MIT，Unity 2021.3+，工具齊全、社群活躍）

### 導入時機
- 目前已完成核心系統與資料結構，後續進入大量 UI/內容製作與重複性 Editor 操作階段，適合導入 MCP。

### 導入步驟（擬定）
1. **確認環境**：Unity 2022.3 LTS、Python 3.10+、uv、MCP Client（Cursor / Claude / Codex CLI）。
2. **安裝 Unity Package**：透過 Unity Package Manager 以 Git URL 安裝 MCP for Unity。
3. **啟動 MCP Server**：Unity 視窗 `Window > MCP for Unity`，啟動本機 Server。
4. **設定 MCP Client**：選擇客戶端並生成設定檔，連線成功應顯示連線狀態。
5. **驗證流程**：執行簡單命令（如建立空物件、查詢場景物件）確認穩定可用。
6. **建立使用規則**：只允許可逆操作（UI 布局、Prefab 生成、場景檢視）；破壞性操作需人工確認。

### 預期使用場景
- 批量建立/調整 UI、Prefab、Scene 物件
- 執行 Editor MenuItem、批次修正資料與資產
- 快速檢視 Console、抓取截圖、回歸測試輔助

---

## 交接記錄（2026-02-11）- 休息站系統完成

- 新增休息站設定：`Assets/Resources/Data/rest_site_config.json`
- 新增休息站資料結構：`Assets/Scripts/Rest/RestSiteConfig.cs`
- 新增休息站流程：`Assets/Scripts/Rest/RestSiteManager.cs`
- 休息站選項：回血 / 升級卡牌 / 學新字；持有 `relic_un` 時新增喝湯選項
- 升級流程：連續答題 3 次，依答對次數套用熟練度結果
- 學新字：從未學池抽選，答對才解鎖並加入牌組

---

## 交接記錄（2026-02-11）- 書房系統完成

- 新增書房設定：`Assets/Resources/Data/study_room_config.json`
- 新增書房資料結構：`Assets/Scripts/StudyRoom/StudyRoomConfig.cs`
- 新增書房流程：`Assets/Scripts/StudyRoom/StudyRoomManager.cs`
- 書房選項：預習 / 暫放 / 進化 / 深化 / 筆記（對應 LP 花費）
- LP 儲存：`PlayerData.learningPoints`
- 暫放卡牌與筆記資料：`PlayerData.stashedCardWordIds`、`PlayerData.wordNotes`

---

## 交接記錄（2026-02-11）- 詞族進化系統完成

- 新增詞族進化設定：`Assets/Resources/Data/evolution_config.json`
- 新增詞族進化結構：`Assets/Scripts/Evolution/EvolutionConfig.cs`
- 新增詞族進化流程：`Assets/Scripts/Evolution/EvolutionManager.cs`
- 書房可觸發進化流程（由 UI 透過 EvolutionManager 執行）
