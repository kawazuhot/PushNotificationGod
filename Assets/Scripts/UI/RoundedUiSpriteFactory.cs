using UnityEngine;

namespace PushNotificationGod.UI
{
    public static class RoundedUiSpriteFactory
    {
        private static Sprite cachedSprite;

        public static Sprite Get()
        {
            if (cachedSprite != null)
            {
                return cachedSprite;
            }

            const int width = 256;
            const int height = 96;
            const int radius = 34;
            const int border = 36;

            Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
            {
                name = "RuntimeRoundedUiTexture",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color clear = new(1f, 1f, 1f, 0f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, IsInsideRoundedRect(x + 0.5f, y + 0.5f, width, height, radius) ? Color.white : clear);
                }
            }

            texture.Apply();
            cachedSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
            cachedSprite.name = "RuntimeRoundedUiSprite";
            return cachedSprite;
        }

        private static bool IsInsideRoundedRect(float x, float y, int width, int height, int radius)
        {
            float left = radius;
            float right = width - radius;
            float bottom = radius;
            float top = height - radius;
            float closestX = Mathf.Clamp(x, left, right);
            float closestY = Mathf.Clamp(y, bottom, top);
            float dx = x - closestX;
            float dy = y - closestY;
            return dx * dx + dy * dy <= radius * radius;
        }
    }
}
