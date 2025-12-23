using UnityEngine;

public class Mineral : MonoBehaviour, IMineable
{
    [Header("Mineral Stats")]
    public string mineralName = "Raw Resource";
    public float health = 100f;
    public GameObject hitEffectPrefab; // Ã¤±¼ ½Ã ÆÄÆí ÀÌÆåÆ®
    public GameObject dropItemPrefab;  // ÆÄ±« ½Ã µå¶ø ¾ÆÀÌÅÛ

    public void OnMine(float power)
    {
        health -= power;
        Debug.Log($"{mineralName} Ã¤±¼ Áß... ³²Àº Ã¼·Â: {health}");

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        if (health <= 0)
        {
            Break();
        }
    }

    void Break()
    {
        Debug.Log($"{mineralName} ÆÄ±«µÊ");

        if (dropItemPrefab != null)
        {
            Instantiate(dropItemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}