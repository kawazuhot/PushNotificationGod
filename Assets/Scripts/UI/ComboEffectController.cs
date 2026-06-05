using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class ComboEffectController : MonoBehaviour
    {
        [SerializeField] private Text comboText;
        [SerializeField] private Image screenFlashImage;
        [SerializeField] private Image edgeFlashImage;
        [SerializeField] private float comboPopDuration = 0.18f;

        private Coroutine comboRoutine;
        private Coroutine flashRoutine;
        private Coroutine edgeRoutine;

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

            if (combo < 2)
            {
                comboText.text = "";
                return;
            }

            comboText.text = ComboMessage(combo, multiplier);
            comboText.fontSize = combo >= 20 ? 54 : combo >= 10 ? 50 : combo >= 5 ? 44 : 36;
            comboText.color = combo >= 5 ? new Color(1f, 0.9f, 0.28f, 1f) : new Color(1f, 0.86f, 0.36f, 1f);

            if (comboRoutine != null)
            {
                StopCoroutine(comboRoutine);
            }

            comboRoutine = StartCoroutine(PopCombo(combo >= 20 ? 1.28f : combo >= 10 ? 1.22f : combo >= 5 ? 1.18f : 1.15f));

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
        }

        private string ComboMessage(int combo, float multiplier)
        {
            if (combo >= 20)
            {
                return $"{combo} COMBO!!! x2.0";
            }

            if (combo >= 10)
            {
                return $"{combo} COMBO!! x1.5";
            }

            if (combo >= 5)
            {
                return $"{combo} COMBO!";
            }

            return $"{combo} COMBO";
        }

        private System.Collections.IEnumerator PopCombo(float peakScale)
        {
            RectTransform rect = comboText.rectTransform;
            float elapsed = 0f;
            while (elapsed < comboPopDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / comboPopDuration);
                float scale = t < 0.5f
                    ? Mathf.Lerp(1f, peakScale, t / 0.5f)
                    : Mathf.Lerp(peakScale, 1f, (t - 0.5f) / 0.5f);
                rect.localScale = Vector3.one * scale;
                yield return null;
            }

            rect.localScale = Vector3.one;
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
