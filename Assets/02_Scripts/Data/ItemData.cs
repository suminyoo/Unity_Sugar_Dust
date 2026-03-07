using UnityEngine;
using UnityEngine.Localization;

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
    [SerializeField] private LocalizedString itemName;

    public ItemType itemType;

    [SerializeField] private LocalizedString description;
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

    public string GetItemName()
    {
        return itemName.GetLocalizedString();
    }

    public string GetDescription()
    {
        return description.GetLocalizedString();
    }

    public string GetItemTypeName()
    {
        return itemType switch
        {
            ItemType.Mineral => LocalizationHelper.L("ITEM_TYPE_MINERAL"),
            ItemType.MonsterLoot => LocalizationHelper.L("ITEM_TYPE_MONSTER_LOOT"),
            ItemType.Weapon => LocalizationHelper.L("ITEM_TYPE_WEAPON"),
            _ => LocalizationHelper.L("ITEM_TYPE_DEFAULT")
        };
    }
}