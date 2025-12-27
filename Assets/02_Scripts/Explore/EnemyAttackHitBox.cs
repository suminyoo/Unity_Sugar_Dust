using UnityEngine;

public class EnemyAttackHitBox : MonoBehaviour
{
    private Enemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCondition targetCondition = other.GetComponent<PlayerCondition>();

            if (targetCondition != null && enemy != null)
            {
                targetCondition.TakeDamage(enemy.data.attackDamage);

                Debug.Log($"Attack Target: {other.name}");
            }
        }
    }
}