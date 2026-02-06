using System.Collections.Generic;
using UnityEngine;

namespace VocabCardGame.Core
{
    /// <summary>
    /// 音訊管理器
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource voiceSource;

        [Header("Volume Settings")]
        [Range(0, 1)] public float masterVolume = 1f;
        [Range(0, 1)] public float musicVolume = 0.7f;
        [Range(0, 1)] public float sfxVolume = 1f;
        [Range(0, 1)] public float voiceVolume = 1f;

        [Header("Audio Clips")]
        public AudioClip buttonClick;
        public AudioClip cardPlay;
        public AudioClip cardDraw;
        public AudioClip attackHit;
        public AudioClip enemyHit;
        public AudioClip heal;
        public AudioClip block;
        public AudioClip quizCorrect;
        public AudioClip quizWrong;
        public AudioClip levelUp;
        public AudioClip victory;
        public AudioClip defeat;
        public AudioClip comboTrigger;

        // 快取的單字音訊
        private Dictionary<string, AudioClip> wordAudioCache = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.volume = sfxVolume * masterVolume;
                sfxSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// 播放按鈕音效
        /// </summary>
        public void PlayButtonClick() => PlaySFX(buttonClick);

        /// <summary>
        /// 播放卡牌相關音效
        /// </summary>
        public void PlayCardPlay() => PlaySFX(cardPlay);
        public void PlayCardDraw() => PlaySFX(cardDraw);

        /// <summary>
        /// 播放戰鬥音效
        /// </summary>
        public void PlayAttackHit() => PlaySFX(attackHit);
        public void PlayEnemyHit() => PlaySFX(enemyHit);
        public void PlayHeal() => PlaySFX(heal);
        public void PlayBlock() => PlaySFX(block);

        /// <summary>
        /// 播放答題音效
        /// </summary>
        public void PlayQuizCorrect() => PlaySFX(quizCorrect);
        public void PlayQuizWrong() => PlaySFX(quizWrong);

        /// <summary>
        /// 播放其他音效
        /// </summary>
        public void PlayLevelUp() => PlaySFX(levelUp);
        public void PlayVictory() => PlaySFX(victory);
        public void PlayDefeat() => PlaySFX(defeat);
        public void PlayComboTrigger() => PlaySFX(comboTrigger);

        /// <summary>
        /// 播放單字發音
        /// </summary>
        public void PlayWordAudio(string audioPath)
        {
            if (string.IsNullOrEmpty(audioPath)) return;

            AudioClip clip;
            if (wordAudioCache.TryGetValue(audioPath, out clip))
            {
                PlayVoice(clip);
            }
            else
            {
                // 動態載入
                clip = Resources.Load<AudioClip>(audioPath);
                if (clip != null)
                {
                    wordAudioCache[audioPath] = clip;
                    PlayVoice(clip);
                }
                else
                {
                    Debug.LogWarning($"Word audio not found: {audioPath}");
                }
            }
        }

        /// <summary>
        /// 播放語音
        /// </summary>
        private void PlayVoice(AudioClip clip)
        {
            if (clip != null && voiceSource != null)
            {
                voiceSource.volume = voiceVolume * masterVolume;
                voiceSource.clip = clip;
                voiceSource.Play();
            }
        }

        /// <summary>
        /// 播放背景音樂
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip != null && musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.Play();
            }
        }

        /// <summary>
        /// 停止背景音樂
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// 設定音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
        }

        private void UpdateAllVolumes()
        {
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }
        }

        /// <summary>
        /// 預載入常用音訊
        /// </summary>
        public void PreloadWordAudio(List<string> audioPaths)
        {
            foreach (var path in audioPaths)
            {
                if (!wordAudioCache.ContainsKey(path))
                {
                    var clip = Resources.Load<AudioClip>(path);
                    if (clip != null)
                    {
                        wordAudioCache[path] = clip;
                    }
                }
            }
        }

        /// <summary>
        /// 清除快取
        /// </summary>
        public void ClearAudioCache()
        {
            wordAudioCache.Clear();
        }
    }
}
