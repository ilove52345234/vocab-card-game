using System;
using UnityEngine;
using UnityEngine.UI;
using VocabCardGame.Combat;

namespace VocabCardGame.UI
{
    /// <summary>
    /// 敵人 UI 顯示
    /// </summary>
    public class EnemyView : MonoBehaviour
    {
        public Text nameText;
        public Text hpText;
        public Text intentText;
        public Image selectionFrame;
        public Button selectButton;

        [NonSerialized] public EnemyInstance enemy;

        private Action<EnemyView> onSelected;

        public void Bind(EnemyInstance enemyInstance, Action<EnemyView> onSelectedCallback)
        {
            enemy = enemyInstance;
            onSelected = onSelectedCallback;

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => onSelected?.Invoke(this));
            }

            Refresh();
        }

        public void Refresh()
        {
            if (enemy == null) return;

            if (nameText != null)
            {
                nameText.text = enemy.data != null ? enemy.data.name : "Enemy";
            }

            if (hpText != null)
            {
                hpText.text = $"HP: {enemy.entity.currentHp}/{enemy.entity.maxHp}  Block: {enemy.entity.block}";
            }

            if (intentText != null)
            {
                intentText.text = enemy.currentAction != null
                    ? $"{enemy.currentAction.intent} {enemy.currentAction.value}"
                    : "-";
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectionFrame != null)
            {
                selectionFrame.enabled = selected;
            }
        }
    }
}
