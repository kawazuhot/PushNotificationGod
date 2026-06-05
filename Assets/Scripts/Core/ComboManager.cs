using UnityEngine;

namespace PushNotificationGod.Core
{
    public class ComboManager : MonoBehaviour
    {
        private int currentCombo;
        private int maxCombo;

        public int CurrentCombo => currentCombo;
        public int MaxCombo => maxCombo;

        public void ResetCombo()
        {
            currentCombo = 0;
            maxCombo = 0;
        }

        public float RegisterCorrect()
        {
            currentCombo++;
            maxCombo = Mathf.Max(maxCombo, currentCombo);
            return GetMultiplier(currentCombo);
        }

        public void RegisterMistake()
        {
            currentCombo = 0;
        }

        public float GetMultiplier(int combo)
        {
            if (combo >= 20)
            {
                return 2.0f;
            }

            if (combo >= 10)
            {
                return 1.5f;
            }

            if (combo >= 5)
            {
                return 1.2f;
            }

            return 1.0f;
        }
    }
}
