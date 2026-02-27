using UnityEngine;

public enum ItemType
{
    [InspectorName("광물")] Mineral,
    [InspectorName("몬스터 부산물")] MonsterLoot,
    [InspectorName("무기")] Weapon
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID;
    public string itemName;

    public ItemType itemType;

    public string itemTypeName
    {
        get
        {
            return itemType switch
            {
                ItemType.Mineral => "광물",
                ItemType.MonsterLoot => "몬스터 부산물",
                ItemType.Weapon => "무기",
                _ => "아이템"
            };
        }
    }

    [TextArea] public string description;
    public Sprite icon;

    [Header("Stats")]
    public float weight;
    public int basePrice;
    public bool isStackable;
    public int maxStackAmount = 99;

    [Header("World Object")]
    public GameObject dropPrefab;

    [Header("Price")]
    public int sellPrice;



}