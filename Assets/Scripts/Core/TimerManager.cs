using System;
using UnityEngine;

namespace PushNotificationGod.Core
{
    public class TimerManager : MonoBehaviour
    {
        [SerializeField] private float gameDurationSeconds = 60f;

        private float remainingSeconds;
        private bool running;

        public float GameDurationSeconds => gameDurationSeconds;
        public float RemainingSeconds => remainingSeconds;
        public float Progress01 => gameDurationSeconds <= 0f ? 1f : 1f - Mathf.Clamp01(remainingSeconds / gameDurationSeconds);
        public event Action OnTimeUp;

        public void PrepareTimer()
        {
            remainingSeconds = gameDurationSeconds;
            running = false;
        }

        public void StartTimer()
        {
            remainingSeconds = gameDurationSeconds;
            running = true;
        }

        public void StopTimer()
        {
            running = false;
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }

            remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
            if (remainingSeconds <= 0f)
            {
                running = false;
                OnTimeUp?.Invoke();
            }
        }
    }
}
