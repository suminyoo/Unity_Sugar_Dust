using UnityEngine;
using UnityEngine.Localization;

public enum ItemType
{
    [InspectorName("광물")] Mineral,
    [InspectorName("몬스터 부산물")] MonsterLoot,
    [InspectorName("도구")] Tool,
    [InspectorName("물약")] Potion,
    [InspectorName("업그레이드")] Upgrade


}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Game/Item Data/Default Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public ItemID itemID;
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

    public virtual string GetDescription()
    {
        return description.GetLocalizedString();
    }

    public string GetItemTypeName()
    {
        return itemType switch
        {
            ItemType.Mineral => LocalizationHelper.Main("ITEM_TYPE_MINERAL"),
            ItemType.MonsterLoot => LocalizationHelper.Main("ITEM_TYPE_MONSTER_LOOT"),
            ItemType.Tool => LocalizationHelper.Main("ITEM_TYPE_WEAPON"),
            ItemType.Potion => LocalizationHelper.Main("ITEM_TYPE_POTION"),
            _ => LocalizationHelper.Main("ITEM_TYPE_DEFAULT")
        };
    }

    public virtual bool IsUsable()
    {
        return false;
    }

    public virtual bool Use(GameObject target)
    {
        return false;
    }
}