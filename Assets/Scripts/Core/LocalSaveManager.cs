using UnityEngine;

namespace PushNotificationGod.Core
{
    public static class LocalSaveManager
    {
        private const string PlayerNameKey = "player_name";
        private const float DefaultBgmVolume = 0.175f;
        private const float DefaultSeVolume = 0.8f;

        public static string PlayerName => SafeGetString(PlayerNameKey, string.Empty);
        public static float BgmVolume => DefaultBgmVolume;
        public static float SeVolume => DefaultSeVolume;

        public static void SavePlayerName(string playerName)
        {
            SafeSetString(PlayerNameKey, string.IsNullOrWhiteSpace(playerName) ? "なまえ未設定" : playerName.Trim());
            SafeSave();
        }

        public static void SaveBgmVolume(float volume)
        {
            // Disabled for MVP WebGL stability.
        }

        public static void SaveSeVolume(float volume)
        {
            // Disabled for MVP WebGL stability.
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
