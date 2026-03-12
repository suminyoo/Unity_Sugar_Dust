using UnityEngine;
public enum ToolType
{
    None,
    Blue,
    Green,
    Red,
    White,
    Black,
    Rainbow  
}

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Game/Item Data/Weapon")]
public class WeaponItemData : ItemData
{
    [Header("Weapon Stats")]
    public float attackDamage;
    public float attackSpeed;
    public float cooldown;

    public Vector3 attackBoxSize = new Vector3(1f, 1f, 1f); 

    public ToolType toolType = ToolType.None;

}