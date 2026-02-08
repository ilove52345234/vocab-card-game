using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VocabCardGame.Combat;
using VocabCardGame.Core;
using VocabCardGame.Data;

namespace VocabCardGame.UI
{
    /// <summary>
    /// 戰鬥 UI 控制器（UGUI 最小可玩版）
    /// </summary>
    public class CombatUIController : MonoBehaviour
    {
        [Header("References")]
        public CombatManager combatManager;
        public QuizUIController quizUI;

        [Header("Player UI")]
        public Text playerHpText;
        public Text playerBlockText;
        public Text energyText;
        public Text turnText;
        public Text stateText;

        [Header("Hand UI")]
        public Transform handContainer;
        public GameObject cardButtonPrefab;

        [Header("Enemy UI")]
        public Transform enemyContainer;
        public GameObject enemyItemPrefab;

        [Header("Controls")]
        public Button endTurnButton;

        private readonly List<EnemyView> enemyViews = new List<EnemyView>();
        private EnemyInstance selectedEnemy;

        private void Start()
        {
            if (combatManager == null)
            {
                combatManager = GameManager.Instance != null ? GameManager.Instance.combatManager : null;
            }

            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }

            // 自動啟動一場戰鬥（MVP 測試用）
            if (combatManager != null && combatManager.currentState == CombatState.NotInCombat && GameManager.Instance != null)
            {
                var enemies = GameManager.Instance.dataManager.GetEnemiesForFloor(1, GameManager.Instance.currentMode);
                combatManager.InitializeRun();
                combatManager.StartCombat(enemies);
            }

            RefreshAll();
        }

        private void OnEnable()
        {
            if (combatManager == null)
            {
                combatManager = GameManager.Instance != null ? GameManager.Instance.combatManager : null;
            }
            if (combatManager == null) return;

            combatManager.OnCombatStateChanged += OnCombatStateChanged;
            combatManager.OnCardDrawn += OnCardDrawn;
            combatManager.OnCardPlayed += OnCardPlayed;
            combatManager.OnEnemyDamaged += OnEnemyDamaged;
            combatManager.OnPlayerDamaged += OnPlayerDamaged;
            combatManager.OnTurnStart += OnTurnStart;
            combatManager.OnTurnEnd += OnTurnEnd;
            combatManager.OnCombatEnd += OnCombatEnd;
        }

        private void OnDisable()
        {
            if (combatManager == null) return;

            combatManager.OnCombatStateChanged -= OnCombatStateChanged;
            combatManager.OnCardDrawn -= OnCardDrawn;
            combatManager.OnCardPlayed -= OnCardPlayed;
            combatManager.OnEnemyDamaged -= OnEnemyDamaged;
            combatManager.OnPlayerDamaged -= OnPlayerDamaged;
            combatManager.OnTurnStart -= OnTurnStart;
            combatManager.OnTurnEnd -= OnTurnEnd;
            combatManager.OnCombatEnd -= OnCombatEnd;
        }

        private void RefreshAll()
        {
            RefreshPlayerUI();
            RefreshHandUI();
            RefreshEnemyUI();
            RefreshStateUI();
        }

        private void RefreshPlayerUI()
        {
            if (combatManager == null || combatManager.player == null) return;

            if (playerHpText != null)
            {
                playerHpText.text = $"HP: {combatManager.player.currentHp}/{combatManager.player.maxHp}";
            }

            if (playerBlockText != null)
            {
                playerBlockText.text = $"Block: {combatManager.player.block}";
            }

            if (energyText != null)
            {
                energyText.text = $"Energy: {combatManager.currentEnergy}/{combatManager.maxEnergy}";
            }

            if (turnText != null)
            {
                turnText.text = $"Turn: {combatManager.turnNumber}";
            }
        }

        private void RefreshStateUI()
        {
            if (stateText != null && combatManager != null)
            {
                stateText.text = combatManager.currentState.ToString();
            }
        }

        private void RefreshHandUI()
        {
            if (handContainer == null || cardButtonPrefab == null || combatManager == null) return;

            foreach (Transform child in handContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var card in combatManager.hand)
            {
                var go = Instantiate(cardButtonPrefab, handContainer);
                var button = go.GetComponent<Button>();
                var text = go.GetComponentInChildren<Text>();

                if (text != null)
                {
                    text.text = $"{card.wordData?.english ?? card.wordId} ({card.energyCost})";
                }

                if (button != null)
                {
                    button.onClick.AddListener(() => OnCardClicked(card));
                }
            }
        }

        private void RefreshEnemyUI()
        {
            if (enemyContainer == null || enemyItemPrefab == null || combatManager == null) return;

            foreach (Transform child in enemyContainer)
            {
                Destroy(child.gameObject);
            }
            enemyViews.Clear();

            foreach (var enemy in combatManager.enemies)
            {
                var go = Instantiate(enemyItemPrefab, enemyContainer);
                var view = go.GetComponent<EnemyView>();
                if (view != null)
                {
                    view.Bind(enemy, OnEnemySelected);
                    enemyViews.Add(view);
                }
            }

            // 預設選擇第一個存活敵人
            selectedEnemy = combatManager.enemies.FirstOrDefault(e => e.entity.IsAlive);
            UpdateEnemySelectionUI();
        }

        private void UpdateEnemySelectionUI()
        {
            foreach (var view in enemyViews)
            {
                view.SetSelected(view.enemy == selectedEnemy);
            }
        }

        private void OnCardClicked(CardData card)
        {
            if (combatManager == null || combatManager.currentState != CombatState.PlayerTurn) return;

            var target = selectedEnemy;
            if (target == null || !target.entity.IsAlive)
            {
                target = combatManager.enemies.FirstOrDefault(e => e.entity.IsAlive);
            }

            combatManager.TryPlayCard(card, target);
            RefreshHandUI();
            RefreshPlayerUI();
            RefreshEnemyUI();
        }

        private void OnEnemySelected(EnemyView view)
        {
            if (view == null || view.enemy == null) return;
            if (!view.enemy.entity.IsAlive) return;

            selectedEnemy = view.enemy;
            UpdateEnemySelectionUI();
        }

        private void OnEndTurnClicked()
        {
            if (combatManager == null) return;
            combatManager.EndPlayerTurn();
            RefreshAll();
        }

        private void OnCombatStateChanged(CombatState state)
        {
            RefreshStateUI();
            RefreshPlayerUI();
        }

        private void OnCardDrawn(CardData card)
        {
            RefreshHandUI();
            RefreshPlayerUI();
        }

        private void OnCardPlayed(CardData card)
        {
            RefreshHandUI();
            RefreshPlayerUI();
            RefreshEnemyUI();
        }

        private void OnEnemyDamaged(EnemyInstance enemy, int dmg)
        {
            RefreshEnemyUI();
        }

        private void OnPlayerDamaged(int dmg)
        {
            RefreshPlayerUI();
        }

        private void OnTurnStart()
        {
            RefreshAll();
        }

        private void OnTurnEnd()
        {
            RefreshAll();
        }

        private void OnCombatEnd(bool victory)
        {
            RefreshAll();
            if (stateText != null)
            {
                stateText.text = victory ? "Victory" : "Defeat";
            }
        }
    }
}
