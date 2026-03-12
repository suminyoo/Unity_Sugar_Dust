using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class PlayerHitEffect : MonoBehaviour
{
    [Header("References")]
    public PostProcessVolume postProcessVolume;
    public PlayerCondition playerCondition; 

    [Header("Hit Effect Settings")]
    public Color hitColor = new Color(0.8f, 0f, 0f);
    [Range(0f, 1f)] public float hitIntensity = 0.45f;
    public float effectDuration = 0.3f;

    private Vignette vignette;
    private float originalIntensity;
    private Color originalColor;
    private Coroutine hitEffectCoroutine;

    private void Start()
    {
        if (postProcessVolume.profile.TryGetSettings(out vignette))
        {
            originalIntensity = vignette.intensity.value;
            originalColor = vignette.color.value;
        }


        if (playerCondition != null)
        {
            playerCondition.OnTakeDamage += TriggerHitEffect;
        }
    }

    private void OnDestroy()
    {
        if (playerCondition != null)
        {
            playerCondition.OnTakeDamage -= TriggerHitEffect;
        }
    }

    private void TriggerHitEffect()
    {
        if (vignette == null) return;

        if (hitEffectCoroutine != null)
        {
            StopCoroutine(hitEffectCoroutine);
        }
        hitEffectCoroutine = StartCoroutine(HitEffectRoutine());
    }

    private IEnumerator HitEffectRoutine()
    {
        vignette.color.value = hitColor;
        vignette.intensity.value = hitIntensity;

        float timer = 0f;

        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float t = timer / effectDuration;

            vignette.color.value = Color.Lerp(hitColor, originalColor, t);
            vignette.intensity.value = Mathf.Lerp(hitIntensity, originalIntensity, t);

            yield return null; 
        }

        vignette.color.value = originalColor;
        vignette.intensity.value = originalIntensity;
    }
}