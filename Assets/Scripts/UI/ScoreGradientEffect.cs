using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class ScoreGradientEffect : BaseMeshEffect
    {
        [SerializeField] private Color topColor = Color.white;
        [SerializeField] private Color bottomColor = Color.white;
        [SerializeField] private bool rainbow;
        [SerializeField] private float rainbowSpeed = 0.28f;

        public void SetGradient(Color top, Color bottom)
        {
            topColor = top;
            bottomColor = bottom;
            rainbow = false;
            if (graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }

        public void SetRainbow()
        {
            rainbow = true;
            if (graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }

        private void Update()
        {
            if (rainbow && graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
            {
                return;
            }

            List<UIVertex> vertices = new();
            vh.GetUIVertexStream(vertices);
            if (vertices.Count == 0)
            {
                return;
            }

            float minX = vertices[0].position.x;
            float maxX = minX;
            float minY = vertices[0].position.y;
            float maxY = minY;
            for (int i = 1; i < vertices.Count; i++)
            {
                Vector3 position = vertices[i].position;
                minX = Mathf.Min(minX, position.x);
                maxX = Mathf.Max(maxX, position.x);
                minY = Mathf.Min(minY, position.y);
                maxY = Mathf.Max(maxY, position.y);
            }

            float width = Mathf.Max(1f, maxX - minX);
            float height = Mathf.Max(1f, maxY - minY);
            for (int i = 0; i < vertices.Count; i++)
            {
                UIVertex vertex = vertices[i];
                if (rainbow)
                {
                    float x01 = (vertex.position.x - minX) / width;
                    float hue = Mathf.Repeat(x01 + Time.unscaledTime * rainbowSpeed, 1f);
                    vertex.color = Color.HSVToRGB(hue, 0.88f, 1f);
                }
                else
                {
                    float y01 = (vertex.position.y - minY) / height;
                    vertex.color = Color.Lerp(bottomColor, topColor, y01);
                }

                vertices[i] = vertex;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertices);
        }
    }
}
