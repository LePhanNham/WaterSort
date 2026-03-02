using UnityEngine;

/// <summary>
/// Quản lý âm thanh game (SFX)
/// </summary>
public class SoundManager : SingletonMono<SoundManager>
{
    [Header("Audio Sources")]
    public AudioSource sfxSource; // Dùng cho sound effects
    public AudioSource bgmSource; // Dùng cho background music
    
    [Header("Background Music")]
    public AudioClip backgroundMusic; // Nhạc nền game
    
    [Header("Sound Effects")]
    public AudioClip bottleUpSound;      // Khi tube bay lên
    public AudioClip bottleDownSound;    // Khi tube hạ xuống
    public AudioClip bottleFullSound;    // Khi tube hoàn thành (đầy)
    public AudioClip bottleCloseSound;   // Khi đóng nắp
    [Tooltip("Pour sound timing có thể điều chỉnh trong TubeView.pourSoundOffset")]
    public AudioClip pourSound;          // Khi rót nước (optional)
    public AudioClip errorSound;         // Khi click sai (optional)
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;
    
    private bool soundEnabled = true;
    private bool bgmEnabled = true;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Tạo AudioSource cho SFX nếu chưa có
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        // Tạo AudioSource cho BGM nếu chưa có
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }
        
        // Load sound settings
        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        bgmEnabled = PlayerPrefs.GetInt("BGMEnabled", 1) == 1;
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        
        // Play background music
        PlayBackgroundMusic();
    }
    
    public void PlayBottleUp()
    {
        PlaySFX(bottleUpSound);
    }
    
    public void PlayBottleDown()
    {
        PlaySFX(bottleDownSound);
    }
    
    public void PlayBottleFull()
    {
        PlaySFX(bottleFullSound);
    }
    
    public void PlayBottleClose()
    {
        PlaySFX(bottleCloseSound);
    }
    
    public void PlayPour()
    {
        if (!soundEnabled || pourSound == null || sfxSource == null) return;
        
        // Dùng Play() thay vì PlayOneShot() để có thể Stop()
        sfxSource.clip = pourSound;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
        sfxSource.Play();
    }
    
    public void StopPour()
    {
        if (sfxSource != null && sfxSource.isPlaying && sfxSource.clip == pourSound)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }
    }
    
    public void PlayError()
    {
        PlaySFX(errorSound);
    }
    
    private void PlaySFX(AudioClip clip)
    {
        if (!soundEnabled || clip == null || sfxSource == null) return;
        
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
    
    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
        PlayerPrefs.SetInt("SoundEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    public bool IsSoundEnabled() => soundEnabled;
    
    // ==================== BGM Methods ====================
    
    private void PlayBackgroundMusic()
    {
        if (bgmSource == null || backgroundMusic == null) return;
        
        bgmSource.clip = backgroundMusic;
        bgmSource.volume = bgmEnabled ? bgmVolume : 0f;
        bgmSource.loop = true;
        
        if (!bgmSource.isPlaying)
            bgmSource.Play();
    }
    
    public void SetBGMEnabled(bool enabled)
    {
        bgmEnabled = enabled;
        PlayerPrefs.SetInt("BGMEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
        
        if (bgmSource != null)
        {
            bgmSource.volume = bgmEnabled ? bgmVolume : 0f;
        }
    }
    
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.Save();
        
        if (bgmSource != null && bgmEnabled)
        {
            bgmSource.volume = bgmVolume;
        }
    }
    
    public bool IsBGMEnabled() => bgmEnabled;
}
