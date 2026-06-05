using PushNotificationGod.Audio;
using PushNotificationGod.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class FeedbackManager : MonoBehaviour
    {
        [SerializeField] private RectTransform feedbackRoot;
        [SerializeField] private FloatingScoreText floatingScorePrefab;
        [SerializeField] private RectTransform scoreGroup;
        [SerializeField] private ComboEffectController comboEffectController;
        [SerializeField] private AudioManager audioManager;

        private Coroutine scorePopRoutine;

        public void Configure(RectTransform parent, Text scoreText, Text comboText, AudioManager manager)
        {
            if (audioManager == null)
            {
                audioManager = manager;
            }

            if (scoreGroup == null && scoreText != null)
            {
                scoreGroup = scoreText.transform.parent as RectTransform;
            }

            if (feedbackRoot == null && parent != null)
            {
                GameObject feedbackLayer = new("FeedbackLayer", typeof(RectTransform));
                feedbackLayer.transform.SetParent(parent, false);
                feedbackRoot = feedbackLayer.GetComponent<RectTransform>();
                feedbackRoot.anchorMin = Vector2.zero;
                feedbackRoot.anchorMax = Vector2.one;
                feedbackRoot.offsetMin = Vector2.zero;
                feedbackRoot.offsetMax = Vector2.zero;
            }

            if (comboEffectController == null && comboText != null)
            {
                comboEffectController = comboText.GetComponent<ComboEffectController>();
                if (comboEffectController == null)
                {
                    comboEffectController = comboText.gameObject.AddComponent<ComboEffectController>();
                }

                Image screenFlash = CreateFlashImage("CorrectScreenFlash", new Color(1f, 1f, 1f, 0f));
                Image edgeFlash = CreateFlashImage("ComboEdgeFlash", new Color(1f, 0.86f, 0.25f, 0f));
                comboEffectController.Configure(comboText, screenFlash, edgeFlash);
            }

            if (floatingScorePrefab == null && feedbackRoot != null && scoreText != null)
            {
                floatingScorePrefab = CreateFloatingScoreTemplate(scoreText.font);
            }
        }

        public void PlayCorrectFeedback(TaskCard card, TaskAction action, int gainedScore, int combo, float multiplier)
        {
            PlayScoreGainFeedback(card, gainedScore);
            PopScoreGroup();
            PlayComboFeedback(combo, multiplier);
        }

        public void PlayScoreGainFeedback(TaskCard card, int gainedScore)
        {
            if (card == null)
            {
                return;
            }

            ShowFloatingScore(gainedScore, card.GetStableScoreWorldPosition());
        }

        public void ShowFloatingScore(int score, Vector3 basePosition)
        {
            if (floatingScorePrefab == null || feedbackRoot == null)
            {
                return;
            }

            FloatingScoreText floatingScore = Instantiate(floatingScorePrefab, feedbackRoot);
            floatingScore.gameObject.SetActive(true);
            RectTransform rect = (RectTransform)floatingScore.transform;
            rect.anchoredPosition = (Vector2)feedbackRoot.InverseTransformPoint(basePosition);
            floatingScore.Play(score, score >= 300);
            audioManager?.PlayScorePop();
        }

        public void PlayComboFeedback(int combo, float multiplier)
        {
            comboEffectController?.PlayCombo(combo, multiplier);

            if (combo == 5)
            {
                audioManager?.PlayCombo5();
            }
            else if (combo == 10)
            {
                audioManager?.PlayCombo10();
            }
            else if (combo >= 20 && combo % 20 == 0)
            {
                audioManager?.PlayCombo20();
            }
            else if (combo >= 2)
            {
                audioManager?.PlayCombo();
            }
        }

        public void PlayMistakeFeedback()
        {
            comboEffectController?.ResetCombo();
        }

        private void PopScoreGroup()
        {
            if (scoreGroup == null)
            {
                return;
            }

            if (scorePopRoutine != null)
            {
                StopCoroutine(scorePopRoutine);
            }

            scorePopRoutine = StartCoroutine(PopScore());
        }

        private System.Collections.IEnumerator PopScore()
        {
            const float duration = 0.18f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = t < 0.5f
                    ? Mathf.Lerp(1f, 1.08f, t / 0.5f)
                    : Mathf.Lerp(1.08f, 1f, (t - 0.5f) / 0.5f);
                scoreGroup.localScale = Vector3.one * scale;
                yield return null;
            }

            scoreGroup.localScale = Vector3.one;
        }

        private Image CreateFlashImage(string objectName, Color color)
        {
            if (feedbackRoot == null)
            {
                return null;
            }

            GameObject flashObject = new(objectName, typeof(RectTransform), typeof(Image));
            flashObject.transform.SetParent(feedbackRoot, false);
            RectTransform rect = flashObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = flashObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private FloatingScoreText CreateFloatingScoreTemplate(Font font)
        {
            GameObject template = new("FloatingScoreTextTemplate", typeof(RectTransform), typeof(Text), typeof(CanvasGroup), typeof(Shadow), typeof(Outline), typeof(FloatingScoreText));
            template.transform.SetParent(feedbackRoot, false);
            Text text = template.GetComponent<Text>();
            text.text = "+100";
            text.font = font;
            text.fontSize = 92;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.93f, 0.18f, 1f);
            text.raycastTarget = false;
            template.GetComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);
            template.GetComponent<Shadow>().effectDistance = new Vector2(0f, -5f);
            template.GetComponent<Outline>().effectColor = new Color(0.15f, 0.08f, 0f, 0.9f);
            template.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
            RectTransform rect = template.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 120f);
            FloatingScoreText floatingScore = template.GetComponent<FloatingScoreText>();
            floatingScore.Configure(text, template.GetComponent<CanvasGroup>());
            template.SetActive(false);
            return floatingScore;
        }
    }
}
