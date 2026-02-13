using UnityEngine;
using System.Collections.Generic;

public class PlayerAttackZone : MonoBehaviour
{
    private BoxCollider boxCol;
    private float damage;
    private bool isCritical;
    private ActionSystem.ActionType type;

    void Awake()
    {
        boxCol = GetComponent<BoxCollider>();
        boxCol.isTrigger = true;
        boxCol.enabled = false;
    }

    public void EnableZone(float damage, bool isCritical, ActionSystem.ActionType actionType)
    {
        this.damage = damage;
        this.isCritical = isCritical;
        type = actionType;

        boxCol.enabled = true;
    }

    public void DisableZone()
    {
        boxCol.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (type == ActionSystem.ActionType.Attack)
        {
            IDamageable target = other.GetComponentInParent<IDamageable>();

            if (target != null )
            {
                target.TakeDamage(damage, isCritical); 
            }
        }
        else if (type == ActionSystem.ActionType.Mine)
        {
            IMineable mineral = other.GetComponentInParent<IMineable>();

            if (mineral != null )
            {
                mineral.OnMine(damage, isCritical);
            }
        }
    }
}