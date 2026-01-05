using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    public InventoryUI inventoryUI; // MouseItemData에 접근을 위해

    public void OnDrop(PointerEventData eventData)
    {
        // 마우스에 아이템이 없으면 무시
        if (inventoryUI.mouseItemData == null || !inventoryUI.mouseItemData.HasItem) return;

        InventorySlot mouseSlot = inventoryUI.mouseItemData.mouseSlot;

        if (mouseSlot.itemData.dropPrefab != null)
        {
            // 플레이어 위치
            Transform playerTransform = inventoryUI.connectedInventory.transform;
            Vector3 dropPos = playerTransform.position + playerTransform.forward * 1.5f;

            GameObject droppedObj = Instantiate(mouseSlot.itemData.dropPrefab, dropPos, Quaternion.identity);

            // 개수 전달
            var worldItem = droppedObj.GetComponent<WorldItem>();
            if (worldItem != null) worldItem.Initialize(mouseSlot.itemData, mouseSlot.amount);
        }

        // 마우스 비우기
        inventoryUI.mouseItemData.ClearSlot();
    }
}