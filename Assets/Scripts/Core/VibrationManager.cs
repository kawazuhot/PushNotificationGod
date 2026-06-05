using System.Runtime.InteropServices;
using UnityEngine;

namespace PushNotificationGod.Core
{
    public static class VibrationManager
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void PngVibrate(int milliseconds);
#endif

        public static void PlayLightVibration()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PngVibrate(45);
#elif UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
    }
}
