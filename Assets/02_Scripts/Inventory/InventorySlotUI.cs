using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image itemIcon;
    public TextMeshProUGUI amountText;
    public Button slotButton;

    public void SetSlot(InventorySlot slot)
    {
        if (slot != null)
        {
            itemIcon.sprite = slot.itemData.icon;
            itemIcon.color = new Color(1, 1, 1, 1);

            if (slot.amount > 1)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = slot.amount.ToString();
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
        }
    }

    public void ClearSlot()
    {
        itemIcon.sprite = null;
        itemIcon.color = new Color(1, 1, 1, 0); 
        amountText.gameObject.SetActive(false);
    }
}