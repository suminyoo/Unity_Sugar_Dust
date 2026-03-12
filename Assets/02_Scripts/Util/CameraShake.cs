using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public PlayerCondition playerCondition;

    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;

    private Vector3 originalLocalPos;
    private Coroutine shakeCoroutine;

    private void Start()
    {
        if (playerCondition != null)
        {
            playerCondition.OnTakeDamage += TriggerShake;
        }
    }

    private void OnDestroy()
    {
        if (playerCondition != null)
        {
            playerCondition.OnTakeDamage -= TriggerShake;
        }
    }

    public void TriggerShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalLocalPos;
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        originalLocalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = new Vector3(originalLocalPos.x + x, originalLocalPos.y + y, originalLocalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPos;
    }
}