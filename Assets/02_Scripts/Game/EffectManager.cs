using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayEffect(GameObject prefab, Vector3 position, float duration = 2.0f)
    {
        if (prefab == null) return;

        GameObject effect = Instantiate(prefab, position, Quaternion.identity);

        Destroy(effect, duration);
    }
}