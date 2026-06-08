using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public static class ScoreColorUtility
    {
        public static int GetScoreTier(int score)
        {
            if (score >= 60000)
            {
                return 4;
            }

            if (score >= 30000)
            {
                return 3;
            }

            if (score >= 10000)
            {
                return 2;
            }

            if (score >= 5000)
            {
                return 1;
            }

            return 0;
        }

        public static int GetComboTier(int combo)
        {
            if (combo >= 100)
            {
                return 5;
            }

            if (combo >= 31)
            {
                return 3;
            }

            if (combo >= 21)
            {
                return 2;
            }

            if (combo >= 11)
            {
                return 1;
            }

            return 0;
        }

        public static void ApplyScoreVisual(Text text, int score)
        {
            if (text == null)
            {
                return;
            }

            int tier = GetScoreTier(score);
            ApplyReadableEffects(text);
            ApplyTierVisual(text, tier);
        }

        public static void ApplyComboVisual(Text text, int combo)
        {
            if (text == null)
            {
                return;
            }

            int tier = GetComboTier(combo);
            ApplyReadableEffects(text);
            ApplyComboTierVisual(text, tier);
        }

        public static void ApplyReadableEffects(Text text)
        {
            Shadow shadow = text.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = text.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = new Color(0f, 0f, 0f, 0.58f);
            shadow.effectDistance = new Vector2(0f, -4f);

            Outline outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0f, 0f, 0f, 0.32f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private static void ApplyTierVisual(Text text, int tier)
        {
            ScoreGradientEffect gradient = text.GetComponent<ScoreGradientEffect>();
            if (gradient == null)
            {
                gradient = text.gameObject.AddComponent<ScoreGradientEffect>();
            }

            text.color = Color.white;
            gradient.enabled = tier > 0;

            switch (tier)
            {
                case 1:
                    gradient.SetGradient(new Color(0.68f, 0.96f, 1f), new Color(0.08f, 0.42f, 1f));
                    break;
                case 2:
                    gradient.SetGradient(new Color(1f, 0.98f, 0.36f), new Color(1f, 0.62f, 0.02f));
                    break;
                case 3:
                    gradient.SetGradient(new Color(1f, 0.58f, 0.08f), new Color(1f, 0.05f, 0.04f));
                    break;
                case 4:
                    gradient.SetRainbow();
                    break;
            }
        }

        private static void ApplyComboTierVisual(Text text, int tier)
        {
            ScoreGradientEffect gradient = text.GetComponent<ScoreGradientEffect>();
            if (gradient == null)
            {
                gradient = text.gameObject.AddComponent<ScoreGradientEffect>();
            }

            text.color = Color.white;
            gradient.enabled = tier > 0;

            switch (tier)
            {
                case 1:
                    gradient.SetGradient(new Color(1f, 0.98f, 0.36f), new Color(1f, 0.62f, 0.02f));
                    break;
                case 2:
                    gradient.SetGradient(new Color(0.82f, 1f, 0.28f), new Color(0.02f, 0.74f, 0.18f));
                    break;
                case 3:
                    gradient.SetGradient(new Color(1f, 0.58f, 0.08f), new Color(1f, 0.05f, 0.04f));
                    break;
                case 5:
                    gradient.SetRainbow();
                    break;
            }
        }
    }
}
