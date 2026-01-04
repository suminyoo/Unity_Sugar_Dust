using UnityEngine;

public class InventoryHolder : MonoBehaviour
{
    [Header("Settings")]
    private int inventorySize;
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

    //아이템 transfer 로직 변화에 맞게 수정
    public virtual void TransferTo(int fromIndex, InventoryHolder toHolder)
    {
        // 내 슬롯 데이터 가져오기
        InventorySlot fromSlot = inventorySystem.slots[fromIndex];

        if (fromSlot.IsEmpty) return; // 빈칸 패스

        // 받는 쪽에 넣기 시도 
        // 보내려는 아이템과 개수
        ItemData itemToSend = fromSlot.itemData;
        int amountToSend = fromSlot.amount;

        // 받는 인벤토리 시스템
        InventorySystem toSystem = toHolder.InventorySystem;

        // 상대방 인벤토리에 들어갈 수 있는 잔여 공간 계산이 필요하지만, 
        // 우선 성공하면 차감 방식

        // 상대방에게 성공적으로 다 들어간 겨경우
        if (toSystem.AddItemToSlots(itemToSend, amountToSend))
        {
            // 내 인벤토리에서 해당 개수제거
            inventorySystem.RemoveItemAtIndex(fromIndex, amountToSend);
        }
        else
        {
            // 실패 (꽉 참? 등ㄷ응)
            // TODO: 일부만 들어가는 로직은 AddItemToSlots가 남은 개수를 반환해ㅑ야함(현재 bool값만 리턴)
            Debug.Log("상대방 공간이 부족합니다!");
        }
    }
}