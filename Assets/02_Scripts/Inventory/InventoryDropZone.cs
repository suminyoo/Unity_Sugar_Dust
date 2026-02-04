using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    public InventoryUI inventoryUI; // MouseItemData에 접근을 위해

    public void OnDrop(PointerEventData eventData)
    {
        // 마우스에 아이템이 없으면 무시
        if (inventoryUI.mouseItemData == null || !inventoryUI.mouseItemData.HasItem) return;
        inventoryUI.mouseItemData.DropItemAndClear();
    }
}