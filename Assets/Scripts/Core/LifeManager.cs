using System;
using UnityEngine;

namespace PushNotificationGod.Core
{
    public class LifeManager : MonoBehaviour
    {
        [SerializeField] private int initialLife = 3;
        [SerializeField] private int lifePenaltyOnMistake = 1;

        private int currentLife;

        public int CurrentLife => currentLife;
        public int InitialLife => initialLife;
        public event Action OnLifeDepleted;

        public void ResetLife()
        {
            currentLife = initialLife;
        }

        public void ApplyMistakePenalty()
        {
            currentLife = Mathf.Max(0, currentLife - lifePenaltyOnMistake);
            if (currentLife <= 0)
            {
                OnLifeDepleted?.Invoke();
            }
        }
    }
}
