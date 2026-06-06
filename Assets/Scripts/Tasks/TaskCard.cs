using System;
using PushNotificationGod.Data;
using PushNotificationGod.Input;
using PushNotificationGod.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.Tasks
{
    public class TaskCard : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text appNameText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text messageText;
        [SerializeField] private Color normalColor = new(0.96f, 0.99f, 1f, 0.94f);
        [SerializeField] private Color goldColor = new(1f, 0.95f, 0.78f, 0.95f);
        [SerializeField] private Color tapFlashColor = new(1f, 1f, 1f, 1f);
        [SerializeField] private float tapPopScale = 1.06f;
        [SerializeField] private float tapDismissScale = 0.9f;
        [SerializeField] private float tapDisappearDuration = 0.15f;
        [SerializeField] private SwipeInputHandler inputHandler;

        private TaskDefinition definition;
        private bool handled;
        private Color baseBackgroundColor;

        public TaskDefinition Definition => definition;
        public RectTransform RectTransform => (RectTransform)transform;
        public bool IsInteracting => inputHandler != null && inputHandler.IsInteracting;
        public event Action<TaskCard, TaskAction> OnAction;

        private void Awake()
        {
            UIJapaneseFont.Apply(appNameText);
            UIJapaneseFont.Apply(timeText);
            UIJapaneseFont.Apply(messageText);

            if (inputHandler == null)
            {
                inputHandler = GetComponent<SwipeInputHandler>();
            }

            inputHandler.OnTap += () => Submit(TaskAction.Tap);
            inputHandler.OnSwipeRight += () => Submit(TaskAction.SwipeRight);
        }

        public void Setup(TaskDefinition task, Sprite iconSprite = null)
        {
            definition = task;
            handled = false;
            appNameText.text = task.appName;
            timeText.text = "今";
            messageText.text = task.messageText;
            ApplyTextStyle();
            baseBackgroundColor = task.backgroundStyleId == "gold" ? goldColor : normalColor;
            backgroundImage.color = baseBackgroundColor;

            if (iconImage != null)
            {
                if (iconSprite != null)
                {
                    iconImage.sprite = iconSprite;
                }

                iconImage.color = Color.white;
                iconImage.preserveAspect = true;
            }

            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
            }

            RectTransform.localRotation = Quaternion.identity;
            RectTransform.localScale = Vector3.one;
        }

        private void ApplyTextStyle()
        {
            if (appNameText != null)
            {
                UIJapaneseFont.Apply(appNameText);
                appNameText.fontSize = 34;
                appNameText.fontStyle = FontStyle.Bold;
                appNameText.color = new Color(0.12f, 0.12f, 0.14f, 1f);
                appNameText.horizontalOverflow = HorizontalWrapMode.Overflow;
                appNameText.verticalOverflow = VerticalWrapMode.Truncate;
                appNameText.lineSpacing = 1.05f;
            }

            if (timeText != null)
            {
                UIJapaneseFont.Apply(timeText);
                timeText.fontSize = 28;
                timeText.fontStyle = FontStyle.Bold;
                timeText.color = new Color(0.28f, 0.28f, 0.31f, 0.92f);
                timeText.horizontalOverflow = HorizontalWrapMode.Overflow;
                timeText.verticalOverflow = VerticalWrapMode.Truncate;
            }

            if (messageText != null)
            {
                UIJapaneseFont.Apply(messageText);
                messageText.fontSize = 40;
                messageText.fontStyle = FontStyle.Bold;
                messageText.color = new Color(0.11f, 0.11f, 0.12f, 1f);
                messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
                messageText.verticalOverflow = VerticalWrapMode.Truncate;
                messageText.lineSpacing = 1.08f;
            }
        }

        public void CacheBasePosition()
        {
            inputHandler.CacheBasePosition();
        }

        public Vector3 GetStableScoreWorldPosition()
        {
            Vector3 localOffset = new(0f, RectTransform.rect.height * 0.18f, 0f);
            Vector3 worldPoint = RectTransform.TransformPoint(localOffset);
            if (inputHandler == null || !inputHandler.HasBasePosition || RectTransform.parent == null)
            {
                return worldPoint;
            }

            Vector2 anchoredDelta = inputHandler.BaseAnchoredPosition - RectTransform.anchoredPosition;
            return worldPoint + RectTransform.parent.TransformVector(anchoredDelta);
        }

        public void PlaySwipeDismiss(Action onDone)
        {
            inputHandler.PlayFlyOutAndDestroy(onDone);
        }

        public void PlayTapDismiss(Action onDone)
        {
            StartCoroutine(TapDismiss(onDone));
        }

        public void HideForGameOver()
        {
            handled = true;
            OnAction = null;
            StopAllCoroutines();
            if (inputHandler != null)
            {
                inputHandler.StopAllCoroutines();
            }

            gameObject.SetActive(false);
        }

        private void Submit(TaskAction action)
        {
            if (handled)
            {
                return;
            }

            handled = true;
            OnAction?.Invoke(this, action);
        }

        private System.Collections.IEnumerator TapDismiss(Action onDone)
        {
            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = gameObject.AddComponent<CanvasGroup>();
            }

            RectTransform.localRotation = Quaternion.identity;
            Vector3 startScale = Vector3.one;
            Vector3 popScale = Vector3.one * tapPopScale;
            Vector3 endScale = Vector3.one * tapDismissScale;
            float totalDuration = Mathf.Max(0.01f, tapDisappearDuration);
            float popDuration = Mathf.Min(0.045f, totalDuration * 0.35f);
            float fadeDuration = Mathf.Max(0.01f, totalDuration - popDuration);

            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / popDuration);
                RectTransform.localScale = Vector3.Lerp(startScale, popScale, t);
                backgroundImage.color = Color.Lerp(baseBackgroundColor, tapFlashColor, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                RectTransform.localScale = Vector3.Lerp(popScale, endScale, t);
                backgroundImage.color = Color.Lerp(tapFlashColor, baseBackgroundColor, t);
                group.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }

            group.alpha = 0f;
            onDone?.Invoke();
        }
    }
}
