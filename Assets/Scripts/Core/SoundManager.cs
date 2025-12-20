using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip menuBGM;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip choiceSelectSound;

    [Header("Gameplay Sounds")]
    [SerializeField] private AudioClip recordingStartSound;

    [Header("Tension Ambient - MAIN AUDIO DURING GAMEPLAY")]
    [SerializeField] private AudioClip tensionLowAmbient;
    [SerializeField] private AudioClip tensionMidAmbient;
    [SerializeField] private AudioClip tensionHighAmbient;

    [Header("Ending Sounds")]
    [SerializeField] private AudioClip goodEndingSound;
    [SerializeField] private AudioClip badEndingSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1.0f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float ambientVolume = 0.9f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float crossfadeDuration = 2f;

    // ✅ FIXED: Threshold phải match với GlobalStateManager
    [Header("Tension Thresholds")]
    [Range(0f, 1f)] public float mediumTensionThreshold = 0.4f; // 40%
    [Range(0f, 1f)] public float highTensionThreshold = 0.7f;   // 70%

    private TensionLevel currentTensionLevel = TensionLevel.Low;
    private Coroutine bgmFadeCoroutine;
    private Coroutine ambientFadeCoroutine;
    private bool ambientHasStarted = false;

    public enum TensionLevel
    {
        Low,    // < 40%
        Mid,    // 40% - 70%
        High    // >= 70%
    }

    #region Initialization

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }

        ApplyVolumeSettings();
    }

    private void ApplyVolumeSettings()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume * masterVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
        if (ambientSource != null) ambientSource.volume = ambientVolume * masterVolume;
    }

    #endregion

    #region BGM Control

    public void PlayMenuBGM()
    {
        PlayBGM(menuBGM, true);
    }

    /// <summary>
    ///  ONLY starts tension ambient (no BGM)
    /// Menu BGM should be stopped before gameplay starts
    /// </summary>
    public void PlayGameplayBGM()
    {
        // Stop menu BGM if playing
        if (bgmSource.isPlaying)
        {
            Debug.Log("[SoundManager] Stopping Menu BGM");
            StopBGM(true);
        }
        
        // ✅ Force start ambient at Low level
        ambientHasStarted = false;
        currentTensionLevel = TensionLevel.Low;
        
        Debug.Log("[SoundManager] Starting tension ambient at Low level");
        SwitchTensionAmbient(TensionLevel.Low);
        ambientHasStarted = true;
    }

    private void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] BGM clip is null!");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        if (bgmFadeCoroutine != null)
            StopCoroutine(bgmFadeCoroutine);

        bgmFadeCoroutine = StartCoroutine(CrossfadeBGM(clip, loop));
    }

    private IEnumerator CrossfadeBGM(AudioClip newClip, bool loop)
    {
        float targetVolume = bgmVolume * masterVolume;

        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }
            bgmSource.Stop();
        }

        bgmSource.clip = newClip;
        bgmSource.loop = loop;
        bgmSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    public void StopBGM(bool fade = true)
    {
        if (bgmFadeCoroutine != null)
            StopCoroutine(bgmFadeCoroutine);

        if (fade)
            bgmFadeCoroutine = StartCoroutine(FadeOutBGM());
        else
            bgmSource.Stop();
    }

    private IEnumerator FadeOutBGM()
    {
        float startVolume = bgmSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = bgmVolume * masterVolume;
    }

    #endregion

    #region SFX Control

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void PlayButtonHover()
    {
        PlaySFX(buttonHoverSound, 0.6f);
    }

    public void PlayChoiceSelect()
    {
        PlaySFX(choiceSelectSound);
    }

    public void PlayRecordingStart()
    {
        PlaySFX(recordingStartSound);
    }

    public void PlayGoodEnding()
    {
        PlaySFX(goodEndingSound);
    }

    public void PlayBadEnding()
    {
        PlaySFX(badEndingSound);
    }

    public void PlayViolenceSound()
    {
        PlaySFX(badEndingSound, 1f);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
    }

    #endregion

    #region Tension Ambient System

    /// <summary>
    /// Sử dụng threshold đúng để detect Mid/High level
    /// </summary>
    public void UpdateTensionAmbient(float tensionValue, float maxTension)
    {
        float tensionRatio = tensionValue / maxTension;
        TensionLevel newLevel;

        // Dùng threshold từ SoundManager thay vì GlobalStateManager
        if (tensionRatio < mediumTensionThreshold)
        {
            newLevel = TensionLevel.Low;
        }
        else if (tensionRatio < highTensionThreshold)
        {
            newLevel = TensionLevel.Mid;
        }
        else
        {
            newLevel = TensionLevel.High;
        }

        Debug.Log($"[SoundManager] UpdateTensionAmbient: {tensionValue:F1}/{maxTension} (Ratio: {tensionRatio:F2}) → Level: {newLevel}");
        
        if (ambientSource != null && ambientSource.isPlaying)
        {
            Debug.Log($"[SoundManager] Currently playing: {ambientSource.clip?.name ?? "NULL"} at volume {ambientSource.volume:F2}");
        }
        else
        {
            Debug.LogWarning("[SoundManager] Ambient source is NOT playing!");
        }

        // ✅ FIXED: Also start ambient if not started yet
        if (!ambientHasStarted || newLevel != currentTensionLevel)
        {
            currentTensionLevel = newLevel;
            Debug.Log($"[SoundManager] Switching to: {newLevel}");
            SwitchTensionAmbient(newLevel);
            ambientHasStarted = true;
        }
        else
        {
            Debug.Log($"[SoundManager] Already at {newLevel}, no switch needed");
        }
    }

    private void SwitchTensionAmbient(TensionLevel level)
    {
        AudioClip targetClip = null;

        switch (level)
        {
            case TensionLevel.Low:
                targetClip = tensionLowAmbient;
                break;
            case TensionLevel.Mid:
                targetClip = tensionMidAmbient;
                break;
            case TensionLevel.High:
                targetClip = tensionHighAmbient;
                break;
        }

        if (targetClip == null)
        {
            Debug.LogWarning($"[SoundManager] ⚠️ Missing ambient clip for {level}!");
            return;
        }

        if (ambientFadeCoroutine != null)
            StopCoroutine(ambientFadeCoroutine);

        ambientFadeCoroutine = StartCoroutine(CrossfadeAmbient(targetClip));
    }

    private IEnumerator CrossfadeAmbient(AudioClip newClip)
    {
        float targetVolume = ambientVolume * masterVolume;

        if (ambientSource.isPlaying)
        {
            float startVolume = ambientSource.volume;
            for (float t = 0; t < crossfadeDuration; t += Time.deltaTime)
            {
                ambientSource.volume = Mathf.Lerp(startVolume, 0f, t / crossfadeDuration);
                yield return null;
            }
            ambientSource.Stop();
        }

        ambientSource.clip = newClip;
        ambientSource.loop = true;
        ambientSource.Play();

        Debug.Log($"[SoundManager] ▶️ Playing ambient: {newClip.name}");

        for (float t = 0; t < crossfadeDuration; t += Time.deltaTime)
        {
            ambientSource.volume = Mathf.Lerp(0f, targetVolume, t / crossfadeDuration);
            yield return null;
        }

        ambientSource.volume = targetVolume;
        Debug.Log($"[SoundManager]  Ambient fully playing at volume: {targetVolume:F2}");
    }

    public void StopTensionAmbient(bool fade = true)
    {
        if (ambientFadeCoroutine != null)
            StopCoroutine(ambientFadeCoroutine);

        if (fade)
        {
            ambientFadeCoroutine = StartCoroutine(FadeOutAmbient());
        }
        else
        {
            ambientSource.Stop();
        }
        
        ambientHasStarted = false;
    }

    private IEnumerator FadeOutAmbient()
    {
        float startVolume = ambientSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        ambientSource.Stop();
        ambientSource.volume = ambientVolume * masterVolume;
    }

    #endregion

    #region Volume Control

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume * masterVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume * masterVolume;
    }

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        ambientSource.volume = ambientVolume * masterVolume;
    }

    #endregion

    #region Utility

    public void PauseAll()
    {
        bgmSource.Pause();
        ambientSource.Pause();
    }

    public void ResumeAll()
    {
        bgmSource.UnPause();
        ambientSource.UnPause();
    }

    public void StopAll()
    {
        bgmSource.Stop();
        sfxSource.Stop();
        ambientSource.Stop();
    }

    #endregion
}