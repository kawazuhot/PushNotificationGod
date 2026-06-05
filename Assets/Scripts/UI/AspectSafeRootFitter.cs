using System.Collections.Generic;
using UnityEngine;

namespace PushNotificationGod.UI
{
    [RequireComponent(typeof(Canvas))]
    public class AspectSafeRootFitter : MonoBehaviour
    {
        [SerializeField] private Vector2 referenceResolution = new(1080f, 1920f);
        [SerializeField] private string rootName = "AspectSafeRoot";

        private RectTransform canvasRect;
        private RectTransform aspectRoot;

        private void Awake()
        {
            canvasRect = (RectTransform)transform;
            EnsureAspectRoot();
            MoveExistingChildrenIntoRoot();
            FitRoot();
        }

        private void Start()
        {
            FitRoot();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (canvasRect == null)
            {
                canvasRect = (RectTransform)transform;
            }

            FitRoot();
        }

        private void EnsureAspectRoot()
        {
            Transform existing = transform.Find(rootName);
            if (existing != null)
            {
                aspectRoot = (RectTransform)existing;
                return;
            }

            GameObject root = new(rootName, typeof(RectTransform));
            root.transform.SetParent(transform, false);
            aspectRoot = root.GetComponent<RectTransform>();
        }

        private void MoveExistingChildrenIntoRoot()
        {
            List<Transform> children = new();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != aspectRoot)
                {
                    children.Add(child);
                }
            }

            foreach (Transform child in children)
            {
                child.SetParent(aspectRoot, false);
            }

            aspectRoot.SetAsFirstSibling();
        }

        private void FitRoot()
        {
            if (aspectRoot == null)
            {
                return;
            }

            float width = Mathf.Max(1f, canvasRect.rect.width);
            float height = Mathf.Max(1f, canvasRect.rect.height);
            float scale = Mathf.Min(width / referenceResolution.x, height / referenceResolution.y);

            aspectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            aspectRoot.anchorMax = new Vector2(0.5f, 0.5f);
            aspectRoot.pivot = new Vector2(0.5f, 0.5f);
            aspectRoot.anchoredPosition = Vector2.zero;
            aspectRoot.sizeDelta = referenceResolution;
            aspectRoot.localScale = Vector3.one * scale;
        }
    }
}
