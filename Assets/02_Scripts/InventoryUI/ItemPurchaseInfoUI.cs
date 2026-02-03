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
        itemNameText.text = data.itemName;
        purchasePriceText.text = $"가격: {price:N0} {CustomerPaymentSystem.CURRENCY_SYMBOL}";
        itemWeightText.text = $"무게: {data.weight:F1}kg";
        itemDescriptionText.text = data.description;
        if(stockAmount == -1)
            itemStockAmountText.text = "재고: 무제한";
        else
            itemStockAmountText.text = $"재고: {stockAmount}개"; 
    }
    public void Close()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }

    //구매 버튼 클릭
    public void OnPurchaseButtonClicked()
    {
        onPurchased?.Invoke();
    }

}