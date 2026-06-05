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
        [SerializeField] private float flyOutSpeed = 1800f;
        [SerializeField] private float returnSpeed = 12f;

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
            while (Vector2.Distance(rectTransform.anchoredPosition, baseAnchoredPosition) > 1f)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, baseAnchoredPosition, Time.deltaTime * returnSpeed);
                rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, Quaternion.identity, Time.deltaTime * returnSpeed);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * returnSpeed);
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
            while (rectTransform.anchoredPosition.x < 1300f)
            {
                rectTransform.anchoredPosition += Vector2.right * flyOutSpeed * Time.deltaTime;
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, -maxTiltDegrees);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Max(0f, canvasGroup.alpha - Time.deltaTime * 3f);
                }

                yield return null;
            }

            onDone?.Invoke();
        }
    }
}
