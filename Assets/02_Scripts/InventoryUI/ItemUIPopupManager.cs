using System;
using UnityEngine;

public class ItemUIPopupManager : MonoBehaviour
{
    public static ItemUIPopupManager Instance;

    [Header("Panels")]
    public ItemUsageInfoUI usageInfoUI; // 기존 정보창
    public ItemPriceInfoUI priceInfoUI; // 가격 설정창
    public ItemPurchaseInfoUI purchaseInfoUI;    // 구매창

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
    public void ShowPriceInfo(ItemData data, int currentPrice, bool isActive,
                                 Action<int> onPriceChanged, Action<bool> onActiveChanged)
    {
        priceInfoUI.OpenPanel(data, currentPrice, isActive, onPriceChanged, onActiveChanged);
    }

    // 상점용 아이템 창
    public void ShowPurchaseInfo(ItemData data, int currentPrice, Action onConfirm)
    {
        purchaseInfoUI.OpenPanel(data, currentPrice, onConfirm);
    }

    //TODO: 로직 수정??
    public void CloseAllPopups()
    {
        usageInfoUI.Close();
        priceInfoUI.Close();
        purchaseInfoUI.Close();
        // shopBuyUI.Close();
    }
}
