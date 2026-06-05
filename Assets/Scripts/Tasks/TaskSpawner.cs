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

        public void Begin()
        {
            running = true;
            spawnTimer = CurrentInterval();

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
            audioManager?.PlayNotificationPop();
        }

        private bool CanSpawn()
        {
            return taskManager != null && (maxVisibleBeforeSpawn <= 0 || taskManager.VisibleCount < maxVisibleBeforeSpawn);
        }
    }
}
