using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Mixer and Groups")]
    public AudioMixer mainMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup ambienceGroup;
    public AudioMixerGroup leakGroup;
    public AudioMixerGroup patchGroup;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambienceSource;
    public AudioSource leakSource;
    public AudioSource patchSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip backgroundAmbience;
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
        musicSource.outputAudioMixerGroup = musicGroup;
        ambienceSource.outputAudioMixerGroup = ambienceGroup;
        leakSource.outputAudioMixerGroup = leakGroup;
        patchSource.outputAudioMixerGroup = patchGroup;

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

    public void SetLeakVolume(float volume)
    {
        mainMixer.SetFloat("LeakVolume", Mathf.Log10(volume) * 20);
    }

    public void SetPatchVolume(float volume)
    {
        mainMixer.SetFloat("PatchVolume", Mathf.Log10(volume) * 20);
    }
}