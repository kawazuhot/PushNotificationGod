using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PushNotificationGod.Input
{
    public class SwipeInputHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private float tapMaxDistance = 30f;
        [SerializeField] private float swipeRightThreshold = 220f;
        [SerializeField] private float maxTiltDegrees = 8f;
        [SerializeField] private float minDragAlpha = 0.65f;
        [SerializeField] private float swipeFlyOutDuration = 0.15f;
        [SerializeField] private float swipeReturnDuration = 0.12f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 baseAnchoredPosition;
        private Vector2 pointerDownPosition;
        private bool dragging;
        private bool resolving;
        private bool hasBasePosition;

        public bool IsInteracting => dragging || resolving;
        public bool HasBasePosition => hasBasePosition;
        public Vector2 BaseAnchoredPosition => baseAnchoredPosition;
        public event Action OnTap;
        public event Action OnSwipeRight;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void CacheBasePosition()
        {
            baseAnchoredPosition = rectTransform.anchoredPosition;
            hasBasePosition = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (resolving)
            {
                return;
            }

            dragging = true;
            pointerDownPosition = eventData.position;
            CacheBasePosition();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging || resolving)
            {
                return;
            }

            Vector2 delta = eventData.position - pointerDownPosition;
            float x = Mathf.Max(0f, delta.x);
            rectTransform.anchoredPosition = baseAnchoredPosition + new Vector2(x, 0f);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Clamp(x / swipeRightThreshold, 0f, 1f) * maxTiltDegrees);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, minDragAlpha, Mathf.Clamp01(x / swipeRightThreshold));
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!dragging || resolving)
            {
                return;
            }

            dragging = false;
            Vector2 delta = eventData.position - pointerDownPosition;
            if (delta.x >= swipeRightThreshold)
            {
                resolving = true;
                OnSwipeRight?.Invoke();
                return;
            }

            if (delta.magnitude <= tapMaxDistance)
            {
                resolving = true;
                OnTap?.Invoke();
                return;
            }

            StartCoroutine(ReturnToBase());
        }

        public void PlayFlyOutAndDestroy(Action onDone)
        {
            resolving = true;
            StartCoroutine(FlyOut(onDone));
        }

        private System.Collections.IEnumerator ReturnToBase()
        {
            Vector2 startPosition = rectTransform.anchoredPosition;
            Quaternion startRotation = rectTransform.localRotation;
            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
            float duration = Mathf.Max(0.01f, swipeReturnDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, baseAnchoredPosition, eased);
                rectTransform.localRotation = Quaternion.Slerp(startRotation, Quaternion.identity, eased);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, eased);
                }

                yield return null;
            }

            rectTransform.anchoredPosition = baseAnchoredPosition;
            rectTransform.localRotation = Quaternion.identity;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private System.Collections.IEnumerator FlyOut(Action onDone)
        {
            Vector2 startPosition = rectTransform.anchoredPosition;
            Vector2 endPosition = new(Mathf.Max(startPosition.x + 900f, 1400f), startPosition.y);
            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
            float duration = Mathf.Max(0.01f, swipeFlyOutDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, eased);
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, -maxTiltDegrees);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, eased);
                }

                yield return null;
            }

            rectTransform.anchoredPosition = endPosition;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            onDone?.Invoke();
        }
    }
}
