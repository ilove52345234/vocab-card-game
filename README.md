# Vocab Card Game

英語學習卡牌 roguelike - 以答題與熟練度驅動戰鬥與構築深度

## 系統設計簡述

- 學習與熟練度：SM-2 熟練度直接影響卡牌倍率與答題機率，學得越好戰力越強。
- 答題流程：識讀→聽力→拼字的漸進式難度，答題結果回饋於戰鬥表現。
- 戰鬥循環：回合制出牌，能量與抽牌節奏控制，敵人意圖透明。
- 卡牌系統：一字一卡，標準值平衡，效果以關鍵詞與條件驅動。
- 協同系統：維度連鎖、元素共鳴、知識共振三軸，資源媒介作為互動管道。
- 遺物系統：字首/字尾/字根構詞學對應效果，字根遺物具詞庫連動。
- 地圖與房間：三章節、路線規則、7 種房間，休息站與書房承接學習節奏。

## 參考遊戲方向

- Slay the Spire：戰鬥節奏、意圖顯示、卡牌構築與地圖進程的整體框架。
- 以學習為核心的遊戲化設計：答題即戰鬥、熟練度即強度，強化長期學習動機。

## 開發進度

<!-- 此區塊由 Claude 自動更新 -->

| 項目 | 狀態 | 備註 |
|------|------|------|
| 遊戲設計文件 | ✅ 完成 | `docs/plans/2026-02-06-game-design.md` |
| 程式架構 | ✅ 完成 | Core, Data, Combat, Learning |
| 單字資料 | 🔶 進行中 | 30/1000 個單字 |
| 卡牌效果 | 🔶 進行中 | 30/1000 張卡牌 |
| 敵人設計 | 🔶 進行中 | 6/50 個敵人 |
| Unity 專案 | ⏳ 待處理 | 需要授權啟用 |
| UI 系統 | 🔶 進行中 | MVP UGUI 版可操作 |
| 美術資源 | ⏳ 待處理 | 計劃使用 AI 生成 |
| 音效/發音 | ⏳ 待處理 | 需要 1000+ 單字發音 |

**最後更新：2026-02-08**

---

## 系統設計解說

### 學習與熟練度系統

以 SM-2 記憶曲線為核心，熟練度直接影響卡牌倍率與答題機率。新字必須答題後才能入牌，確保「學習即遊玩」。對應設計：`docs/plans/2026-02-06-game-design.md`。

### 答題系統

答題流程採識讀→聽力→拼字漸進式難度。答題結果會即時影響卡牌效果倍率，並回饋學習進度。對應實作：`Assets/Scripts/Learning/QuizManager.cs`。

### 戰鬥系統

回合制出牌，能量與抽牌節奏控制，敵人意圖透明顯示。戰鬥力來源遵循 60/35/5 分佈，避免數值膨脹。對應實作：`Assets/Scripts/Combat/CombatManager.cs`。

### 卡牌系統

一字一卡，卡牌效果基於標準值表並可量化偏移。卡牌維度與元素由單字語義決定，不可為平衡需要而調整。對應資料：`Assets/Resources/Data/cards.json`。

### 協同系統

協同固定三軸：維度連鎖、元素共鳴、知識共振。資源媒介（produces/consumes）僅作為互動管道，不新增協同軸。對應設計：`docs/plans/2026-02-11-synergy-system-design.md`、`docs/plans/2026-02-11-resource-mediator-synergy.md`。

### 遺物系統

以字首/字尾/字根構詞學為基礎，效果必須與語意掛鉤，字根遺物必有詞庫連動。對應設計：`docs/plans/2026-02-11-relic-system-design.md`。

### 地圖與房間系統

三章節、15 步路線，遵循固定規則與房間比例，休息站與書房承接學習節奏。對應設計：`docs/plans/2026-02-11-map-system-design.md`、`docs/plans/2026-02-11-study-room-design.md`。

### 詞族進化系統

Lv.5 進化三叉選擇（進化/深化/繼續），進化不是上位替代而是策略分歧。對應設計：`docs/plans/2026-02-11-card-upgrade-system.md`。

### UI/UX 系統

資訊層級以學習優先，戰鬥中必須可存取單字釋義與發音。答題回饋必須即時清楚。對應規範：`docs/DESIGN_RULES.md`。

---

## Unity 設定步驟

### 1. 開啟專案

```bash
# macOS
open -a "/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app" --args -projectPath "$(pwd)"
```

### 2. 建立測試場景

1. `File > New Scene`
2. 儲存為 `Assets/Scenes/TestScene.unity`
3. 創建空物件，命名為 `GameBootstrap`
4. 添加腳本：`VocabCardGame.Core.GameBootstrap`
5. 執行場景

### 3. 測試功能

在 `GameBootstrap` 物件上右鍵：
- **Test Show Database Info** - 檢查資料載入
- **Test Start Combat** - 測試戰鬥流程

---

## 專案結構

```
vocab-card-game/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # GameManager, DataManager, AudioManager
│   │   ├── Data/           # Enums, WordData, CardData, CombatData
│   │   ├── Combat/         # CombatManager
│   │   ├── Learning/       # LearningManager, QuizManager
│   │   └── UI/             # （待建立）
│   └── Resources/Data/     # JSON 資料檔
├── docs/
│   ├── plans/              # 設計文件
│   └── SESSION_NOTES.md    # 開發筆記
├── .claude/skills/         # Claude 技能檔
├── CLAUDE.md               # Claude 專案說明
└── README.md               # 本文件
```

---

## 核心設計

### 五大元素
- 🌿 **Life** - 回復、成長、數量
- 🔥 **Force** - 直傷、爆發、連段
- 💧 **Mind** - 控場、抽牌、干擾
- ⚙️ **Matter** - 裝備、防禦、持續
- ✨ **Abstract** - 特殊規則、費用操控

### 戰鬥力分佈
- 學習相關：60%
- 策略操作：35%
- 純數值：5%

### 詳細設計
見 `docs/plans/2026-02-06-game-design.md`

---

## 技術棧

| 項目 | 選擇 |
|------|------|
| 引擎 | Unity 2022.3 LTS |
| 語言 | C# |
| 美術 | AI 生成 + 後製 |
| 儲存 | SQLite + JSON |
| 平台 | iOS / Android |

---

## License

Private Project

### WebGL 熱部署（檔案變動自動重建）

```bash
scripts/dev_webgl_watch.sh
```

- 預設輸出到 `Builds/WebGL_dev/`
- 伺服器預設 `http://127.0.0.1:8000/index.html`
- 變更 `PORT=9000 scripts/dev_webgl_watch.sh`
- 停止：`Ctrl+C`
