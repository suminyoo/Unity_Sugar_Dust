using System.Collections.Generic;
using UnityEngine.Events;

//인벤 한칸
[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int amount;

    public InventorySlot(ItemData item, int count)
    {
        itemData = item;
        amount = count;
    }

    public void AddAmount(int value) => amount += value;
    public void RemoveAmount(int value) => amount -= value;
}

//==========================================================

//인벤토리 시스템: 인벤토리 아이템을 더하거나 빼며 리스트로 관리
// 저장에 용이, 인벤토리뿐 아니라 상점과 판매대에서 사용 가능
[System.Serializable]
public class InventorySystem
{
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int maxSlots; // 최대 칸

    public UnityAction OnInventoryUpdated;

    public InventorySystem(int size)
    {
        maxSlots = size;
    }

    // ADD: 이미 있으면 갯수 더하기, 없으면 슬롯 리스트에 새로 추가
    public bool AddToSlots(ItemData item, int count)
    {
        // 있는 경우
        if (item.isStackable)
        {
            InventorySlot existingSlot = slots.Find(s => s.itemData == item && s.amount < item.maxStackAmount);
            if (existingSlot != null)
            {
                existingSlot.AddAmount(count);
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // 없는 경우
        if (slots.Count < maxSlots)
        {
            slots.Add(new InventorySlot(item, count));
            OnInventoryUpdated?.Invoke();
            return true;
        }

        return false;
    }

    // REMOVE: 이미 있으면 갯수 빼기, 0개 이하면 슬롯 리스트에서 없애기
    public void RemoveItem(ItemData item, int count)
    {
        InventorySlot existingSlot = slots.Find(s => s.itemData == item);

        if (existingSlot != null)
        {
            existingSlot.RemoveAmount(count);

            if (existingSlot.amount <= 0)
            {
                slots.Remove(existingSlot);
            }

            OnInventoryUpdated?.Invoke();
        }
    }
}