using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MouseItemData : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI amountText;
    public InventorySlot mouseSlot;

    public event Action OnMouseItemChanged;

    private void Awake()
    {
        itemSprite.color = Color.clear;
        itemSprite.raycastTarget = false; //마우스 클릭 방해 금지
        amountText.text = "";
    }

    public void UpdateMouseSlot(InventorySlot slot)
    {
        mouseSlot.UpdateSlot(slot.itemData, slot.amount); // 데이터 복사
        itemSprite.sprite = slot.itemData.icon;
        itemSprite.color = Color.white;
        amountText.text = slot.amount > 1 ? slot.amount.ToString() : "";

        OnMouseItemChanged?.Invoke();
    }

    public void ClearSlot()
    {
        mouseSlot.Clear();
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
        amountText.text = "";

        OnMouseItemChanged?.Invoke();
    }

    //마우스 아이템 무게
    public float GetMouseItemWeight()
    {
        if (HasItem) return mouseSlot.itemData.weight * mouseSlot.amount;
        return 0f;
    }

    public bool HasItem => mouseSlot != null && mouseSlot.itemData != null;

    void Update()
    {
        if (HasItem) transform.position = Input.mousePosition;
    }
}