using UnityEngine;

public class NPCShop : InventoryHolder, IShopSource
{
    [Header("Data Source")]
    public ShopData shopData; // 사용할 상점 데이터

    [Header("Settings")]
    private string uiShopType = "Weapon";

    protected override void Awake()
    {
        base.Awake();
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
        int size = (shopData != null && shopData.itemsForSale != null) ? shopData.itemsForSale.Count : 0;
        if (size == 0) size = 1; //사이즈 0이어도 최소 한칸
        
        inventorySystem = new InventorySystem(size);

        // 아이템 배치
        for (int i = 0; i < shopData.itemsForSale.Count; i++)
        {
            var info = shopData.itemsForSale[i];
            if (info.itemData == null) continue; // 데이터 비어있으면 패스

            //재고수량 적용 (무한이면 1, 그외는 수량만큼)
            int amount = info.isInfinite ? 1 : info.stockCount;

            // 수량 설정을 깜빡해서 0으로 해뒀다면 최소 1개는 넣기
            if (amount <= 0) amount = 1;
            inventorySystem.UpdateSlotAtIndex(i, info.itemData, amount);
        }
    }

    public int GetPrice(int slotIndex)
    {
        // 데이터가 없거나 리스트가 비어있거나 유효한 슬롯이 아니면 0원 반환
        if (shopData == null || shopData.itemsForSale == null) return 0;
        if (!IsValidIndex(slotIndex)) return 0;

        var info = shopData.itemsForSale[slotIndex];

        // 오버라이드 가격이 설정되어 있다면(-1이 아니면, 0도 기본가격으로) 우선 사용
        if (info.priceOverride > 0) return info.priceOverride;

        // 설정 없으면 아이템 기본 가격
        return info.itemData.basePrice;
    }

    //재고 개수를 반환. 만약 무한대라면 -1 반환
    public int GetStockAmount(int slotIndex)
    {
        if (!IsValidIndex(slotIndex)) return 0;

        var info = shopData.itemsForSale[slotIndex];
        if (info.isInfinite) // 무한대
            return -1; 
        var slot = inventorySystem.Slots[slotIndex];
        return slot.Amount;
    }

    public bool IsSlotActive(int slotIndex)
    {
        if (!IsValidIndex(slotIndex)) return false;

        // 품절
        return !inventorySystem.Slots[slotIndex].IsEmpty;
    }

    // 구매 눌렀을 때 호출
    public bool TryPurchaseItem(int slotIndex, PlayerInventory player)
    {
        if (!IsSlotActive(slotIndex)) return false;

        var info = shopData.itemsForSale[slotIndex];
        var slot = inventorySystem.Slots[slotIndex];
        int price = GetPrice(slotIndex);

        if (!PlayerAssetsManager.Instance.CheckMoney(price)) return false;

        int remaining = player.AddItem(slot.ItemData, 1);

        if (remaining == 0) // 성공
        {
            PlayerAssetsManager.Instance.TrySpendMoney(price); // 돈 차감

            if (!info.isInfinite)
            {
                // 한정 수량일 때만 상점에서 제거
                inventorySystem.RemoveItemAtIndex(slotIndex, 1);
            }
            NotificationUIManager.Instance.ShowNotification($"{slot.ItemData.itemName}을(를) 구매했습니다.");
            return true;
        }
        else
        {
            return false;
        }

    }

    private bool IsValidIndex(int i)
    {
        return i >= 0 && i < shopData.itemsForSale.Count;
    }

    public string GetShopType() => uiShopType;
}