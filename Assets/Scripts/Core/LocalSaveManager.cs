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

        public static int BestScore => SafeGetInt(BestScoreKey, 0);
        public static int LastScore => SafeGetInt(LastScoreKey, 0);
        public static int LastSuccessCount => SafeGetInt(LastSuccessCountKey, 0);
        public static int LastMissCount => SafeGetInt(LastMissCountKey, 0);
        public static int LastMaxCombo => SafeGetInt(LastMaxComboKey, 0);
        public static string LastRankTitle => SafeGetString(LastRankTitleKey, string.Empty);
        public static bool LastWasNewRecord => SafeGetInt(LastWasNewRecordKey, 0) == 1;
        public static string PlayerName => SafeGetString(PlayerNameKey, string.Empty);
        public static float BgmVolume => SafeGetFloat(BgmVolumeKey, DefaultBgmVolume);
        public static float SeVolume => SafeGetFloat(SeVolumeKey, DefaultSeVolume);

        public static void SaveBestScore(int score)
        {
            if (score <= BestScore)
            {
                return;
            }

            SafeSetInt(BestScoreKey, score);
            SafeSave();
        }

        public static void SaveLastResult(int score, int maxCombo, bool wasNewRecord)
        {
            SaveLastResult(score, 0, 0, maxCombo, GameResultData.GetRankTitle(score), wasNewRecord);
        }

        public static void SaveLastResult(int score, int successCount, int missCount, int maxCombo, string rankTitle, bool wasNewRecord)
        {
            SafeSetInt(LastScoreKey, Mathf.Max(0, score));
            SafeSetInt(LastSuccessCountKey, Mathf.Max(0, successCount));
            SafeSetInt(LastMissCountKey, Mathf.Max(0, missCount));
            SafeSetInt(LastMaxComboKey, Mathf.Max(0, maxCombo));
            SafeSetString(LastRankTitleKey, string.IsNullOrWhiteSpace(rankTitle) ? GameResultData.GetRankTitle(score) : rankTitle);
            SafeSetInt(LastWasNewRecordKey, wasNewRecord ? 1 : 0);
            SafeSave();
        }

        public static void SavePlayerName(string playerName)
        {
            SafeSetString(PlayerNameKey, string.IsNullOrWhiteSpace(playerName) ? "なまえ未設定" : playerName.Trim());
            SafeSave();
        }

        public static void SaveBgmVolume(float volume)
        {
            SafeSetFloat(BgmVolumeKey, Mathf.Clamp01(volume));
            SafeSave();
        }

        public static void SaveSeVolume(float volume)
        {
            SafeSetFloat(SeVolumeKey, Mathf.Clamp01(volume));
            SafeSave();
        }

        private static int SafeGetInt(string key, int defaultValue)
        {
            try
            {
                return PlayerPrefs.GetInt(key, defaultValue);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PlayerPrefs.GetInt failed for {key}. Using default. {ex.GetType().Name}: {ex.Message}");
                return defaultValue;
            }
        }

        private static float SafeGetFloat(string key, float defaultValue)
        {
            try
            {
                return PlayerPrefs.GetFloat(key, defaultValue);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PlayerPrefs.GetFloat failed for {key}. Using default. {ex.GetType().Name}: {ex.Message}");
                return defaultValue;
            }
        }

        private static string SafeGetString(string key, string defaultValue)
        {
            try
            {
                return PlayerPrefs.GetString(key, defaultValue);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PlayerPrefs.GetString failed for {key}. Using default. {ex.GetType().Name}: {ex.Message}");
                return defaultValue;
            }
        }

        private static void SafeSetInt(string key, int value)
        {
            try
            {
                PlayerPrefs.SetInt(key, value);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PlayerPrefs.SetInt failed for {key}. {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static void SafeSetFloat(string key, float value)
        {
            try
            {
                PlayerPrefs.SetFloat(key, value);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PlayerPrefs.SetFloat failed for {key}. {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static void SafeSetString(string key, string value)
        {
            try
            {
                PlayerPrefs.SetString(key, value);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PlayerPrefs.SetString failed for {key}. {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static void SafeSave()
        {
            try
            {
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PlayerPrefs.Save failed. Continuing without persistent save. {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
