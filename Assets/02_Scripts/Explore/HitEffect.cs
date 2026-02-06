using UnityEngine;
using System.Collections;

public class HitEffect : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float flashIntensity = 5f;

    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                mat.EnableKeyword("_EMISSION");
            }
        }
    }

    public void PlayHitFlash()
    {
        StopAllCoroutines();
        StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        SetFlash(Color.white, flashIntensity);
        yield return new WaitForSeconds(flashDuration);
        SetFlash(Color.black, 0f);
    }

    private void SetFlash(Color color, float intensity)
    {
        foreach (var r in renderers)
        {
            r.GetPropertyBlock(propBlock);

            propBlock.SetColor("_EmissionColor", color);
            //propBlock.SetColor("_ColorTint1", color);
            propBlock.SetFloat("_EmissionPower", intensity);

            r.SetPropertyBlock(propBlock);
        }
    }
}