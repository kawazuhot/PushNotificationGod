using System;
using System.Collections.Generic;
using PushNotificationGod.Tasks;
using UnityEngine;

namespace PushNotificationGod.Data
{
    public class TaskDatabase : MonoBehaviour
    {
        [SerializeField] private TextAsset taskCsv;
        [SerializeField] private int recentTaskHistoryLimit = 10;

        private readonly List<TaskDefinition> tasks = new();
        private readonly Queue<string> recentTaskIds = new();

        public IReadOnlyList<TaskDefinition> Tasks => tasks;
        public int Count => tasks.Count;

        public void Load()
        {
            tasks.Clear();
            recentTaskIds.Clear();

            if (taskCsv == null)
            {
                Debug.LogError("TaskDatabase: task CSV is not assigned.");
                return;
            }

            string[] lines = taskCsv.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = ParseCsvLine(lines[i]);
                if (columns.Length < 8)
                {
                    Debug.LogWarning($"TaskDatabase: skipped malformed CSV line {i + 1}: {lines[i]}");
                    continue;
                }

                if (!Enum.TryParse(columns[4], out TaskAction action))
                {
                    Debug.LogWarning($"TaskDatabase: skipped task with invalid action: {columns[0]}");
                    continue;
                }

                TaskDefinition task = new()
                {
                    taskId = columns[0],
                    appName = columns[1],
                    iconId = columns[2],
                    messageText = columns[3],
                    correctAction = action,
                    baseScore = ParseInt(columns[5], 100),
                    backgroundStyleId = columns[6],
                    spawnWeight = Mathf.Max(0, ParseInt(columns[7], 100)),
                    tag = columns.Length >= 9 ? columns[8].Trim() : string.Empty
                };

                tasks.Add(task);
                if (!string.IsNullOrEmpty(task.tag))
                {
                    Debug.Log($"[TaskLoad] {task.taskId} tag={task.tag}");
                }
            }

            Debug.Log($"TaskDatabase: loaded {tasks.Count} tasks.");
        }

        public TaskDefinition PickRandom()
        {
            if (tasks.Count == 0)
            {
                return null;
            }

            List<TaskDefinition> candidates = new();
            foreach (TaskDefinition task in tasks)
            {
                if (!recentTaskIds.Contains(task.taskId))
                {
                    candidates.Add(task);
                }
            }

            if (candidates.Count == 0)
            {
                candidates.AddRange(tasks);
            }

            TaskDefinition picked = PickWeighted(candidates);
            Remember(picked.taskId);
            return picked;
        }

        private TaskDefinition PickWeighted(List<TaskDefinition> candidates)
        {
            int totalWeight = 0;
            foreach (TaskDefinition task in candidates)
            {
                totalWeight += Mathf.Max(0, task.spawnWeight);
            }

            if (totalWeight <= 0)
            {
                return candidates[UnityEngine.Random.Range(0, candidates.Count)];
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);
            foreach (TaskDefinition task in candidates)
            {
                roll -= Mathf.Max(0, task.spawnWeight);
                if (roll < 0)
                {
                    return task;
                }
            }

            return candidates[^1];
        }

        private void Remember(string taskId)
        {
            recentTaskIds.Enqueue(taskId);
            while (recentTaskIds.Count > recentTaskHistoryLimit)
            {
                recentTaskIds.Dequeue();
            }
        }

        private static int ParseInt(string value, int fallback)
        {
            return int.TryParse(value, out int parsed) ? parsed : fallback;
        }

        private static string[] ParseCsvLine(string line)
        {
            List<string> values = new();
            bool inQuotes = false;
            System.Text.StringBuilder current = new();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            values.Add(current.ToString());
            return values.ToArray();
        }
    }
}
