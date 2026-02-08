---
name: add-enemy
description: 新增敵人到遊戲
---

# 新增敵人

請按照以下步驟新增敵人：

1. 讀取現有敵人資料 `Assets/Resources/Data/enemies.json`
2. 詢問用戶：
   - 敵人名稱（中文）
   - 敵人 ID（英文）
   - HP 數值
   - 所屬元素
   - 弱點元素（可選）
   - 抗性元素（可選）
   - 是否為 Boss

3. 根據敵人類型設計行動模式：
   - 普通敵人：2-3 種行動
   - 精英敵人：3-4 種行動
   - Boss：4-5 種行動 + 特殊技能

4. 行動類型參考：
   - 攻擊 (intent: 0)
   - 防禦 (intent: 1)
   - 增益自己 (intent: 2)
   - 削弱玩家 (intent: 3)
   - 攻擊+削弱 (intent: 4)
   - 特殊技能 (intent: 5)

5. 新增到 enemies.json
6. 提交變更到 git
