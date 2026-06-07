using PushNotificationGod.Core;
using UnityEngine;

namespace PushNotificationGod.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private AudioClip notificationPopSe;
        [SerializeField] private AudioClip tapCorrectSe;
        [SerializeField] private AudioClip swipeCorrectSe;
        [SerializeField] private AudioClip missSe;
        [SerializeField] private AudioClip scorePopSe;
        [SerializeField] private AudioClip comboSe;
        [SerializeField] private AudioClip combo5Se;
        [SerializeField] private AudioClip combo10Se;
        [SerializeField] private AudioClip combo20Se;
        [SerializeField] private AudioClip combo30Se;
        [SerializeField] private AudioClip gameOverSe;
        [SerializeField] private AudioClip resultSe;
        [SerializeField] private AudioClip countdownTickSe;
        [SerializeField] private AudioClip countdownStartSe;
        [SerializeField] private AudioClip gameplayBgm;
        [SerializeField] private AudioClip countdownTickClipOverride;
        [SerializeField] private AudioClip countdownStartClipOverride;
        [SerializeField] private AudioClip gameplayBgmClipOverride;

        [SerializeField] private float notificationPopVolume = 0.6f;
        [SerializeField] private float correctVolume = 0.7f;
        [SerializeField] private float swipeVolume = 0.7f;
        [SerializeField] private float missVolume = 0.7f;
        [SerializeField] private float scorePopVolume = 0.5f;
        [SerializeField] private float comboVolume = 0.8f;
        [SerializeField] private float gameOverVolume = 0.8f;
        [SerializeField] private float resultVolume = 0.8f;
        [SerializeField] private float countdownTickVolume = 0.8f;
        [SerializeField] private float countdownStartVolume = 0.9f;
        [SerializeField] private float bgmVolume = 0.35f;
        [SerializeField] private float seVolume = 0.8f;
        [SerializeField] private float sameClipCooldownSeconds = 0.04f;

        private AudioClip lastClip;
        private float lastClipTime = -999f;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
                audioSource.loop = false;
            }

            if (bgmAudioSource == null)
            {
                bgmAudioSource = gameObject.AddComponent<AudioSource>();
            }

            LoadExplicitAudioClips();
            PreloadSeClips();
            LoadVolumes();
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.loop = true;
            bgmAudioSource.volume = bgmVolume;
            bgmAudioSource.spatialBlend = 0f;
            Debug.Log($"[{BuildInfo.BuildId}] [Audio] seVolume={seVolume:F2} bgmVolume={bgmVolume:F2}");
        }

        public void PlayNotificationPop() => Play(notificationPopSe, notificationPopVolume);
        public void PlayTapCorrect() => Play(tapCorrectSe, correctVolume);
        public void PlaySwipeCorrect() => Play(swipeCorrectSe, swipeVolume);
        public void PlayMiss() => Play(missSe, missVolume);
        public void PlayScorePop() => Play(scorePopSe, scorePopVolume);
        public void PlayCombo() => Play(comboSe, comboVolume);
        public void PlayCombo5() => Play(combo5Se, comboVolume);
        public void PlayCombo10() => Play(combo10Se, comboVolume);
        public void PlayCombo20() => Play(combo20Se, comboVolume);
        public void PlayCombo30()
        {
            if (combo30Se == null)
            {
                Debug.LogWarning("[AudioManager] combo30Se is not assigned. Skipping 30 combo SE.");
                return;
            }

            Play(combo30Se, comboVolume);
        }
        public void PlayGameOver() => Play(gameOverSe, gameOverVolume);
        public void PlayResult() => Play(resultSe, resultVolume);
        public void PlayCountdownTick()
        {
            AudioClip clip = CountdownTickClip;
            Debug.Log($"[{BuildInfo.BuildId}] [Audio] PlayCountdownTick clip={ClipName(clip)}");
            Play(clip, countdownTickVolume);
        }

        public void PlayCountdownStart()
        {
            AudioClip clip = CountdownStartClip;
            Debug.Log($"[{BuildInfo.BuildId}] [Audio] PlayCountdownStart clip={ClipName(clip)}");
            Play(clip, countdownStartVolume);
        }

        public void PlayGameplayBgm()
        {
            AudioClip clip = GameplayBgmClip;
            Debug.Log($"[{BuildInfo.BuildId}] [Audio] PlayGameplayBgm clip={ClipName(clip)} volume={bgmVolume:F2}");
            StopAllBgm();
            PlayBgm(clip);
        }

        public void StopGameplayBgm()
        {
            StopAllBgm();
        }

        public void StopAllBgm()
        {
            if (bgmAudioSource == null)
            {
                return;
            }

            bgmAudioSource.Stop();
        }

        public void SetBgmVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmAudioSource != null)
            {
                bgmAudioSource.volume = bgmVolume;
            }
        }

        public void SetSeVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
        }

        public void PlaySpawn() => PlayNotificationPop();
        public void PlayCorrect() => PlayTapCorrect();
        public void PlayMistake() => PlayMiss();

        public void LogGameSceneAudioStatus()
        {
            Debug.Log($"[{BuildInfo.BuildId}] [AudioCheck] countdownTickClip = {ClipName(CountdownTickClip)} raw={ClipName(countdownTickSe)} override={ClipName(countdownTickClipOverride)}");
            Debug.Log($"[{BuildInfo.BuildId}] [AudioCheck] countdownStartClip = {ClipName(CountdownStartClip)} raw={ClipName(countdownStartSe)} override={ClipName(countdownStartClipOverride)}");
            Debug.Log($"[{BuildInfo.BuildId}] [AudioCheck] gameplayBgmClip = {ClipName(GameplayBgmClip)} raw={ClipName(gameplayBgm)} override={ClipName(gameplayBgmClipOverride)}");
            Debug.Log($"[{BuildInfo.BuildId}] [AudioCheck] seVolume = {seVolume:F2}, seSource.volume = {(audioSource != null ? audioSource.volume.ToString("F2") : "NULL")}");
            Debug.Log($"[{BuildInfo.BuildId}] [AudioCheck] bgmVolume = {bgmVolume:F2}, bgmSource.volume = {(bgmAudioSource != null ? bgmAudioSource.volume.ToString("F2") : "NULL")}");

            if (CountdownTickClip != null && CountdownTickClip.name.Contains("start"))
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] [AudioCheck] countdownTickClip looks wrong: {CountdownTickClip.name}");
            }

            if (GameplayBgmClip == null)
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] [AudioCheck] gameplayBgmClip is NULL.");
            }
        }

        private AudioClip CountdownTickClip => countdownTickClipOverride != null ? countdownTickClipOverride : countdownTickSe;
        private AudioClip CountdownStartClip => countdownStartClipOverride != null ? countdownStartClipOverride : countdownStartSe;
        private AudioClip GameplayBgmClip => gameplayBgmClipOverride != null ? gameplayBgmClipOverride : gameplayBgm;

        private void LoadExplicitAudioClips()
        {
            notificationPopSe = LoadSeOrKeep("se_notification_pop", notificationPopSe);
            tapCorrectSe = LoadSeOrKeep("se_tap_correct", tapCorrectSe);
            swipeCorrectSe = LoadSeOrKeep("se_swipe_correct", swipeCorrectSe);
            missSe = LoadSeOrKeep("se_miss", missSe);
            scorePopSe = LoadSeOrKeep("se_score_pop", scorePopSe);
            comboSe = LoadSeOrKeep("se_combo", comboSe);
            combo5Se = LoadSeOrKeep("se_combo_5", combo5Se);
            combo10Se = LoadSeOrKeep("se_combo_10", combo10Se);
            combo20Se = LoadSeOrKeep("se_combo_20", combo20Se);
            combo30Se = LoadSeOrKeep("se_combo_30", combo30Se);
            gameOverSe = LoadSeOrKeep("se_game_over", gameOverSe);
            resultSe = LoadSeOrKeep("se_result", resultSe);

            AudioClip tick = Resources.Load<AudioClip>("Audio/SE/se_countdown_tick");
            AudioClip start = Resources.Load<AudioClip>("Audio/SE/se_countdown_start");
            AudioClip bgm = Resources.Load<AudioClip>("Audio/BGM/bgm_gameplay");

            if (tick != null)
            {
                countdownTickClipOverride = tick;
            }
            else
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] [AudioCheck] Resources tick clip missing: Audio/SE/se_countdown_tick");
            }

            if (start != null)
            {
                countdownStartClipOverride = start;
            }
            else
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] [AudioCheck] Resources start clip missing: Audio/SE/se_countdown_start");
            }

            if (bgm != null)
            {
                gameplayBgmClipOverride = bgm;
            }
            else
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] [AudioCheck] Resources gameplay BGM missing: Audio/BGM/bgm_gameplay");
            }
        }

        public System.Collections.IEnumerator WaitForCountdownAudioReady(float maxWaitSeconds)
        {
            AudioClip tick = CountdownTickClip;
            AudioClip start = CountdownStartClip;
            RequestAudioDataLoad(tick);
            RequestAudioDataLoad(start);

            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < maxWaitSeconds)
            {
                bool tickReady = tick == null || tick.loadState == AudioDataLoadState.Loaded;
                bool startReady = start == null || start.loadState == AudioDataLoadState.Loaded;
                if (tickReady && startReady)
                {
                    break;
                }

                yield return new WaitForSecondsRealtime(0.05f);
            }

            Debug.Log($"[{BuildInfo.BuildId}] [AudioPreload] countdown ready tick={LoadStateName(tick)} start={LoadStateName(start)} waited={(Time.realtimeSinceStartup - startTime):F2}s");
        }

        private void PreloadSeClips()
        {
            PreloadClip(notificationPopSe);
            PreloadClip(tapCorrectSe);
            PreloadClip(swipeCorrectSe);
            PreloadClip(missSe);
            PreloadClip(scorePopSe);
            PreloadClip(comboSe);
            PreloadClip(combo5Se);
            PreloadClip(combo10Se);
            PreloadClip(combo20Se);
            PreloadClip(combo30Se);
            PreloadClip(gameOverSe);
            PreloadClip(resultSe);
            PreloadClip(CountdownTickClip);
            PreloadClip(CountdownStartClip);
        }

        private void PreloadClip(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            bool requested = RequestAudioDataLoad(clip);
            Debug.Log($"[{BuildInfo.BuildId}] [AudioPreload] clip={clip.name} state={clip.loadState} requested={requested}");
        }

        private static bool RequestAudioDataLoad(AudioClip clip)
        {
            if (clip == null || clip.loadState == AudioDataLoadState.Loaded || clip.loadState == AudioDataLoadState.Loading)
            {
                return false;
            }

            return clip.LoadAudioData();
        }

        private AudioClip LoadSeOrKeep(string resourceName, AudioClip current)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/SE/{resourceName}");
            if (clip != null)
            {
                return clip;
            }

            Debug.LogWarning($"[{BuildInfo.BuildId}] [AudioCheck] Resources SE missing: Audio/SE/{resourceName}. Keeping scene reference={ClipName(current)}");
            return current;
        }

        private void PlayBgm(AudioClip clip)
        {
            if (bgmAudioSource == null || clip == null)
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] BGM play skipped. sourceNull={bgmAudioSource == null}, clipNull={clip == null}");
                return;
            }

            if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying)
            {
                bgmAudioSource.volume = bgmVolume;
                Debug.Log($"[{BuildInfo.BuildId}] BGM already playing: {clip.name}");
                return;
            }

            bgmAudioSource.Stop();
            bgmAudioSource.clip = clip;
            bgmAudioSource.loop = true;
            bgmAudioSource.volume = bgmVolume;
            bgmAudioSource.Play();
            Debug.Log($"[{BuildInfo.BuildId}] [Audio] Gameplay BGM started: {clip.name}, volume={bgmVolume:F2}, sourceVolume={bgmAudioSource.volume:F2}, isPlaying={bgmAudioSource.isPlaying}");
        }

        private void Play(AudioClip clip, float volume)
        {
            if (audioSource == null || clip == null)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (clip == lastClip && now - lastClipTime < sameClipCooldownSeconds)
            {
                return;
            }

            if (clip.loadState != AudioDataLoadState.Loaded)
            {
                RequestAudioDataLoad(clip);
                Debug.Log($"[{BuildInfo.BuildId}] [Audio] Delayed SE until loaded. clip={clip.name} state={clip.loadState}");
                StartCoroutine(PlayWhenLoaded(clip, volume, 1f));
                return;
            }

            lastClip = clip;
            lastClipTime = now;
            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume) * Mathf.Clamp01(seVolume));
        }

        private System.Collections.IEnumerator PlayWhenLoaded(AudioClip clip, float volume, float maxWaitSeconds)
        {
            float startTime = Time.realtimeSinceStartup;
            while (clip != null && clip.loadState != AudioDataLoadState.Loaded && Time.realtimeSinceStartup - startTime < maxWaitSeconds)
            {
                yield return new WaitForSecondsRealtime(0.05f);
            }

            if (audioSource == null || clip == null || clip.loadState != AudioDataLoadState.Loaded)
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] [Audio] Delayed SE skipped. clip={ClipName(clip)} state={LoadStateName(clip)}");
                yield break;
            }

            float now = Time.unscaledTime;
            if (clip == lastClip && now - lastClipTime < sameClipCooldownSeconds)
            {
                yield break;
            }

            lastClip = clip;
            lastClipTime = now;
            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume) * Mathf.Clamp01(seVolume));
            Debug.Log($"[{BuildInfo.BuildId}] [Audio] Delayed SE played. clip={clip.name} waited={(Time.realtimeSinceStartup - startTime):F2}s");
        }

        private void LoadVolumes()
        {
            bgmVolume = LoadVolume("bgm_volume", 0.35f);
            seVolume = LoadVolume("se_volume", 0.8f);
        }

        private static float LoadVolume(string key, float fallback)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return fallback;
            }

            float value = PlayerPrefs.GetFloat(key, fallback);
            if (value <= 0.001f)
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] [Audio] {key} was {value:F2}. Reset to {fallback:F2} for MVP playback.");
                PlayerPrefs.SetFloat(key, fallback);
                return fallback;
            }

            return Mathf.Clamp01(value);
        }

        private static string ClipName(AudioClip clip)
        {
            return clip != null ? clip.name : "NULL";
        }

        private static string LoadStateName(AudioClip clip)
        {
            return clip != null ? clip.loadState.ToString() : "NULL";
        }
    }
}
