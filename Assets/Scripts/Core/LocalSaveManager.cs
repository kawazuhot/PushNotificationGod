using UnityEngine;

namespace PushNotificationGod.Core
{
    public static class LocalSaveManager
    {
        private const string BestScoreKey = "PushNotificationGod.BestScore";
        private const string LastScoreKey = "PushNotificationGod.LastScore";
        private const string LastSuccessCountKey = "PushNotificationGod.LastSuccessCount";
        private const string LastMissCountKey = "PushNotificationGod.LastMissCount";
        private const string LastMaxComboKey = "PushNotificationGod.LastMaxCombo";
        private const string LastRankTitleKey = "PushNotificationGod.LastRankTitle";
        private const string LastWasNewRecordKey = "PushNotificationGod.LastWasNewRecord";
        private const string PlayerNameKey = "player_name";
        private const string BgmVolumeKey = "bgm_volume";
        private const string SeVolumeKey = "se_volume";
        private const float DefaultBgmVolume = 0.35f;
        private const float DefaultSeVolume = 0.8f;

        public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);
        public static int LastScore => PlayerPrefs.GetInt(LastScoreKey, 0);
        public static int LastSuccessCount => PlayerPrefs.GetInt(LastSuccessCountKey, 0);
        public static int LastMissCount => PlayerPrefs.GetInt(LastMissCountKey, 0);
        public static int LastMaxCombo => PlayerPrefs.GetInt(LastMaxComboKey, 0);
        public static string LastRankTitle => PlayerPrefs.GetString(LastRankTitleKey, string.Empty);
        public static bool LastWasNewRecord => PlayerPrefs.GetInt(LastWasNewRecordKey, 0) == 1;
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

        public static void SaveLastResult(int score, int maxCombo, bool wasNewRecord)
        {
            SaveLastResult(score, 0, 0, maxCombo, GameResultData.GetRankTitle(score), wasNewRecord);
        }

        public static void SaveLastResult(int score, int successCount, int missCount, int maxCombo, string rankTitle, bool wasNewRecord)
        {
            PlayerPrefs.SetInt(LastScoreKey, Mathf.Max(0, score));
            PlayerPrefs.SetInt(LastSuccessCountKey, Mathf.Max(0, successCount));
            PlayerPrefs.SetInt(LastMissCountKey, Mathf.Max(0, missCount));
            PlayerPrefs.SetInt(LastMaxComboKey, Mathf.Max(0, maxCombo));
            PlayerPrefs.SetString(LastRankTitleKey, string.IsNullOrWhiteSpace(rankTitle) ? GameResultData.GetRankTitle(score) : rankTitle);
            PlayerPrefs.SetInt(LastWasNewRecordKey, wasNewRecord ? 1 : 0);
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
