using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemPurchaseInfoUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject itemInfoPanel;
    public GameObject defaultPanel;

    [Header("UI Elements")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI purchasePriceText;
    public TextMeshProUGUI itemWeightText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemStockAmountText;
    private Action onPurchased;


    private void Start()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }

    public void OpenPanel(ItemData data, int price, int stockAmount, Action onConfirm)
    {
        onPurchased = onConfirm;

        defaultPanel.SetActive(false);
        itemInfoPanel.SetActive(true);

        itemIcon.sprite = data.icon;
        itemNameText.text = data.GetItemName();
        purchasePriceText.text = LocalizationHelper.L("ITEM_INFO_PRICE", price, CustomerPaymentSystem.CURRENCY_SYMBOL);
        itemWeightText.text = LocalizationHelper.L("ITEM_INFO_WEIGHT", data.weight);
        itemDescriptionText.text = data.GetDescription();
        
        if (stockAmount == -1)
            itemStockAmountText.text = LocalizationHelper.L("ITEM_INFO_ITEM_UNLIMITED");
        else
            itemStockAmountText.text = LocalizationHelper.L("ITEM_INFO_STOCK", stockAmount);
    }
    public void Close()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }

    public void OnPurchaseButtonClicked()
    {
        onPurchased?.Invoke();
    }

}