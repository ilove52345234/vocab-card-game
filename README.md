# Vocab Card Game

英文單字卡牌肉鴿遊戲 - 結合殺戮尖塔機制與間隔重複學習

## Unity 設定步驟

### 1. 建立 Unity 專案

1. 開啟 Unity Hub
2. 點擊 **New Project**
3. 選擇 **2D (Built-in Render Pipeline)** 模板
4. 專案名稱設為任意（如 `VocabCardGameUnity`）
5. 建立專案

### 2. 匯入程式碼

**方法 A：複製資料夾**
```bash
# 將 Assets 資料夾複製到 Unity 專案中
cp -r /Users/romin.zhang/IdeaProjects/vocab-card-game/Assets/* /path/to/your/unity/project/Assets/
```

**方法 B：直接使用此資料夾**
1. 在此資料夾建立 Unity 專案設定檔
2. 用 Unity Hub 開啟此資料夾

### 3. 建立測試場景

1. 在 Unity 中建立新場景：`File > New Scene`
2. 儲存為 `Assets/Scenes/TestScene.unity`
3. 創建空物件：`GameObject > Create Empty`
4. 命名為 `GameBootstrap`
5. 添加腳本：`Add Component > VocabCardGame.Core.GameBootstrap`
6. 執行場景

### 4. 測試功能

場景運行後，在 `GameBootstrap` 物件上：
- 右鍵點擊 `GameBootstrap` 腳本
- 選擇 **Test Show Database Info** 檢查資料載入
- 選擇 **Test Start Combat** 測試戰鬥流程

### 5. Console 預期輸出

```
[GameBootstrap] GameManager created successfully
[GameBootstrap] QuizManager created successfully
[Test] Word Database: 10 words loaded
  - fire (火) [Force]
  - water (水) [Mind]
  - wolf (狼) [Life]
  ...
```

## 專案結構

```
Assets/
├── Scripts/
│   ├── Core/           # 核心管理器
│   ├── Data/           # 資料結構
│   ├── Combat/         # 戰鬥系統
│   ├── Learning/       # 學習系統
│   └── UI/             # UI 控制（待建立）
├── Resources/Data/     # JSON 資料
├── Prefabs/            # 預製物件（待建立）
└── Scenes/             # 場景（待建立）
```

## 下一步開發

1. [ ] 建立 UI 系統
2. [ ] 設計卡牌 Prefab
3. [ ] 實作戰鬥畫面
4. [ ] 添加更多單字資料
5. [ ] 整合 AI 生成美術

## 技術文件

詳細設計文件：`docs/plans/2026-02-06-game-design.md`
