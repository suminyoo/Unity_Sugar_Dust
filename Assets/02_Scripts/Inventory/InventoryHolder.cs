using System.Collections.Generic;
using UnityEngine;

public class InventoryHolder : MonoBehaviour
{
    [Header("Settings")]
    private int inventorySize;
    public Transform itemDropPosition; // 버릴 위치

    //구현하는 자식만 건들 수 있게
    [SerializeField] protected InventorySystem inventorySystem;
    public InventorySystem InventorySystem => inventorySystem;

    #region Unity lifecycle

    protected virtual void Awake()
    {
        inventorySystem = new InventorySystem(inventorySize);
    }

    #endregion

    #region 인벤토리 아이템 관리

    // 공통 기능: UI 결과창이나 리스트 보여주기용 (상점, 상자, 플레이어 공통)
    public List<InventorySlot> GetOccupiedSlots()
    {
        List<InventorySlot> occupied = new List<InventorySlot>();
        foreach (var slot in inventorySystem.Slots) // IReadOnlyList 사용
        {
            if (!slot.IsEmpty) occupied.Add(slot);
        }
        return occupied;
    }

    // 아이템 넣기
    public virtual int AddItem(ItemData item, int count)   
    {
        return inventorySystem.AddItemToSlots(item, count);
    }

    // 아이템 빼기 : 바닥에 버리기, 다른 인벤토리로 옮기기 등 
    public virtual void DropItemAtIndex(int index, int count)
    {
        var slot = inventorySystem.Slots[index];
        if (slot.IsEmpty || slot.Amount < count) return;

        // 프리팹 생성
        if (slot.ItemData.dropPrefab != null)
        {
            GameObject droppedObj = Instantiate(slot.ItemData.dropPrefab, itemDropPosition.position, Quaternion.identity);

            // 바닥에 떨어진 아이템에 개수 전달
            var worldItem = droppedObj.GetComponent<WorldItem>();
            if (worldItem != null) worldItem.Initialize(slot.ItemData, count);
        }

        // 인벤토리 데이터 삭제
        inventorySystem.RemoveItemAtIndex(index, count);
    }

    // 상점, 조합대, 퀘스트 제출 등 인벤토리에서 아이템 자동 소모? 할때 (아직 미사용 중)
    public virtual void ConsumeItem(ItemData item, int count)
    {
        // 아이템 개수 확인
        int currentCount = inventorySystem.GetItemCount(item);

        if (currentCount >= count)
        {
            // 소모
            inventorySystem.ConsumeItem(item, count);
        }
    }

    //아이템 transfer : 다른 인벤토리로 옮기기
    public virtual void TransferTo(int fromIndex, InventoryHolder toHolder)
    {
        // 내 슬롯 데이터 가져오기
        InventorySlot fromSlot = inventorySystem.Slots[fromIndex];

        if (fromSlot.IsEmpty) return; // 빈칸 패스

        // 받는 쪽에 넣기 시도 
        // 보내려는 아이템과 개수
        ItemData itemToSend = fromSlot.ItemData;
        int amountToSend = fromSlot.Amount;

        int remaining = toHolder.AddItem(itemToSend, amountToSend);

        // 받는 인벤토리 시스템
        InventorySystem toSystem = toHolder.InventorySystem;

        if (remaining == 0) // 전체 성공
        {
            inventorySystem.RemoveItemAtIndex(fromIndex, amountToSend);
        }
        else if (0 < remaining)
        {
            int actualSent = amountToSend - remaining;
            inventorySystem.RemoveItemAtIndex(fromIndex, actualSent);
        }

    }

    #endregion
    public virtual bool CanAcceptItem(int slotIndex, ItemData item)
    {
        return true;
    }

}