using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Game/Item Data/Weapon")]
public class WeaponItemData : ItemData
{
    [Header("Weapon Stats")]
    public float attackDamage;
    public float attackSpeed;
    public float cooldown;
}