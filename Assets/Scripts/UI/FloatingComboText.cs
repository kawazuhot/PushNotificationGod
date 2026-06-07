using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class FloatingComboText : MonoBehaviour
    {
        [SerializeField] private Text comboText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float riseDistance = 42f;
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private int fontSize = 62;

        private RectTransform rectTransform;
        private Vector2 startPosition;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            if (comboText == null)
            {
                comboText = GetComponent<Text>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void Play(int combo)
        {
            if (comboText == null || canvasGroup == null)
            {
                Destroy(gameObject);
                return;
            }

            comboText.supportRichText = true;
            comboText.text = ComboMessage(combo);
            comboText.fontSize = fontSize;
            comboText.fontStyle = FontStyle.Bold;
            comboText.alignment = TextAnchor.MiddleCenter;
            comboText.color = Color.white;
            startPosition = rectTransform.anchoredPosition;
            StartCoroutine(Animate());
        }

        private static string ComboMessage(int combo)
        {
            if (combo >= 50)
            {
                return $"<color=#FF4D4D>{combo}</color> <color=#FFD84A>COM</color><color=#36F57A>BO!!</color>";
            }

            string color = GetComboColorHex(combo);
            return $"<color={color}>{combo} COMBO!!</color>";
        }

        private static string GetComboColorHex(int combo)
        {
            if (combo >= 40)
            {
                return "#FF3A2E";
            }

            if (combo >= 30)
            {
                return "#35F06A";
            }

            if (combo >= 20)
            {
                return "#FFD83D";
            }

            if (combo >= 10)
            {
                return "#4DBDFF";
            }

            return "#FFFFFF";
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
                rectTransform.localScale = Vector3.one * ComboScale(t);
                canvasGroup.alpha = t < 0.45f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.45f) / 0.55f);
                yield return null;
            }

            Destroy(gameObject);
        }

        private static float ComboScale(float t)
        {
            if (t < 0.22f)
            {
                return Mathf.Lerp(0.8f, 1.2f, t / 0.22f);
            }

            if (t < 0.42f)
            {
                return Mathf.Lerp(1.2f, 1f, (t - 0.22f) / 0.2f);
            }

            return Mathf.Lerp(1f, 0.96f, (t - 0.42f) / 0.58f);
        }
    }
}
