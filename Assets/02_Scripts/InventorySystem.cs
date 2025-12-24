using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot //인벤토리 한칸
{
    public ItemData itemData;
    public int quantity;

    public InventorySlot(ItemData data, int qty)
    {
        itemData = data;
        quantity = qty;
    }

    public void AddQuantity(int count) => quantity += count;
}

public class InventorySystem : MonoBehaviour
{
    public float maxWeight = 100f;
    public float extremeMaxWeight;

    public float currentWeight = 0f;

    public List<InventorySlot> slots = new List<InventorySlot>();

    public Transform dropPosition;


    private void Start()
    {
    }

    //아이템 먹음
    public bool AddItem(ItemData item, int count)
    {
        // TODO: 업그레이드 항목으로 옮기기
        float extraCapacity = maxWeight * (0.8f / Mathf.Log(maxWeight + 1, 10));
        extremeMaxWeight = maxWeight + extraCapacity;

        // 무게 체크
        if (currentWeight + (item.weight * count) > extremeMaxWeight)
        {
            Debug.Log("너무 무거워서 못 줍는다");
            return false;
        }

        // 이미 있는 아이템인지 확인 (스택 가능할 경우)
        if (item.isStackable)
        {
            InventorySlot existingSlot = slots.Find(s => s.itemData == item);
            if (existingSlot != null)
            {
                existingSlot.AddQuantity(count);
                UpdateTotalWeight();
                return true;
            }
        }

        // 새 슬롯 추가
        slots.Add(new InventorySlot(item, count));
        UpdateTotalWeight();
        return true;
    }

    // 아이템 버리기
    public void DropItem(InventorySlot slot)
    {
        if (slot.itemData.dropPrefab != null)
        {
            GameObject obj = Instantiate(slot.itemData.dropPrefab, dropPosition.position, Quaternion.identity);

            // 생성된 오브젝트에 데이터
            WorldItem worldItem = obj.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                worldItem.Initialize(slot.itemData, slot.quantity);
            }

            // 물리 효과
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * 2f + Vector3.up * 2f, ForceMode.Impulse);
            }
        }

        // 인벤토리에서 제거
        currentWeight -= slot.itemData.weight * slot.quantity;
        slots.Remove(slot);

        Debug.Log($"{slot.itemData.itemName}을(를) 바닥에 버렸습니다.");
    }

    // 총 무게 재계산
    void UpdateTotalWeight()
    {
        currentWeight = 0f;
        foreach (var slot in slots)
        {
            currentWeight += slot.itemData.weight * slot.quantity;
        }
    }
}