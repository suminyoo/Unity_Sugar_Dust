using UnityEngine;

[CreateAssetMenu(fileName = "New Mineral Data", menuName = "Explore/Mineral Data")]
public class MineralData : ScriptableObject
{
    [Header("Basic Info")]
    public string mineralName;
    public float maxHealth;

    [Header("Visual & Audio")]
    public GameObject hitEffectPrefab;
    public AudioClip mineSound;
    public AudioClip destroySound;

    [Header("Loot")]
    public DropItemTable lootTable;
}