using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class LastSecondsWarningController : MonoBehaviour
    {
        [SerializeField] private Image overlayImage;
        [SerializeField] private float pulseDuration = 1f;
        [SerializeField] private float minAlpha = 0.07f;
        [SerializeField] private float maxAlpha = 0.32f;

        private bool warningActive;

        public void Configure(Image targetOverlayImage)
        {
            overlayImage = targetOverlayImage;
            StopWarning();
        }

        public void StartWarning()
        {
            if (overlayImage == null)
            {
                return;
            }

            if (!warningActive)
            {
                warningActive = true;
                overlayImage.gameObject.SetActive(true);
            }

            UpdateWarningPulse();
        }

        public void StopWarning()
        {
            warningActive = false;
            if (overlayImage == null)
            {
                return;
            }

            overlayImage.color = new Color(1f, 0f, 0f, 0f);
            overlayImage.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!warningActive)
            {
                return;
            }

            UpdateWarningPulse();
        }

        private void UpdateWarningPulse()
        {
            if (overlayImage == null)
            {
                return;
            }

            float duration = Mathf.Max(0.01f, pulseDuration);
            float phase = Mathf.PingPong(Time.unscaledTime, duration * 0.5f) / (duration * 0.5f);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, phase);
            overlayImage.color = new Color(1f, 0.02f, 0f, alpha);
        }
    }
}
