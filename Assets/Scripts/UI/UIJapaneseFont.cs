using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public static class UIJapaneseFont
    {
        private const string JapaneseFontResourcePath = "Fonts/HiraginoKakuGothicW6";
        private const string FallbackJapaneseFontResourcePath = "Fonts/AppleGothic";
        private static Font cachedFont;

        public static Font Get()
        {
            if (cachedFont != null)
            {
                return cachedFont;
            }

            cachedFont = Resources.Load<Font>(JapaneseFontResourcePath);
            if (cachedFont == null)
            {
                cachedFont = Resources.Load<Font>(FallbackJapaneseFontResourcePath);
            }

            if (cachedFont == null)
            {
                Debug.LogWarning($"Japanese UI font not found at Resources/{JapaneseFontResourcePath} or Resources/{FallbackJapaneseFontResourcePath}. Falling back to LegacyRuntime.");
                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return cachedFont;
        }

        public static void ApplyToSceneTexts()
        {
            Font font = Get();
            if (font == null)
            {
                return;
            }

            foreach (Text text in Resources.FindObjectsOfTypeAll<Text>())
            {
                if (text == null || !text.gameObject.scene.IsValid())
                {
                    continue;
                }

                text.font = font;
            }
        }

        public static void Apply(Text text)
        {
            if (text != null)
            {
                text.font = Get();
            }
        }
    }
}
