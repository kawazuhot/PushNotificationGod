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
        [SerializeField] private int maxVisibleBeforeSpawn = 5;
        [SerializeField] private float spawnIntervalSeconds = 2.0f;

        private float spawnTimer;
        private bool running;
        private bool loggedThirtySecondsOrLess;

        public void Begin()
        {
            running = true;
            spawnTimer = CurrentInterval();
            loggedThirtySecondsOrLess = false;
            Debug.Log($"[{BuildInfo.BuildId}] TaskSpawner.Begin interval={CurrentInterval()}, maxVisible={maxVisibleBeforeSpawn}, initial={initialTaskCount}");

            for (int i = 0; i < initialTaskCount; i++)
            {
                SpawnOne();
            }
        }

        public void Stop()
        {
            running = false;
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }

            if (!loggedThirtySecondsOrLess && timerManager != null && timerManager.RemainingSeconds <= 30f)
            {
                loggedThirtySecondsOrLess = true;
                Debug.Log($"[{BuildInfo.BuildId}] Remaining time <= 30 reached. interval still fixed={CurrentInterval()}, visible={taskManager.VisibleCount}, remaining={timerManager.RemainingSeconds:F1}");
            }

            if (taskManager.VisibleCount <= refillThreshold)
            {
                while (taskManager.VisibleCount < minimumVisibleTaskCount && CanSpawn())
                {
                    SpawnOne();
                }
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                if (CanSpawn())
                {
                    SpawnOne();
                }

                spawnTimer = CurrentInterval();
            }
        }

        private float CurrentInterval()
        {
            return Mathf.Max(0.5f, spawnIntervalSeconds);
        }

        private void SpawnOne()
        {
            if (!CanSpawn())
            {
                return;
            }

            TaskDefinition task = taskDatabase.PickRandom();
            if (task == null)
            {
                return;
            }

            taskManager.Spawn(task);
            float remaining = timerManager != null ? timerManager.RemainingSeconds : -1f;
            Debug.Log($"[{BuildInfo.BuildId}] Spawn task={task.taskId}, remaining={remaining:F1}, visible={taskManager.VisibleCount}, interval={CurrentInterval()}");
            audioManager?.PlayNotificationPop();
        }

        private bool CanSpawn()
        {
            return taskManager != null && (maxVisibleBeforeSpawn <= 0 || taskManager.VisibleCount < maxVisibleBeforeSpawn);
        }
    }
}
