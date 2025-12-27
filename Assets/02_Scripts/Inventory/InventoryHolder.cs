using UnityEngine;

public class InventoryHolder : MonoBehaviour
{
    [Header("Settings")]
    public int inventorySize = 20;
    public Transform dropPosition; // 버릴 위치

    //외부에서는 못건들고 구현하는 자식만 건들 수 있게
    [SerializeField] protected InventorySystem inventorySystem;
    public InventorySystem InventorySystem => inventorySystem;

    protected virtual void Awake()
    {
        inventorySystem = new InventorySystem(inventorySize);
    }

    // 아이템 넣기
    public virtual bool AddItem(ItemData item, int count)   
    {
        return inventorySystem.AddToSlots(item, count);
    }

    // 바닥에 버리기 -> 플레이어, 보물상자
    public virtual void DropItem(ItemData item, int count)
    {
        if (item.dropPrefab != null)
        {
            Vector3 pos = dropPosition != null ? dropPosition.position : transform.position + transform.forward;
            GameObject droppedObj = Instantiate(item.dropPrefab, pos, Quaternion.identity);

            var worldItem = droppedObj.GetComponent<WorldItem>();
            if (worldItem != null) worldItem.Initialize(item, count);
        }

        // 데이터 삭제
        inventorySystem.RemoveItem(item, count);
    }

    // 아이템 Destroy -> 상점, 조합대, 퀘스트 제출 등
    public void DestroyItem(ItemData item, int count)
    {
        inventorySystem.RemoveItem(item, count);
    }

    public void TransferItem(InventoryHolder from, InventoryHolder to, ItemData item, int count)
    {
        // 1. 받는 쪽에 공간이 있는지 먼저 확인하고 넣음
        if (to.AddItem(item, count))
        {
            // 2. 성공했으면 보내는 쪽에서 삭제 (DestroyItem 사용)
            from.DestroyItem(item, count);
        }
        else
        {
            Debug.Log("상대방 인벤토리가 꽉 찼습니다.");
        }
    }
}