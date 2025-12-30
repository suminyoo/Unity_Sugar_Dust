using UnityEngine;

public class InventoryHolder : MonoBehaviour
{
    [Header("Settings")]
    public int inventorySize = 9;
    public Transform dropPosition; // 버릴 위치

    //외부에서는 못건들고 구현하는 자식만 건들 수 있게
    [SerializeField] protected InventorySystem inventorySystem;
    public InventorySystem InventorySystem => inventorySystem; //얘는 public

    protected virtual void Awake()
    {
        inventorySystem = new InventorySystem(inventorySize);
    }

    // 아이템 넣기
    public virtual bool AddItem(ItemData item, int count)   
    {
        return inventorySystem.AddItemToSlots(item, count);
    }

    // 바닥에 버리기: 플레이어, 보물상자
    public virtual void DropItemAtIndex(int index, int count)
    {
        var slot = inventorySystem.slots[index];
        if (slot.IsEmpty || slot.amount < count) return;

        // 프리팹 생성
        if (slot.itemData.dropPrefab != null)
        {
            Vector3 pos = dropPosition != null ? dropPosition.position : transform.position + transform.forward;
            GameObject droppedObj = Instantiate(slot.itemData.dropPrefab, pos, Quaternion.identity);

            // 바닥에 떨어진 아이템에 개수 전달
            var worldItem = droppedObj.GetComponent<WorldItem>();
            if (worldItem != null) worldItem.Initialize(slot.itemData, count);
        }

        // 인벤토리 데이터 삭제
        inventorySystem.RemoveItemAtIndex(index, count);
    }

    // 상점, 조합대, 퀘스트 제출 등 인벤토리에서 아이템 자동 소모? 할때
    public virtual void ConsumeItem(ItemData item, int count)
    {
        // 아이템 개수 확인
        int currentCount = inventorySystem.GetItemCount(item);

        if (currentCount >= count)
        {
            //  소모
            inventorySystem.ConsumeItem(item, count);
            Debug.Log($"{item.name} {count}개 소모 완료");
        }
        else
        {
            Debug.Log($"아이템이 부족합니다! (보유: {currentCount}, 필요: {count})");
        }
    }

    public virtual void TransferSlot(int index, InventoryHolder to)
    {
        var slot = inventorySystem.slots[index];
        if (slot.IsEmpty) return;

        // 넣기 시도
        // TODO: (전체를 다 넣을 수도 있고 공간 부족으로 일부만 들어갈 수도 있음 - 구현 고려 필요)
        // 일단 임시 (전부 들어가거 안 들어가거나)
        if (to.AddItem(slot.itemData, slot.amount))
        {
            // 성공시 해당 칸 비우기
            inventorySystem.RemoveItemAtIndex(index, slot.amount);
        }
        else
        {
            Debug.Log("상대방 가방이 꽉 찼거나 넣을 수 없습니다.");
        }
    }
}