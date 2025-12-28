using UnityEngine;

//플레이어 인벤토리, 인벤홀더 상속
public class PlayerInventory : InventoryHolder
{
    [Header("Weight Stats")]
    public float maxWeight = 100f;
    public float currentWeight = 0f;

    // 아이템 얻을때 무게 계산 로직
    public override bool AddItem(ItemData item, int count)
    {
        float extraCapacity = maxWeight * (0.8f / Mathf.Log(maxWeight + 1, 10));
        float limit = maxWeight + extraCapacity;

        // 무게 체크
        if (currentWeight + (item.weight * count) > limit)
        {
            Debug.Log($"너무 무거움! (현재: {currentWeight}, 한계: {limit})");
            return false;
        }

         if (base.AddItem(item, count))
        {
            currentWeight += item.weight * count;
            return true;
        }

        Debug.Log("가방 칸이 꽉 찼습니다.");
        return false;
    }

    // 바닥에 버릴 때 무게 감소
    public override void DropItemAtIndex(int index, int count)
    {
        // 인덱스 안전 검사
        if (index < 0 || index >= inventorySystem.slots.Count) return;

        var slot = inventorySystem.slots[index];

        currentWeight -= slot.itemData.weight * count;

        base.DropItemAtIndex(index, count);
    }


    public override void ConsumeItem(ItemData item, int count)
    {
        // 실제 시스템에 아이템이 충분한지 확인
        int currentCount = inventorySystem.GetItemCount(item);
        if (currentCount >= count)
        {
            currentWeight -= item.weight * count;
            base.ConsumeItem(item, count);
        }
    }
}