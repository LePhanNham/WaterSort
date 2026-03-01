using UnityEngine;

/// <summary>
/// Quản lý âm thanh game (SFX)
/// </summary>
public class SoundManager : SingletonMono<SoundManager>
{
    [Header("Audio Sources")]
    public AudioSource sfxSource; // Dùng cho sound effects
    
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
    
    private bool soundEnabled = true;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Tạo AudioSource nếu chưa có
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        // Load sound settings
        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
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
        PlaySFX(pourSound);
    }
    
    public void StopPour()
    {
        if (sfxSource != null && sfxSource.isPlaying && sfxSource.clip == pourSound)
        {
            sfxSource.Stop();
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
}
