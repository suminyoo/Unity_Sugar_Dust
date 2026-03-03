using UnityEngine;

public class AlienVoicePlayer : MonoBehaviour
{
    public SoundData voiceSound;

    public float pitchVariation = 0.5f;

    public void PlayRandomSyllable()
    {
        if (voiceSound.clip != null )
            SoundManager.Instance.PlaySFX2D(voiceSound, pitchVariation);
    }
}