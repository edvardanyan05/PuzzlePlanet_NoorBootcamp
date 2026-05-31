using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip winMusic;
    public AudioClip loseMusic;

    [Header("Sound Effects")]
    public AudioClip buttonClickSFX;
    public AudioClip swapSFX;
    public AudioClip pieceLockedSFX;
    public AudioClip chainBreakSFX;

    private bool musicEnabled;
    private bool sfxEnabled;
    private bool vibrationEnabled;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    // ---- MUSIC ----

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null) return;
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = musicEnabled ? 1f : 0f;
        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void PlayWinMusic()
    {
        if (winMusic == null) return;
        musicSource.Stop();
        musicSource.clip = winMusic;
        musicSource.loop = false;
        if (musicEnabled)
            musicSource.Play();
    }

    public void PlayLoseMusic()
    {
        if (loseMusic == null) return;
        musicSource.Stop();
        musicSource.clip = loseMusic;
        musicSource.loop = false;
        if (musicEnabled)
            musicSource.Play();
    }

    public void ResumeBackgroundMusic()
    {
        PlayBackgroundMusic();
    }

    // ---- SFX ----

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSFX);
    }

    public void PlaySwap()
    {
        PlaySFX(swapSFX);
    }

    public void PlayPieceLocked()
    {
        PlaySFX(pieceLockedSFX);
        Vibrate();
    }

    void PlaySFX(AudioClip clip)
    {
        if (!sfxEnabled || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // ---- VIBRATION ----

    public void Vibrate()
    {
        if (!vibrationEnabled) return;
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    // ---- SETTINGS ----

    public void SetMusic(bool enabled)
    {
        musicEnabled = enabled;
        if (musicSource != null)
            musicSource.volume = enabled ? 1f : 0f;
        PlayerPrefs.SetInt("MusicEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSFX(bool enabled)
    {
        sfxEnabled = enabled;
        PlayerPrefs.SetInt("SFXEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVibration(bool enabled)
    {
        vibrationEnabled = enabled;
        PlayerPrefs.SetInt("VibrationEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void PlayChainBreak()
    {
        PlaySFX(chainBreakSFX);
    }

    public bool IsMusicEnabled() => musicEnabled;
    public bool IsSFXEnabled() => sfxEnabled;
    public bool IsVibrationEnabled() => vibrationEnabled;

    void LoadSettings()
    {
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
        vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
    }
}