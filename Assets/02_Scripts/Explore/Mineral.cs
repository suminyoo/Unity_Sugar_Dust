using UnityEngine;

public class Mineral : MonoBehaviour, IMineable
{
    [Header("Settings")]
    public string mineralName;
    public float health = 100f;
    public GameObject hitEffectPrefab;

    [Header("Loot System")]
    [SerializeField] private DropItemTable lootTable;

    public void OnMine(float power)
    {
        health -= power;

        if (hitEffectPrefab)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        if (health <= 0)
            Break();
    }

    void Break()
    {
        if (lootTable != null)
        {
            lootTable.SpawnItem(transform.position);
        }

        Destroy(gameObject);
    }
}