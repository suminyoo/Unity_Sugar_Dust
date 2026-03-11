using UnityEngine;

public abstract class ConsumableItemData : ItemData
{
    [Header("Use Settings")]
    public AudioClip useSound;
    public float cooldownTime;
    public bool isConsumedOnUse = true;

    public override bool IsUsable() => true;

    public abstract override bool Use(GameObject target);
}