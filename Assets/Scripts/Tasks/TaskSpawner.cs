using PushNotificationGod.Audio;
using PushNotificationGod.Core;
using PushNotificationGod.Data;
using UnityEngine;

namespace PushNotificationGod.Tasks
{
    public class TaskSpawner : MonoBehaviour
    {
        [SerializeField] private TaskDatabase taskDatabase;
        [SerializeField] private TaskManager taskManager;
        [SerializeField] private TimerManager timerManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private int initialTaskCount = 2;
        [SerializeField] private int maxVisibleBeforeSpawn = 4;

        private bool running;
        private bool isSpawning;
        private int lastLoggedVisibleCount = -1;
        private int lastLoggedTargetTaskCount = -1;

        public void Begin()
        {
            running = true;
            isSpawning = false;
            lastLoggedVisibleCount = -1;
            lastLoggedTargetTaskCount = -1;
            Debug.Log($"[{BuildInfo.BuildId}] TaskSpawner.Begin fixed-count mode. initialTaskCount={initialTaskCount}, maxVisible={maxVisibleBeforeSpawn}");

            AdjustToTargetCount(GetTargetTaskCountByElapsedTime(0f), "initial");
            Debug.Log($"[{BuildInfo.BuildId}] Initial tasks spawned. activeTasks.Count={taskManager.VisibleCount}");
        }

        public void Stop()
        {
            running = false;
            Debug.Log($"[{BuildInfo.BuildId}] TaskSpawner.Stop called. Refill disabled. activeTasks.Count={(taskManager != null ? taskManager.VisibleCount : -1)}");
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }

            int visibleCount = taskManager != null ? taskManager.VisibleCount : -1;
            if (visibleCount != lastLoggedVisibleCount)
            {
                lastLoggedVisibleCount = visibleCount;
                Debug.Log($"[{BuildInfo.BuildId}] activeTasks.Count changed. activeTasks.Count={visibleCount}");
            }

            int targetTaskCount = GetCurrentTargetTaskCount();
            if (targetTaskCount != lastLoggedTargetTaskCount)
            {
                lastLoggedTargetTaskCount = targetTaskCount;
                Debug.Log($"[{BuildInfo.BuildId}] Target task count changed. elapsed={GetElapsedSeconds():F1}, targetTaskCount={targetTaskCount}");
            }

            AdjustToTargetCount(targetTaskCount, "fixed-count-adjust");
        }

        private void AdjustToTargetCount(int targetCount, string reason)
        {
            if (!running)
            {
                Debug.Log($"[{BuildInfo.BuildId}] Fixed-count adjustment skipped because game is not running.");
                return;
            }

            if (taskManager == null)
            {
                return;
            }

            int cappedTarget = maxVisibleBeforeSpawn > 0 ? Mathf.Min(targetCount, maxVisibleBeforeSpawn) : targetCount;
            while (taskManager.VisibleCount < cappedTarget && CanSpawn())
            {
                TrySpawnOne(reason);
            }

            while (taskManager.VisibleCount > cappedTarget)
            {
                taskManager.RemoveTopMost();
                Debug.Log($"[{BuildInfo.BuildId}] Removed excess top task. reason={reason}, target={cappedTarget}, activeTasks.Count={taskManager.VisibleCount}");
            }
        }

        private void TrySpawnOne(string reason)
        {
            if (isSpawning || !CanSpawn())
            {
                return;
            }

            isSpawning = true;
            SpawnOne(reason);
            isSpawning = false;
            Debug.Log($"[{BuildInfo.BuildId}] Spawned one task. reason={reason}, activeTasks.Count={taskManager.VisibleCount}");
        }

        private void SpawnOne(string reason)
        {
            if (!CanSpawn())
            {
                Debug.Log($"[{BuildInfo.BuildId}] Spawn skipped reason={reason}. activeTasks.Count={(taskManager != null ? taskManager.VisibleCount : -1)}, maxVisible={maxVisibleBeforeSpawn}");
                return;
            }

            TaskDefinition task = taskDatabase.PickRandom();
            if (task == null)
            {
                return;
            }

            taskManager.Spawn(task);
            float remaining = timerManager != null ? timerManager.RemainingSeconds : -1f;
            Debug.Log($"[{BuildInfo.BuildId}] Spawn task={task.taskId}, reason={reason}, remaining={remaining:F1}, activeTasks.Count={taskManager.VisibleCount}");
            audioManager?.PlayNotificationPop();
        }

        private bool CanSpawn()
        {
            return taskManager != null && (maxVisibleBeforeSpawn <= 0 || taskManager.VisibleCount < maxVisibleBeforeSpawn);
        }

        private int GetCurrentTargetTaskCount()
        {
            return GetTargetTaskCountByElapsedTime(GetElapsedSeconds());
        }

        private int GetTargetTaskCountByElapsedTime(float elapsedTime)
        {
            if (elapsedTime < 10f)
            {
                return 2;
            }

            if (elapsedTime < 30f)
            {
                return 3;
            }

            return 4;
        }

        private float GetElapsedSeconds()
        {
            if (timerManager == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, timerManager.GameDurationSeconds - timerManager.RemainingSeconds);
        }
    }
}
