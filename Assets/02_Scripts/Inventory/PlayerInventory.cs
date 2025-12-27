using UnityEngine;

//플레이어 인벤토리, 인벤홀더 상속
public class PlayerInventory : InventoryHolder
{
    [Header("Weight Stats")]
    public float maxWeight = 100f;
    public float currentWeight = 0f;

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

    public override void DropItem(ItemData item, int count)
    {
        currentWeight -= item.weight * count;

        base.DropItem(item, count);
    }

}