using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseItemData : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI amountText;
    public InventorySlot mouseSlot;

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
    }

    public void ClearSlot()
    {
        mouseSlot.Clear();
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
        amountText.text = "";
    }

    public bool HasItem => mouseSlot != null && mouseSlot.itemData != null;

    void Update()
    {
        if (HasItem) transform.position = Input.mousePosition;
    }
}