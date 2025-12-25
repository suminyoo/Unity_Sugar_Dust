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
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null && enemy != null)
            {
                player.TakeDamage(enemy.data.attackDamage);
                Debug.Log("Attack Target");
            }
        }
    }
}