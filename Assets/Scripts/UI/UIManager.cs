using System;
using PushNotificationGod.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Text dateText;
        [SerializeField] private Text remainingTimeText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text lifeText;
        [SerializeField] private Text comboText;

        private ScoreManager scoreManager;
        private LifeManager lifeManager;
        private ComboManager comboManager;
        private TimerManager timerManager;
        private ScoreGradientEffect scoreGradientEffect;
        private Coroutine scoreTierPopRoutine;
        private int currentScoreTier = -1;
        private int lastDisplayedScore = int.MinValue;

        public Text ScoreText => scoreText;
        public Text ComboText => comboText;
        public RectTransform FeedbackParent => comboText != null ? (RectTransform)comboText.transform.parent : null;

        private void Start()
        {
            UIJapaneseFont.ApplyToSceneTexts();
            ConfigureBottomScoreLayout();
            EnsureScoreVisualComponents();
            UIJapaneseFont.ApplyToSceneTexts();
        }

        public void Bind(ScoreManager score, LifeManager life, ComboManager combo, TimerManager timer)
        {
            scoreManager = score;
            lifeManager = life;
            comboManager = combo;
            timerManager = timer;
        }

        private void Update()
        {
            if (dateText != null)
            {
                dateText.text = DateTime.Now.ToString("M月d日（ddd）");
            }

            if (remainingTimeText != null && timerManager != null)
            {
                remainingTimeText.text = FormatRemainingTime(timerManager.RemainingSeconds);
            }

            if (scoreText != null && scoreManager != null)
            {
                if (lastDisplayedScore != scoreManager.Score)
                {
                    lastDisplayedScore = scoreManager.Score;
                    scoreText.text = scoreManager.Score.ToString();
                    UpdateScoreVisual(scoreManager.Score);
                }
            }

            if (lifeText != null && lifeManager != null)
            {
                lifeText.text = $"LIFE {lifeManager.CurrentLife}";
            }

            // Combo text is animated by FeedbackManager so it can pop and show milestone messages.
        }

        private static string FormatRemainingTime(float remainingSeconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes}:{seconds:00}";
        }

        private void UpdateScoreVisual(int score)
        {
            if (scoreText == null)
            {
                return;
            }

            EnsureScoreVisualComponents();
            int tier = ScoreColorUtility.GetScoreTier(score);
            bool tierChanged = tier != currentScoreTier;
            if (tierChanged)
            {
                currentScoreTier = tier;
            }

            ScoreColorUtility.ApplyScoreVisual(scoreText, score);

            if (tierChanged && tier > 0)
            {
                PlayScoreTierUpEffect();
            }
        }

        private void PlayScoreTierUpEffect()
        {
            if (scoreText == null)
            {
                return;
            }

            if (scoreTierPopRoutine != null)
            {
                StopCoroutine(scoreTierPopRoutine);
            }

            scoreTierPopRoutine = StartCoroutine(ScoreTierPop());
        }

        private System.Collections.IEnumerator ScoreTierPop()
        {
            const float duration = 0.22f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = t < 0.45f
                    ? Mathf.Lerp(1f, 1.15f, t / 0.45f)
                    : Mathf.Lerp(1.15f, 1f, (t - 0.45f) / 0.55f);
                scoreText.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            scoreText.transform.localScale = Vector3.one;
        }

        private void ConfigureBottomScoreLayout()
        {
            if (scoreText == null || scoreText.transform.parent.name == "ScoreGroup")
            {
                return;
            }

            Transform safeParent = scoreText.transform.parent;
            GameObject scoreGroup = new("ScoreGroup", typeof(RectTransform));
            scoreGroup.transform.SetParent(safeParent, false);
            RectTransform groupRect = scoreGroup.GetComponent<RectTransform>();
            groupRect.anchorMin = new Vector2(0.5f, 0f);
            groupRect.anchorMax = new Vector2(0.5f, 0f);
            groupRect.pivot = new Vector2(0.5f, 0f);
            groupRect.anchoredPosition = new Vector2(0f, 28f);
            groupRect.sizeDelta = new Vector2(560f, 160f);

            Text label = new GameObject("ScoreLabelText", typeof(RectTransform), typeof(Text), typeof(Shadow)).GetComponent<Text>();
            label.transform.SetParent(scoreGroup.transform, false);
            label.text = "SCORE";
            label.font = scoreText.font;
            label.fontStyle = FontStyle.Bold;
            label.fontSize = 28;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 1f, 1f, 0.82f);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.GetComponent<Shadow>().effectDistance = new Vector2(0f, -2f);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.offsetMin = new Vector2(0f, -42f);
            labelRect.offsetMax = Vector2.zero;

            scoreText.transform.SetParent(scoreGroup.transform, false);
            scoreText.name = "ScoreValueText";
            scoreText.text = "0";
            scoreText.fontStyle = FontStyle.Bold;
            scoreText.fontSize = 82;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.color = Color.white;
            if (scoreText.GetComponent<Shadow>() == null)
            {
                scoreText.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0f, -4f);
            }

            RectTransform valueRect = scoreText.rectTransform;
            valueRect.anchorMin = new Vector2(0f, 0f);
            valueRect.anchorMax = new Vector2(1f, 0f);
            valueRect.pivot = new Vector2(0.5f, 0f);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = new Vector2(0f, 104f);
            EnsureScoreVisualComponents();
        }

        private void EnsureScoreVisualComponents()
        {
            if (scoreText == null)
            {
                return;
            }

            ScoreColorUtility.ApplyReadableEffects(scoreText);

            if (scoreGradientEffect == null)
            {
                scoreGradientEffect = scoreText.GetComponent<ScoreGradientEffect>();
                if (scoreGradientEffect == null)
                {
                    scoreGradientEffect = scoreText.gameObject.AddComponent<ScoreGradientEffect>();
                }
            }
        }
    }
}
