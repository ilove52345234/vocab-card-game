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
- 每日新詞上限：5-10 → 10-15 → 15-20

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
│   ├── Enums.cs            # 列舉定義
│   ├── WordData.cs         # 單字資料結構
│   ├── CardData.cs         # 卡牌資料結構
│   └── CombatData.cs       # 戰鬥資料結構
├── Combat/
│   └── CombatManager.cs    # 戰鬥邏輯（700+ 行）
└── Learning/
    ├── LearningManager.cs  # 學習進度管理
    └── QuizManager.cs      # 答題邏輯
```

## 範例資料
- 10 個單字（5 元素各 2 個）
- 對應卡牌效果
- 6 個敵人（含 1 Boss）

## 待完成

- [ ] Unity 授權啟用
- [ ] UI 系統建立
- [ ] 擴充詞庫至 30 張 MVP
- [ ] AI 生成卡牌美術
- [ ] 戰鬥畫面實作

## 重要文件

- 設計文件：`docs/plans/2026-02-06-game-design.md`
- 專案 README：`README.md`

## GitHub

- Repo: https://github.com/ilove52345234/vocab-card-game
- 帳號: ilove52345234

---

*最後更新：2026-02-08*
