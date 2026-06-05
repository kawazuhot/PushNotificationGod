using UnityEngine;

namespace PushNotificationGod.Core
{
    public static class LocalSaveManager
    {
        private const string BestScoreKey = "PushNotificationGod.BestScore";
        private const string PlayerNameKey = "player_name";
        private const string BgmVolumeKey = "bgm_volume";
        private const string SeVolumeKey = "se_volume";
        private const float DefaultBgmVolume = 0.35f;
        private const float DefaultSeVolume = 0.8f;

        public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);
        public static string PlayerName => PlayerPrefs.GetString(PlayerNameKey, string.Empty);
        public static float BgmVolume => PlayerPrefs.GetFloat(BgmVolumeKey, DefaultBgmVolume);
        public static float SeVolume => PlayerPrefs.GetFloat(SeVolumeKey, DefaultSeVolume);

        public static void SaveBestScore(int score)
        {
            if (score <= BestScore)
            {
                return;
            }

            PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
        }

        public static void SavePlayerName(string playerName)
        {
            PlayerPrefs.SetString(PlayerNameKey, string.IsNullOrWhiteSpace(playerName) ? "なまえ未設定" : playerName.Trim());
            PlayerPrefs.Save();
        }

        public static void SaveBgmVolume(float volume)
        {
            PlayerPrefs.SetFloat(BgmVolumeKey, Mathf.Clamp01(volume));
            PlayerPrefs.Save();
        }

        public static void SaveSeVolume(float volume)
        {
            PlayerPrefs.SetFloat(SeVolumeKey, Mathf.Clamp01(volume));
            PlayerPrefs.Save();
        }
    }
}
