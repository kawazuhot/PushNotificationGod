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
        [SerializeField] private int minimumVisibleTaskCount = 2;
        [SerializeField] private int refillThreshold = 1;
        [SerializeField] private int maxVisibleBeforeSpawn = 3;

        private bool running;
        private bool isSpawning;
        private int lastLoggedVisibleCount = -1;

        public void Begin()
        {
            running = true;
            isSpawning = false;
            lastLoggedVisibleCount = -1;
            Debug.Log($"[{BuildInfo.BuildId}] TaskSpawner.Begin initialTaskCount={initialTaskCount}, minimumVisible={minimumVisibleTaskCount}, refillThreshold={refillThreshold}, maxVisible={maxVisibleBeforeSpawn}");

            for (int i = 0; i < initialTaskCount; i++)
            {
                SpawnOne("initial");
            }

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

            if (visibleCount <= refillThreshold)
            {
                TryRefillOne();
            }
        }

        private void TryRefillOne()
        {
            if (!running)
            {
                Debug.Log($"[{BuildInfo.BuildId}] Refill skipped because game is not running.");
                return;
            }

            if (isSpawning || !CanSpawn())
            {
                return;
            }

            isSpawning = true;
            SpawnOne("refill");
            isSpawning = false;
            Debug.Log($"[{BuildInfo.BuildId}] Refill spawned exactly one task. activeTasks.Count={taskManager.VisibleCount}");
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
    }
}
