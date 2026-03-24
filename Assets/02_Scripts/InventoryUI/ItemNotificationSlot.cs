using UnityEngine;
using TMPro;

public class ItemNotificationSlot : MonoBehaviour
{
    public TextMeshProUGUI itemText;

    public void SetData(ItemData data, int amount)
    {
        itemText.text = $"{data.GetItemName()} + {amount}";

        Invoke("DestroySelf", 3f);
    }

    private void DestroySelf()
    {
        if (ItemNotificationManager.Instance != null)
        {
            ItemNotificationManager.Instance.RemoveFromList(gameObject);
        }
        Destroy(gameObject);
    }
}