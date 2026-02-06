using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 배경음 재생
    public void PlayBGM(AudioClip clip, float volume = 0.5f)
    {
        if (bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.spatialBlend = 0; // BGM은 2D
        bgmSource.Play();
    }

    // 효과음 재생
    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1.0f)
    {
        if (clip == null) return;

        GameObject go = new GameObject("TempSFX_" + clip.name);
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 1.0f;
        source.minDistance = 1f;
        source.maxDistance = 20f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        source.Play();

        Destroy(go, clip.length);
    }
}