using System.Collections.Generic;
using UnityEngine;

namespace PushNotificationGod.Data
{
    public class IconDatabase : MonoBehaviour
    {
        [System.Serializable]
        public class IconEntry
        {
            public string iconId;
            public Sprite sprite;
        }

        [SerializeField] private Sprite defaultIcon;
        [SerializeField] private List<IconEntry> entries = new();

        private Dictionary<string, Sprite> iconMap;
        private readonly HashSet<string> warnedMissingIconIds = new();

        public Sprite GetIcon(string iconId)
        {
            EnsureMap();
            if (!string.IsNullOrEmpty(iconId) && iconMap.TryGetValue(iconId, out Sprite sprite) && sprite != null)
            {
                return sprite;
            }

            if (!string.IsNullOrEmpty(iconId) && warnedMissingIconIds.Add(iconId))
            {
                Debug.LogWarning($"[IconManager] Missing iconId: {iconId}. Use default icon.");
            }

            return defaultIcon;
        }

        private void EnsureMap()
        {
            if (iconMap != null)
            {
                return;
            }

            iconMap = new Dictionary<string, Sprite>();
            foreach (IconEntry entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.iconId) || entry.sprite == null)
                {
                    continue;
                }

                iconMap[entry.iconId] = entry.sprite;
            }
        }
    }
}
