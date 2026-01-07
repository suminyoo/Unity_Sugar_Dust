using UnityEngine;

public class ItemUIPopupManager : MonoBehaviour
{
    public static ItemUIPopupManager Instance;

    [Header("Panels")]
    public ItemInfoUI itemInfoUI;           // 기존 정보창
    // public PriceSettingUI priceSettingUI; // (새로 만들) 가격 설정창
    // public ShopBuyConfirmUI shopBuyUI;    // (새로 만들) 구매 확인창

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 아이템 정보창 열기
    public void ShowItemInfo(ItemData data)
    {
        CloseAllPopups();
        itemInfoUI.OpenPanel(data);
    }

    // 진열대 아이템 창
    public void ShowPriceSetting(DisplayStand stand, int slotIndex, ItemData data)
    {
        CloseAllPopups();
        // priceSettingUI.Open(stand, slotIndex, data);
        Debug.Log($"[Popup] 가격 설정창 오픈: {data.itemName}");
    }

    // 상점용 아이템 창
    public void ShowBuyConfirm(ItemData data, int price)
    {
        CloseAllPopups();
        // shopBuyUI.Open(data, price);
        Debug.Log($"[Popup] 구매창 오픈: {data.itemName} ({price}G)");
    }

    public void CloseAllPopups()
    {
        itemInfoUI.Close();
        // priceSettingUI.Close();
        // shopBuyUI.Close();
    }
}
