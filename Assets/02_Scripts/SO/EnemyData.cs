using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Explore/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public float maxHp = 100f;
    public GameObject deathEffect;

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
}