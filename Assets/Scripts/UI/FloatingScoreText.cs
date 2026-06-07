using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class FloatingScoreText : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float riseDistance = 50f;
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private int normalFontSize = 92;
        [SerializeField] private int goldFontSize = 108;
        [SerializeField] private Color normalScoreColor = new(1f, 0.93f, 0.18f, 1f);
        [SerializeField] private Color goldScoreColor = new(1f, 0.58f, 0.05f, 1f);

        private RectTransform rectTransform;
        private Vector2 startPosition;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            if (scoreText == null)
            {
                scoreText = GetComponent<Text>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void Play(int gainedScore, bool isGold)
        {
            scoreText.text = $"+{gainedScore}";
            scoreText.fontSize = isGold ? goldFontSize : normalFontSize;
            scoreText.color = isGold ? goldScoreColor : normalScoreColor;
            startPosition = rectTransform.anchoredPosition;
            StartCoroutine(Animate());
        }

        public void Configure(Text targetScoreText, CanvasGroup targetCanvasGroup)
        {
            scoreText = targetScoreText;
            canvasGroup = targetCanvasGroup;
        }

        private System.Collections.IEnumerator Animate()
        {
            float elapsed = 0f;
            Vector2 endPosition = startPosition + new Vector2(0f, riseDistance);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, eased);
                rectTransform.localScale = Vector3.one * ScoreScale(t);
                canvasGroup.alpha = t < 0.45f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.45f) / 0.55f);
                yield return null;
            }

            Destroy(gameObject);
        }

        private static float ScoreScale(float t)
        {
            if (t < 0.22f)
            {
                return Mathf.Lerp(0.8f, 1.25f, t / 0.22f);
            }

            if (t < 0.42f)
            {
                return Mathf.Lerp(1.25f, 1.05f, (t - 0.22f) / 0.2f);
            }

            return Mathf.Lerp(1.05f, 0.98f, (t - 0.42f) / 0.58f);
        }
    }
}
