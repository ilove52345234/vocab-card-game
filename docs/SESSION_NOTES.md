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

- **順序 3：資源媒介協同**（produces/consumes 運行時追蹤）
- 然後順序 4：湧現式三軸協同（維度連鎖/元素共鳴/知識共振）
- 詳見 `docs/IMPLEMENTATION_STATUS.md`
