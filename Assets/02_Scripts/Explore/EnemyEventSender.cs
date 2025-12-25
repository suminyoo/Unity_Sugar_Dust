using UnityEngine;

public class EnemyEventSender : MonoBehaviour
{
    private Enemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }
    public void EnableAttackHitBox() => EnableHitBox();

    public void DisableAttackHitBox() => DisableHitBox();

    private void EnableHitBox() => enemy.EnableAttackHitBox();
    
    private void DisableHitBox() => enemy.DisableAttackHitBox();
    
}