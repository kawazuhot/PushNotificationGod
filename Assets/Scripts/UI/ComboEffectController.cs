using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class ComboEffectController : MonoBehaviour
    {
        [SerializeField] private Text comboText;
        [SerializeField] private Image screenFlashImage;
        [SerializeField] private Image edgeFlashImage;
        [SerializeField] private float popDuration = 0.15f;
        [SerializeField] private float holdDuration = 0.25f;
        [SerializeField] private float fadeDuration = 0.25f;

        private Coroutine comboRoutine;
        private Coroutine flashRoutine;
        private Coroutine edgeRoutine;
        private CanvasGroup comboGroup;

        public void Configure(Text targetComboText, Image targetScreenFlashImage, Image targetEdgeFlashImage)
        {
            comboText = targetComboText;
            screenFlashImage = targetScreenFlashImage;
            edgeFlashImage = targetEdgeFlashImage;
        }

        public void PlayCombo(int combo, float multiplier)
        {
            if (comboText == null)
            {
                return;
            }

            EnsureComboGroup();
            comboText.text = "";
            comboGroup.alpha = 0f;

            if (combo < 5)
            {
                return;
            }

            if (combo == 10)
            {
                PlayScreenFlash(new Color(1f, 1f, 1f, 0.18f), 0.16f);
            }
            else if (combo >= 20 && combo % 20 == 0)
            {
                PlayScreenFlash(new Color(1f, 0.92f, 0.2f, 0.14f), 0.18f);
                PlayEdgeFlash(new Color(1f, 0.86f, 0.25f, 0.32f), 0.22f);
            }
            else if (combo == 5)
            {
                PlayScreenFlash(new Color(1f, 0.9f, 0.25f, 0.08f), 0.12f);
            }
        }

        public void ResetCombo()
        {
            if (comboText != null)
            {
                comboText.text = "";
                comboText.rectTransform.localScale = Vector3.one;
            }

            EnsureComboGroup();
            comboGroup.alpha = 0f;
        }

        private void EnsureComboGroup()
        {
            if (comboGroup != null || comboText == null)
            {
                return;
            }

            comboGroup = comboText.GetComponent<CanvasGroup>();
            if (comboGroup == null)
            {
                comboGroup = comboText.gameObject.AddComponent<CanvasGroup>();
            }
        }

        private static float EaseOut(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private void PlayScreenFlash(Color color, float duration)
        {
            if (screenFlashImage == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(Flash(screenFlashImage, color, duration));
        }

        private void PlayEdgeFlash(Color color, float duration)
        {
            if (edgeFlashImage == null)
            {
                return;
            }

            if (edgeRoutine != null)
            {
                StopCoroutine(edgeRoutine);
            }

            edgeRoutine = StartCoroutine(Flash(edgeFlashImage, color, duration));
        }

        private static System.Collections.IEnumerator Flash(Image image, Color color, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Color current = color;
                current.a = color.a * (1f - t);
                image.color = current;
                yield return null;
            }

            image.color = new Color(color.r, color.g, color.b, 0f);
        }
    }
}
