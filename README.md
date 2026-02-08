# Vocab Card Game

英文單字卡牌肉鴿遊戲 - 結合殺戮尖塔機制與間隔重複學習

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

## 快速開始

### 在 Claude Code 中繼續開發

```bash
git clone https://github.com/ilove52345234/vocab-card-game.git
cd vocab-card-game
claude
```

然後輸入 `/continue` 繼續開發。

### 一鍵建置 WebGL 並啟動本機伺服器

```bash
scripts/build_webgl_and_serve.sh
```

需求：Unity Hub 已安裝該版本的 `WebGL Build Support` 模組。

預設使用 `http://localhost:8000`，如需改 port：

```bash
PORT=9000 scripts/build_webgl_and_serve.sh
```

若只想建置不啟動伺服器：

```bash
SERVE=0 scripts/build_webgl_and_serve.sh
```

若要啟動伺服器但不阻塞終端：

```bash
WAIT_FOR_SERVER=0 scripts/build_webgl_and_serve.sh
```

### MVP 測試場景

MVP 場景已自動生成：`Assets/Scenes/MvpScene.unity`  
可直接 Play 進入戰鬥流程。

### 可用的 Claude Skills

| 指令 | 功能 |
|------|------|
| `/continue` | 繼續專案開發 |
| `/add-word` | 新增單字 |
| `/batch-add-words` | 批次新增單字 |
| `/add-enemy` | 新增敵人 |
| `/design-card` | 設計卡牌效果 |
| `/review-design` | 檢視設計進度 |

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
