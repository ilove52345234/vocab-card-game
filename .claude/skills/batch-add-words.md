---
name: batch-add-words
description: 批次新增多個單字
---

# 批次新增單字

一次新增多個單字到詞庫。

## 輸入格式

用戶可以用以下格式提供單字清單：

```
apple, 蘋果, n., Life, Food
banana, 香蕉, n., Life, Food
run, 跑, v., Force, Actions
happy, 快樂的, adj., Mind, Emotions
```

或是只提供英文，讓 Claude 自動填充其他資訊：

```
apple, banana, orange, grape, watermelon
```

## 執行步驟

1. 解析用戶輸入
2. 如果資訊不完整，自動推斷：
   - 中文翻譯
   - 詞性
   - 適合的元素和種族
3. 為每個單字設計卡牌效果
4. 批次更新 words.json 和 cards.json
5. 顯示新增結果摘要
6. 提交變更到 git
