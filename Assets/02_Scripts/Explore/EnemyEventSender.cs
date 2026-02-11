using UnityEngine;

public class EnemyEventSender : MonoBehaviour
{
    private Enemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }
    public void EnableAttackHitBox() => enemy.EnableAttackHitBox();

    public void DisableAttackHitBox() => enemy.DisableAttackHitBox();

    
}