using UnityEngine;

namespace PushNotificationGod.UI
{
    public class TitleLogoBreath : MonoBehaviour
    {
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float breathScaleMultiplier = 1.045f;
        [SerializeField] private float breathDuration = 0.8f;

        private RectTransform rectTransform;
        private float timeOffset;

        public void Configure(float scale, float multiplier, float duration)
        {
            baseScale = scale;
            breathScaleMultiplier = multiplier;
            breathDuration = Mathf.Max(0.1f, duration);
        }

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            timeOffset = Random.Range(0f, breathDuration);
        }

        private void Update()
        {
            if (rectTransform == null)
            {
                return;
            }

            float cycle = Mathf.PingPong((Time.unscaledTime + timeOffset) / Mathf.Max(0.1f, breathDuration), 1f);
            float eased = cycle * cycle * (3f - 2f * cycle);
            float scale = Mathf.Lerp(baseScale, baseScale * breathScaleMultiplier, eased);
            rectTransform.localScale = Vector3.one * scale;
        }
    }
}
