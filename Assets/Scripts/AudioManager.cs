using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Mixer and Groups")]
    public AudioMixer mainMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup ambienceGroup;
    public AudioMixerGroup soundEffectsGroup;
    public AudioMixerGroup collisionSoundGroup;
    public AudioMixerGroup crackSoundGroup;
    public AudioMixerGroup leakGroup;
    public AudioMixerGroup patchGroup;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambienceSource;
    public AudioSource collisionSoundSource;
    public AudioSource crackSoundSource;
    public AudioSource leakSource;
    public AudioSource patchSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip backgroundAmbience;
    public AudioClip collisionSound;
    public AudioClip crackSound;
    public AudioClip leakSpawnSound;
    public AudioClip patchSound;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (musicSource != null) musicSource.outputAudioMixerGroup = musicGroup;
        if (ambienceSource != null) ambienceSource.outputAudioMixerGroup = ambienceGroup;
        if (collisionSoundSource != null) collisionSoundSource.outputAudioMixerGroup = collisionSoundGroup;
        if (crackSoundSource != null) crackSoundSource.outputAudioMixerGroup = crackSoundGroup;
        if (leakSource != null) leakSource.outputAudioMixerGroup = leakGroup;
        if (patchSource!= null) patchSource.outputAudioMixerGroup = patchGroup;

        PlayBackgroundMusic();
        PlayAmbience();
    }

    // --- Public Methods for Audio Control ---
    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayAmbience()
    {
        if (ambienceSource != null && backgroundAmbience != null)
        {
            ambienceSource.clip = backgroundAmbience;
            ambienceSource.loop = true;
            ambienceSource.Play();
        }
    }

    public void PlayCollisionSound()
    {
        if (collisionSoundSource != null && collisionSound != null)
        {
            collisionSoundSource.PlayOneShot(collisionSound);
        }
    }

    public void PlayCrackSound()
    {
        if (crackSoundSource != null && crackSound != null)
        {
            crackSoundSource.PlayOneShot(crackSound);
        }
    }

    public void PlayLeakSpawnSound()
    {
        if (leakSource != null && leakSpawnSound != null)
        {
            leakSource.PlayOneShot(leakSpawnSound);
        }
    }

    public void PlayPatchSound()
    {
        if (patchSource != null && patchSound != null)
        {
            patchSource.PlayOneShot(patchSound);
        }
    }

    // --- New Method to Stop All Leak-Related Sounds ---
    public void StopAllLeakSounds()
    {
        if (collisionSoundSource != null && collisionSoundSource.isPlaying)
        {
            collisionSoundSource.Stop();
        }
        if (crackSoundSource != null && crackSoundSource.isPlaying)
        {
            crackSoundSource.Stop();
        }
        if (leakSource != null && leakSource.isPlaying)
        {
            leakSource.Stop();
        }
        if (patchSource != null && patchSource.isPlaying)
        {
            patchSource.Stop();
        }
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        if (ambienceSource != null)
        {
            ambienceSource.Play();
        }
    }

    // --- Methods to control the volume using the exposed parameters ---
    public void SetMasterVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetAmbienceVolume(float volume)
    {
        mainMixer.SetFloat("AmbienceVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSoundEffectsGroupVolume(float volume)
    {
        mainMixer.SetFloat("SoundEffectsGroupVolume", Mathf.Log10(volume) * 20);
    }

    public void SetCollisionSoundVolume(float volume)
    {
        mainMixer.SetFloat("CollisionSoundVolume", Mathf.Log10(volume) * 20);
    }

    public void SetCrackSoundVolume(float volume)
    {
        mainMixer.SetFloat("CrackSoundVolume", Mathf.Log10(volume) * 20);
    }

    public void SetLeakVolume(float volume)
    {
        mainMixer.SetFloat("LeakVolume", Mathf.Log10(volume) * 20);
    }

    public void SetPatchVolume(float volume)
    {
        mainMixer.SetFloat("PatchVolume", Mathf.Log10(volume) * 20);
    }
}