---
name: add-word
description: 新增單字到詞庫
---

# 新增單字

請按照以下步驟新增單字：

1. 讀取現有詞庫 `Assets/Resources/Data/words.json`
2. 詢問用戶要新增的單字資訊：
   - 英文單字
   - 中文意思
   - 詞性 (n./v./adj./adv.)
   - 元素分類 (Life/Force/Mind/Matter/Abstract)
   - 種族分類 (Animals/Food/Actions/Emotions/Objects 等)
3. 自動生成：
   - id（使用英文單字小寫）
   - 音標（如果知道）
   - 難度等級（根據字母數：3-4=1, 5-6=2, 7+=3）
   - audioPath: `Audio/Words/{word}`
4. 新增到 words.json
5. 同時在 `Assets/Resources/Data/cards.json` 新增對應卡牌
6. 提交變更到 git
