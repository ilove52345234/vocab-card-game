---
name: design-card
description: 設計卡牌效果
---

# 設計卡牌效果

根據單字的意思和元素，設計適合的卡牌效果。

## 設計原則

1. **語意連結**：卡牌效果要與單字意思相關
   - fire（火）→ 造成傷害 + 燃燒
   - heal（治癒）→ 回復生命
   - shield（盾）→ 獲得格擋

2. **元素特性**：
   - Life：回復、成長、召喚
   - Force：直傷、連擊、爆發
   - Mind：控制、抽牌、干擾
   - Matter：格擋、裝備、持續
   - Abstract：費用操控、特殊規則

3. **費用平衡**：
   - 0 費：弱效果或有副作用
   - 1 費：標準效果
   - 2 費：強效果
   - 3 費：極強效果

4. **效果類型**（參考 CardEffectType）：
   - Damage (0)：造成傷害
   - Block (1)：獲得格擋
   - Heal (2)：回復生命
   - DrawCard (3)：抽牌
   - GainEnergy (4)：獲得能量
   - ApplyBurning (6)：施加燃燒
   - ...（見 Enums.cs）

## 執行步驟

1. 讀取單字資料
2. 根據意思和元素設計效果
3. 確認費用平衡
4. 設計 Combo 組合（可選）
5. 更新 cards.json
