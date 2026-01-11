using System.Collections.Generic;
using UnityEngine;

public class NPCShop : InventoryHolder, IShopSource
{
    [Header("Data Source")]
    public ShopData shopData; // 사용할 상점 데이터

    [Header("Settings")]
    public float priceMarkup = 1.0f; // 가격 배율 (1.0 = 정가)
    private string uiShopType = "Weapon"; // StorageUIManager에서 사용할 switch case 이름
    private int shopInventorySize = 10;

    private List<int> slotPrices = new List<int>();

    protected override void Awake()
    {
        base.Awake();
        // InventoryHolder의 Awake에서 기본 시스템을 생성하지만,
        // 여기서는 ShopData 크기에 맞춰서 재생성하거나 초기화해야 합니다.
    }

    private void Start()
    {
        if (shopData != null)
        {
            InitializeShop();
        }
    }

    // 데이터 기반으로 인벤토리 세팅
    public void InitializeShop()
    {
        // 인벤토리 시스템 생성
        inventorySystem = new InventorySystem(shopInventorySize);

        slotPrices.Clear();

        // 아이템 채우기 및 가격 책정
        for (int i = 0; i < shopData.itemsForSale.Count; i++)
        {
            ItemData item = shopData.itemsForSale[i];
            if (item != null)
            {
                // 인벤토리에 1개씩
                inventorySystem.AddItemToSlots(item, 1);

                slotPrices.Add(item.basePrice);
            }
            else
            {
                slotPrices.Add(0);
            }
        }
        // 나머지 빈 슬롯 가격 0
        while (slotPrices.Count < inventorySystem.maxSlots)
        {
            slotPrices.Add(0);
        }
    }

    public int GetPrice(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotPrices.Count) return 0;
        return slotPrices[slotIndex];
    }

    public bool IsSlotActive(int slotIndex)
    {
        // 물건이 있으면 활성화
        if (inventorySystem == null || slotIndex >= inventorySystem.slots.Count) return false;
        return !inventorySystem.slots[slotIndex].IsEmpty;
    }

    public string GetShopType() => uiShopType;
}