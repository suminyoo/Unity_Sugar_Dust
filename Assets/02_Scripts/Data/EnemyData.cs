using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Explore/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public EnemyID enemyID;
    [SerializeField] private LocalizedString enemyName;
    public float maxHp = 100f;

    [Header("Movement")]
    public float moveSpeed = 1.5f; 
    public float runSpeed = 3.0f;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float attackRange = 3f;

    [Header("AI Perception")]
    public float detectRange = 6f; 
    public float patrolRadius = 5f; 
    public float patrolWaitTime = 2f;

    [Header("Loot")]
    public DropItemTable lootTable;

    [Header("VFX")]
    public GameObject hitEffect;
    public GameObject attackEffect;
    public GameObject detectEffect;
    public GameObject patrolEffect;
    public GameObject dieEffect;

    [Header("SFX")]
    public SoundData hitSound;
    public SoundData attackSound;
    public SoundData detectSound;
    public SoundData dieSound;

    public string GetEnemyName()
    {
        return enemyName.GetLocalizedString();
    }
}