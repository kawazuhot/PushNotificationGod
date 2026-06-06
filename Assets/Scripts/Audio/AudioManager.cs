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
        [SerializeField] private AudioClip gameOverSe;
        [SerializeField] private AudioClip resultSe;
        [SerializeField] private AudioClip countdownTickSe;
        [SerializeField] private AudioClip countdownStartSe;
        [SerializeField] private AudioClip gameplayBgm;

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

            if (bgmAudioSource == null)
            {
                bgmAudioSource = gameObject.AddComponent<AudioSource>();
            }

            bgmVolume = 0.35f;
            seVolume = 0.8f;
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.loop = true;
            bgmAudioSource.volume = bgmVolume;
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
        public void PlayGameOver() => Play(gameOverSe, gameOverVolume);
        public void PlayResult() => Play(resultSe, resultVolume);
        public void PlayCountdownTick() => Play(countdownTickSe, countdownTickVolume);
        public void PlayCountdownStart() => Play(countdownStartSe, countdownStartVolume);

        public void PlayGameplayBgm()
        {
            PlayBgm(gameplayBgm);
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

        private void PlayBgm(AudioClip clip)
        {
            if (bgmAudioSource == null || clip == null)
            {
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
            Debug.Log($"[{BuildInfo.BuildId}] Title/Game BGM Started: {clip.name}, volume={bgmVolume}");
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

            lastClip = clip;
            lastClipTime = now;
            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume) * Mathf.Clamp01(seVolume));
        }
    }
}
