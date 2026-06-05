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
        [SerializeField] private float spawnInterval0To10Seconds = 2.0f;
        [SerializeField] private float spawnInterval10To20Seconds = 1.5f;
        [SerializeField] private float spawnInterval20To30Seconds = 1.3f;
        [SerializeField] private float spawnIntervalAfter30Seconds = 1.2f;

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
                while (taskManager.VisibleCount < minimumVisibleTaskCount)
                {
                    SpawnOne();
                }
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnOne();
                spawnTimer = CurrentInterval();
            }
        }

        private float CurrentInterval()
        {
            float elapsedSeconds = 0f;
            if (timerManager != null)
            {
                elapsedSeconds = Mathf.Max(0f, timerManager.GameDurationSeconds - timerManager.RemainingSeconds);
            }

            if (elapsedSeconds < 10f)
            {
                return spawnInterval0To10Seconds;
            }

            if (elapsedSeconds < 20f)
            {
                return spawnInterval10To20Seconds;
            }

            if (elapsedSeconds < 30f)
            {
                return spawnInterval20To30Seconds;
            }

            return spawnIntervalAfter30Seconds;
        }

        private void SpawnOne()
        {
            TaskDefinition task = taskDatabase.PickRandom();
            if (task == null)
            {
                return;
            }

            taskManager.Spawn(task);
            audioManager?.PlayNotificationPop();
        }
    }
}
