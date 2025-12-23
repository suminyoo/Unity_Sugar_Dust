using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    public float hp = 50f;
    public float moveSpeed = 2f;
    public GameObject deathEffect;

    public void OnDamage(float damage)
    {
        hp -= damage;

        Debug.Log($"적 피격! 남은 HP: {hp}");

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("생명체 처치");

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}