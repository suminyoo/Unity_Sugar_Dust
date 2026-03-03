using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TimeBGM
{
    public GAME_TIME time;
    public AudioClip[] bgmClips;
}

/// SoundManager.Instance.PlaySFX(sound, transform.position);

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("BGM Settings")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<TimeBGM> timeBGMList;

    private Coroutine fadeCoroutine;

    [Header("SFX Settings")]
    [SerializeField] private int sfxPoolSize = 20;
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSFXPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameManager.OnTimeChanged += OnTimeChangedHandler;
    }

    private void OnDisable()
    {
        GameManager.OnTimeChanged -= OnTimeChangedHandler;
    }
    private void Start()
    {
        InitVolumeSettings();
    }

    private void InitVolumeSettings()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        SetMasterVolume(master);
        SetBGMVolume(bgm);
        SetSFXVolume(sfx);
    }

    #region SFX

    private void InitializeSFXPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject go = new GameObject($"SFX_Source_{i}");
            go.transform.SetParent(this.transform);

            AudioSource source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxMixerGroup;
            source.spatialBlend = 1.0f;
            source.minDistance = 1f;
            source.maxDistance = 20f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.playOnAwake = false;

            sfxPool.Enqueue(source);
        }
    }

    public void PlaySFX2D(AudioClip clip, float volume = 1.0f, float pitchVariation = 0.1f)
    {
        PlaySFX(clip, Camera.main.transform.position, volume, pitchVariation, true);
    }

    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1.0f, float pitchVariation = 0.1f, bool is2D = false)
    {
        if (clip == null) return;

        AudioSource source = sfxPool.Dequeue();

        source.spatialBlend = is2D ? 0f : 1.0f;
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        source.Play();

        sfxPool.Enqueue(source);
    }

    #endregion

    #region BGM

    private void OnTimeChangedHandler(GAME_TIME newTime, bool isInstant)
    {
        TimeBGM currentBgms = timeBGMList.Find(x => x.time == newTime);

        if (currentBgms != null && currentBgms.bgmClips.Length > 0)
        {
            int randomIndex = Random.Range(0, currentBgms.bgmClips.Length);
            AudioClip nextClip = currentBgms.bgmClips[randomIndex];

            if (isInstant) PlayBGM(nextClip, 0f, 0f, 0f);
            else PlayBGM(nextClip, 1.0f, 0.5f, 1.0f);
        }
    }

    public void PlayBGM(AudioClip nextClip, float fadeOutTime = 1.0f, float waitTime = 0.5f, float fadeInTime = 1.0f)
    {
        if (bgmSource.clip == nextClip) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(SequentialFadeCoroutine(nextClip, fadeOutTime, waitTime, fadeInTime));
    }

    private IEnumerator SequentialFadeCoroutine(AudioClip nextClip, float fadeOutTime, float waitTime, float fadeInTime)
    {
        // Fade Out
        if (bgmSource.isPlaying && fadeOutTime > 0)
        {
            float timer = 0;
            float startVolume = bgmSource.volume;

            while (timer < fadeOutTime)
            {
                timer += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutTime);
                yield return null;
            }
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();

        if (waitTime > 0)
        {
            yield return new WaitForSecondsRealtime(waitTime);
        }

        // Fade In
        bgmSource.clip = nextClip;
        bgmSource.Play();

        if (fadeInTime > 0)
        {
            float timer = 0;
            while (timer < fadeInTime)
            {
                timer += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, 1f, timer / fadeInTime);
                yield return null;
            }
        }

        bgmSource.volume = 1f;
    }

    #endregion

    #region 설정창 볼륨 조절

    public void SetMasterVolume(float volume)
    {
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        mainMixer.SetFloat("Master_Volume", Mathf.Log10(safeVolume) * 20);
    }

    public void SetBGMVolume(float volume)
    {
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        mainMixer.SetFloat("BGM_Volume", Mathf.Log10(safeVolume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        mainMixer.SetFloat("SFX_Volume", Mathf.Log10(safeVolume) * 20);
    }

    #endregion
}