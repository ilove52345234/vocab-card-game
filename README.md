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
| 資源媒介協同 | ✅ 完成 | produces/consumes 回合追蹤已實作 |
| 三軸協同 | ✅ 完成 | 維度連鎖/元素共鳴/知識共振已實作 |
| 精英怪機制 | ✅ 完成 | 書蟲/石像鬼/墨水怪已實作 |
| 遺物效果 | ✅ 完成 | 戰鬥/答題相關遺物已落地 |

**最後更新：2026-02-11**

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

## 設計原則摘要

- 學習即遊玩：任何新卡入牌前必須答題，不存在跳過學習的獲取。
- 熟練度即力量：卡牌倍率與答題機率由 SM-2 熟練度決定。
- 關鍵詞協同：協同只基於維度/元素/熟練度/資源標籤，不做卡對卡。
- 三軸封頂：維度連鎖、元素共鳴、知識共振固定為唯一協同軸。
- 標準值平衡：所有數值必須回到標準值表並量化偏移。
- 英語系統即遊戲系統：機制必須對應真實語言學習概念。

對應規範：`docs/DESIGN_RULES.md`

---

## 實作順序進度

1. ✅ 順序 1：CardData 讀取新欄位
2. ✅ 順序 2：元素弱點/抗性計算
3. ✅ 順序 3：資源媒介協同（produces/consumes 運行時追蹤）
4. ✅ 順序 4：湧現式三軸協同（維度連鎖/元素共鳴/知識共振）
5. ✅ 順序 5：精英怪特殊機制
6. ✅ 順序 6：遺物效果實作
7. ⏳ 順序 7：地圖生成
8. ⏳ 順序 8：休息站系統
9. ⏳ 順序 9：書房系統
10. ⏳ 順序 10：詞族進化
11. ⏳ 順序 11：UI 全套

對應進度：`docs/IMPLEMENTATION_STATUS.md`

---

## 三軸協同要點

- 維度連鎖：同維度第 2 張 +20%，第 3 張 +40%，上限固定；多維度覆蓋可抽牌與獲能。
- 元素共鳴：同元素 2 張觸發小效果，3 張翻倍，之後不再增加。
- 知識共振：Lv.5+ 產生洞察，3 洞察觸發獎勵；Lv.1-2 接在 Lv.5+ 後 +30%。

對應設計：`docs/plans/2026-02-11-synergy-system-design.md`

---

## 標準值表（卡牌平衡）

| 費用 | 傷害 | 格擋 | 回血 | 抽牌 |
|------|------|------|------|------|
| 0 費 | 7 | 5 | 3 | 1 |
| 1 費 | 12 | 10 | 6 | 2 |
| 2 費 | 20 | 17 | 10 | 3 |
| 3 費 | 30 | 25 | 15 | 5 |

對應設計：`docs/plans/2026-02-11-standard-value-balance.md`

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

## 設計文件索引

- 設計規則：`docs/DESIGN_RULES.md`
- 主設計文檔：`docs/plans/2026-02-06-game-design.md`
- 標準值平衡：`docs/plans/2026-02-11-standard-value-balance.md`
- 協同系統：`docs/plans/2026-02-11-synergy-system-design.md`
- 資源媒介協同：`docs/plans/2026-02-11-resource-mediator-synergy.md`
- 地圖系統：`docs/plans/2026-02-11-map-system-design.md`
- 書房系統：`docs/plans/2026-02-11-study-room-design.md`
- 遺物系統：`docs/plans/2026-02-11-relic-system-design.md`
- 詞族進化：`docs/plans/2026-02-11-card-upgrade-system.md`
- 依賴與進度：`docs/IMPLEMENTATION_STATUS.md`

---

### WebGL 熱部署（檔案變動自動重建）

```bash
scripts/dev_webgl_watch.sh
```

- 預設輸出到 `Builds/WebGL_dev/`
- 伺服器預設 `http://127.0.0.1:8000/index.html`
- 變更 `PORT=9000 scripts/dev_webgl_watch.sh`
- 停止：`Ctrl+C`
