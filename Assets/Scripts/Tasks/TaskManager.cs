using System.Collections.Generic;
using System;
using PushNotificationGod.Data;
using UnityEngine;

namespace PushNotificationGod.Tasks
{
    public class TaskManager : MonoBehaviour
    {
        [SerializeField] private RectTransform cardParent;
        [SerializeField] private TaskCard taskCardPrefab;
        [SerializeField] private IconDatabase iconDatabase;
        [SerializeField] private float stackBottomY = 380f;
        [SerializeField] private float stackBottomPadding;
        [SerializeField] private float taskSpacing = 16f;
        [SerializeField] private float taskCardHeight = 220f;
        [SerializeField] private float slotMoveDuration = 0.14f;
        [SerializeField] private float spawnOffsetY = -120f;
        [SerializeField] private float popDuration = 0.18f;

        private readonly List<TaskCard> visibleCards = new();
        private readonly List<Vector2> slotPositions = new();
        private bool gameOverCleanup;

        public IReadOnlyList<TaskCard> VisibleCards => visibleCards;
        public int VisibleCount => visibleCards.Count;
        public event Action<TaskCard> OnCardSpawned;
        public event Action OnCardRemoved;

        public TaskCard Spawn(TaskDefinition definition)
        {
            gameOverCleanup = false;
            TaskCard card = Instantiate(taskCardPrefab, cardParent);
            NormalizeCardRect(card.RectTransform);
            Sprite iconSprite = iconDatabase != null ? iconDatabase.GetIcon(definition.iconId) : null;
            card.Setup(definition, iconSprite);
            visibleCards.Add(card);
            ReorderSiblings();

            int slotIndex = visibleCards.Count - 1;
            Vector2 target = GetSlotPosition(slotIndex);
            card.RectTransform.anchoredPosition = target + new Vector2(0f, spawnOffsetY);
            card.RectTransform.localScale = Vector3.one * 0.94f;
            StartCoroutine(AnimateSpawn(card, target));
            OnCardSpawned?.Invoke(card);
            return card;
        }

        public void Remove(TaskCard card)
        {
            visibleCards.Remove(card);
            if (card != null)
            {
                if (gameOverCleanup)
                {
                    card.gameObject.SetActive(false);
                }
                else
                {
                    Destroy(card.gameObject);
                }
            }

            ReorderSiblings();
            OnCardRemoved?.Invoke();
        }

        public void RemoveTopMost()
        {
            if (visibleCards.Count == 0)
            {
                return;
            }

            int topIndex = visibleCards.Count - 1;
            TaskCard card = visibleCards[topIndex];
            visibleCards.RemoveAt(topIndex);
            if (card != null)
            {
                card.HideForGameOver();
                Destroy(card.gameObject);
            }

            ReorderSiblings();
        }

        public void HideAllForGameOver()
        {
            gameOverCleanup = true;
            StopAllCoroutines();
            for (int i = 0; i < visibleCards.Count; i++)
            {
                TaskCard card = visibleCards[i];
                if (card != null)
                {
                    card.HideForGameOver();
                }
            }

            visibleCards.Clear();
        }

        public void ClearAllForRestart()
        {
            gameOverCleanup = false;
            StopAllCoroutines();
            for (int i = 0; i < visibleCards.Count; i++)
            {
                TaskCard card = visibleCards[i];
                if (card == null)
                {
                    continue;
                }

                card.HideForGameOver();
                Destroy(card.gameObject);
            }

            visibleCards.Clear();
        }

        public bool IsOverflow(float deadlineY, int maxVisibleTaskCount)
        {
            if (maxVisibleTaskCount > 0 && visibleCards.Count > maxVisibleTaskCount)
            {
                return true;
            }

            foreach (TaskCard card in visibleCards)
            {
                float cardTopY = card.RectTransform.anchoredPosition.y + GetCardHeight() * 0.5f;
                if (cardTopY >= deadlineY)
                {
                    return true;
                }
            }

            return false;
        }

        private void Update()
        {
            for (int i = 0; i < visibleCards.Count; i++)
            {
                TaskCard card = visibleCards[i];
                if (card.IsInteracting)
                {
                    continue;
                }

                Vector2 target = GetSlotPosition(i);
                float moveSpeed = slotMoveDuration <= 0f ? 1000f : 4f / slotMoveDuration;
                card.RectTransform.anchoredPosition = Vector2.Lerp(card.RectTransform.anchoredPosition, target, Time.deltaTime * moveSpeed);
                card.CacheBasePosition();
            }
        }

        private Vector2 GetSlotPosition(int indexFromBottom)
        {
            while (slotPositions.Count <= indexFromBottom)
            {
                slotPositions.Add(CalculateSlotPosition(slotPositions.Count));
            }

            return slotPositions[indexFromBottom];
        }

        private Vector2 CalculateSlotPosition(int indexFromBottom)
        {
            float cardHeight = GetCardHeight();
            float stackBottom = stackBottomY + stackBottomPadding;
            float centerY = stackBottom + cardHeight * 0.5f + indexFromBottom * (cardHeight + taskSpacing);
            return new Vector2(0f, centerY);
        }

        private static void NormalizeCardRect(RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private float GetCardHeight()
        {
            if (taskCardHeight > 0f)
            {
                return taskCardHeight;
            }

            return taskCardPrefab.RectTransform.rect.height;
        }

        private void ReorderSiblings()
        {
            for (int i = 0; i < visibleCards.Count; i++)
            {
                visibleCards[i].transform.SetSiblingIndex(i);
            }
        }

        private System.Collections.IEnumerator AnimateSpawn(TaskCard card, Vector2 target)
        {
            float elapsed = 0f;
            Vector2 start = card.RectTransform.anchoredPosition;
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / popDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                card.RectTransform.anchoredPosition = Vector2.Lerp(start, target, eased);
                card.RectTransform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, eased);
                yield return null;
            }

            card.RectTransform.anchoredPosition = target;
            card.RectTransform.localScale = Vector3.one;
            card.CacheBasePosition();
        }
    }
}
