using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;

    [Header("Music")]
    private AudioSource musicSource;
    private AudioClip currentMusicClip;

    [Header("Player SFX")]
    [SerializeField] private AudioClip playerAttackMelee;
    [SerializeField] private AudioClip playerAttackRanged;
    [SerializeField] private AudioClip[] playerHurt; // Array for variations
    [SerializeField] private AudioClip playerDeath;
    [SerializeField] private AudioClip[] playerFootstep; // Array for variations
    [SerializeField] private AudioClip playerAbilityQ;
    [SerializeField] private AudioClip playerAbilityF;
    [SerializeField] private AudioClip playerAbilityE;

    [Header("Enemy SFX")]
    [SerializeField] private AudioClip[] enemyFootstep; // Array for variations
    [SerializeField] private AudioClip[] enemyHurt; // Array for variations
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private AudioClip enemyAttack;
    [SerializeField] private AudioClip enemySpawn;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip buttonHover;
    [SerializeField] private AudioClip waveComplete;
    [SerializeField] private AudioClip levelComplete;

    [Header("Projectile SFX")]
    [SerializeField] private AudioClip projectileLaunch;
    [SerializeField] private AudioClip projectileHit;
    [SerializeField] private AudioClip projectileExplode;

    [Header("Ambient SFX")]
    [SerializeField] private AudioClip ambientWind;
    [SerializeField] private AudioClip ambientFire;

    [Header("Audio Settings")]
    [SerializeField] private int maxSimultaneousSounds = 10;
    [SerializeField] private float defaultVolume = 1f;

    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicMixerGroup;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = defaultVolume;

        // Pre-populate audio source pool
        for (int i = 0; i < maxSimultaneousSounds; i++)
        {
            CreateNewAudioSource();
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxMixerGroup;
        source.playOnAwake = false;
        audioSourcePool.Enqueue(source);
        return source;
    }

    private AudioSource GetAudioSource()
    {
        // Clean up finished audio sources
        activeAudioSources.RemoveAll(source => !source.isPlaying);

        // Return pooled source or create new one if pool is empty
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }

        return CreateNewAudioSource();
    }

    private void ReturnAudioSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        if (!audioSourcePool.Contains(source))
        {
            audioSourcePool.Enqueue(source);
        }
    }

    /// <summary>
    /// Get a random clip from an array of clips
    /// </summary>
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    /// <summary>
    /// Play a one-shot sound effect
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetAudioSource();
        source.clip = clip;
        source.volume = volume * defaultVolume;
        source.pitch = pitch;
        source.loop = false;
        source.Play();

        activeAudioSources.Add(source);
    }

    /// <summary>
    /// Play a sound effect at a specific position in 3D space
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, volume * defaultVolume);
    }

    /// <summary>
    /// Play music track with optional fade
    /// </summary>
    public void PlayMusic(AudioClip clip, bool fade = true, float fadeDuration = 1f)
    {
        if (clip == null) return;

        // Don't restart the same music if it's already playing
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        currentMusicClip = clip;

        if (fade)
        {
            StartCoroutine(FadeMusic(clip, fadeDuration));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    private System.Collections.IEnumerator FadeMusic(AudioClip newClip, float duration)
    {
        // Fade out
        float startVolume = musicSource.volume;
        for (float t = 0; t < duration / 2; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / (duration / 2));
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        for (float t = 0; t < duration / 2; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, defaultVolume, t / (duration / 2));
            yield return null;
        }

        musicSource.volume = defaultVolume;
    }

    public void StopMusic(bool fade = true, float fadeDuration = 1f)
    {
        if (fade)
        {
            StartCoroutine(FadeMusicOut(fadeDuration));
        }
        else
        {
            musicSource.Stop();
        }
    }

    private System.Collections.IEnumerator FadeMusicOut(float duration)
    {
        float startVolume = musicSource.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = defaultVolume;
    }

    // Convenient methods for specific sounds
    public void PlayPlayerAttack(bool isRanged, float volume = 0.6f) => PlaySFX(isRanged ? playerAttackRanged : playerAttackMelee, volume);
    public void PlayPlayerHurt() => PlaySFX(GetRandomClip(playerHurt), 0.5f);
    public void PlayPlayerDeath() => PlaySFX(playerDeath);
    public void PlayPlayerFootstep(float volume = 0.3f) => PlaySFX(GetRandomClip(playerFootstep), volume);
    public void PlayPlayerAbilityQ(float volume = 0.7f) => PlaySFX(playerAbilityQ, volume);
    public void PlayPlayerAbilityF(float volume = 0.7f) => PlaySFX(playerAbilityF, volume);
    public void PlayPlayerAbilityE(float volume = 0.6f) => PlaySFX(playerAbilityE, volume);

    public void PlayEnemyFootstep(float volume = 0.3f) => PlaySFX(GetRandomClip(enemyFootstep), volume);
    public void PlayEnemyHurt() => PlaySFX(GetRandomClip(enemyHurt), 0.3f);
    public void PlayEnemyDeath() => PlaySFX(enemyDeath);
    public void PlayEnemyAttack() => PlaySFX(enemyAttack);
    public void PlayEnemySpawn() => PlaySFX(enemySpawn, 0.5f);

    public void PlayButtonClick() => PlaySFX(buttonClick);
    public void PlayButtonHover() => PlaySFX(buttonHover, 0.5f);
    public void PlayWaveComplete() => PlaySFX(waveComplete);
    public void PlayLevelComplete() => PlaySFX(levelComplete);

    public void PlayProjectileLaunch() => PlaySFX(projectileLaunch, 0.7f);
    public void PlayProjectileHit() => PlaySFX(projectileHit);
    public void PlayProjectileExplode() => PlaySFX(projectileExplode);

    public void SetMasterVolume(float volume)
    {
        // Note: You'll need to expose parameters in your Audio Mixer
        // Right-click on volume in mixer -> Expose Parameter
        // Then set the name to "MasterVolume", "MusicVolume", "SFXVolume"
        sfxMixerGroup.audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        defaultVolume = volume;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    // Test methods for debugging
    [ContextMenu("Test Player Footstep")]
    private void TestPlayerFootstep() => PlayPlayerFootstep();

    [ContextMenu("Test Player Hurt")]
    private void TestPlayerHurt() => PlayPlayerHurt();

    [ContextMenu("Test Enemy Footstep")]
    private void TestEnemyFootstep() => PlayEnemyFootstep();

    [ContextMenu("Test Enemy Hurt")]
    private void TestEnemyHurt() => PlayEnemyHurt();
}
