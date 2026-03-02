using UnityEngine;

public class AlienVoicePlayer : MonoBehaviour
{
    public AudioClip[] voiceClips;

    public float pitchVariation = 0.5f;

    [Range(0f, 1f)] public float volume = 1.0f;

    public void PlayRandomSyllable()
    {
        if (voiceClips == null || voiceClips.Length == 0 || SoundManager.Instance == null) return;

        int randomIndex = Random.Range(0, voiceClips.Length);
        AudioClip clip = voiceClips[randomIndex];

        Vector3 playPos = Camera.main != null ? Camera.main.transform.position : transform.position;
        SoundManager.Instance.PlaySFX(clip, playPos, volume, pitchVariation);
    }
}