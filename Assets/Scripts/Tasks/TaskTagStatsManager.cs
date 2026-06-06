using System.Collections.Generic;
using PushNotificationGod.Data;
using UnityEngine;

namespace PushNotificationGod.Tasks
{
    public class TaskTagStatsManager : MonoBehaviour
    {
        private readonly Dictionary<string, TaskTagStats> statsByTag = new();

        public void ResetStats()
        {
            statsByTag.Clear();
        }

        public void RecordAction(TaskDefinition task, TaskAction action)
        {
            if (task == null || !IsTrackableTag(task.tag))
            {
                return;
            }

            string normalizedTag = NormalizeTag(task.tag);
            if (!statsByTag.TryGetValue(normalizedTag, out TaskTagStats stats))
            {
                stats = new TaskTagStats(normalizedTag);
                statsByTag.Add(normalizedTag, stats);
            }

            if (action == TaskAction.Tap)
            {
                stats.tapCount++;
            }
            else if (action == TaskAction.SwipeRight)
            {
                stats.swipeCount++;
            }

            Debug.Log($"[TagStats] Record tag={stats.tag} action={action} tap={stats.tapCount} swipe={stats.swipeCount}");
        }

        public TaskTagStats GetStats(string tag)
        {
            string normalizedTag = NormalizeTag(tag);
            return statsByTag.TryGetValue(normalizedTag, out TaskTagStats stats) ? stats : null;
        }

        public Dictionary<string, TaskTagStats> GetAllStats()
        {
            return new Dictionary<string, TaskTagStats>(statsByTag);
        }

        public void LogStats()
        {
            foreach (TaskTagStats stats in statsByTag.Values)
            {
                Debug.Log($"[TagStats] {stats.tag} tap={stats.tapCount} swipe={stats.swipeCount}");
            }
        }

        public static bool IsTrackableTag(string tag)
        {
            string normalizedTag = NormalizeTag(tag);
            return !string.IsNullOrEmpty(normalizedTag) && normalizedTag != "none" && normalizedTag != "null";
        }

        private static string NormalizeTag(string tag)
        {
            return string.IsNullOrWhiteSpace(tag) ? string.Empty : tag.Trim();
        }
    }
}
