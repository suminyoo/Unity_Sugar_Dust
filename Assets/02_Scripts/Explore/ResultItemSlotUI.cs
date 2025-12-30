using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultItemSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI amountText;

    public void SetData(ItemData data, int amount)
    {
        if (data == null) return;

        iconImage.sprite = data.icon;
        amountText.text = amount.ToString();
    }
}
