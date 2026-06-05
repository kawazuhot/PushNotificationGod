using UnityEngine;

namespace PushNotificationGod.Core
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private int score;

        public int Score => score;

        public void ResetScore()
        {
            score = 0;
        }

        public int AddScore(int baseScore, float multiplier)
        {
            int gained = Mathf.RoundToInt(baseScore * multiplier);
            score += gained;
            return gained;
        }
    }
}
