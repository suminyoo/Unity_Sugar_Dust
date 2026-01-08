using System;
using UnityEngine;

public class ItemUIPopupManager : MonoBehaviour
{
    public static ItemUIPopupManager Instance;

    [Header("Panels")]
    public ItemUsageInfoUI usageInfoUI; // 기존 정보창
    public ItemPriceInfoUI priceInfoUI; // 가격 설정창
    // public ShopBuyConfirmUI shopBuyUI;    // (새로 만들) 구매 확인창

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 아이템 정보창 열기
    public void ShowItemInfo(ItemData data)
    {
        usageInfoUI.OpenPanel(data);
    }

    // 진열대 아이템 창
    // Action<int> onPriceChanged: 가격 바뀔 때마다 연락받을 곳
    public void ShowPriceSetting(ItemData data, int currentPrice, bool isActive,
                                 Action<int> onPriceChanged, Action<bool> onActiveChanged)
    {
        CloseAllPopups();
        priceInfoUI.OpenPanel(data, currentPrice, isActive, onPriceChanged, onActiveChanged);
    }

    // 상점용 아이템 창
    public void ShowBuyConfirm(ItemData data, int price)
    {
        // shopBuyUI.Open(data, price);
        Debug.Log($"[Popup] 구매창 오픈: {data.itemName} ({price}G)");
    }

    public void CloseAllPopups()
    {
        usageInfoUI.Close();
        priceInfoUI.Close();
        // shopBuyUI.Close();
    }
}
