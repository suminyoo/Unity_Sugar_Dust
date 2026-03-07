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
            PlayerCondition player = other.GetComponent<PlayerCondition>();

            if (player != null && enemy != null)
            {
                player.TakeDamage(enemy.data.attackDamage);
            }
        }
    }
}