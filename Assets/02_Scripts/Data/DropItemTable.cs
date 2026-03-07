using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Drop Item Table", menuName = "Game/Drop Table")]
public class DropItemTable : ScriptableObject
{
    [System.Serializable]
    public class DropEntry
    {
        public GameObject itemPrefab;
        [Range(0, 100f)] public float dropChance;
        public int minAmount;
        public int maxAmount;
    }

    public List<DropEntry> dropList = new List<DropEntry>();

    [Header("Spawn Settings")]
    public float spawnRadius = 2.0f;

    public void SpawnItem(Vector3 centerPosition)
    {
        foreach (var entry in dropList)
        {
            // È®·ü Ã¼Å©
            if (Random.Range(0f, 100f) > entry.dropChance) continue;

            int count = Random.Range(entry.minAmount, entry.maxAmount + 1);

            for (int i = 0; i < count; i++)
            {
                if (entry.itemPrefab != null)
                {
                    // ·£´ý À§Ä¡
                    Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                    Vector3 randomPos = centerPosition + new Vector3(randomCircle.x, 0.5f, randomCircle.y);
                    Instantiate(entry.itemPrefab, randomPos, Quaternion.identity);
                }
            }
        }
    }
}