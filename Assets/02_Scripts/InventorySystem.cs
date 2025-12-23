using System.Collections.Generic;
using UnityEngine;

// 인벤토리 한 칸을 정의하는 클래스
[System.Serializable]
public class InventorySlot
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
    [Header("Capacity")]
    public float maxWeight = 100f;
    public float currentWeight = 0f;

    [Header("Contents")]
    public List<InventorySlot> slots = new List<InventorySlot>(); // 실제 아이템 리스트

    [Header("References")]
    public Transform dropPosition; // 아이템을 버릴 때 생성될 위치 (플레이어 앞)

    // 아이템 추가 메서드 (WorldItem에서 호출)
    public bool AddItem(ItemData data, int count)
    {
        // 무게 체크 (미리 계산)
        if (currentWeight + (data.weight * count) > maxWeight + 50f) // 50은 초과 허용치
        {
            return false; // 너무 무거워서 못 줍는다 (기획에 따라 삭제 가능)
        }

        // 이미 있는 아이템인지 확인 (스택 가능할 경우)
        if (data.isStackable)
        {
            InventorySlot existingSlot = slots.Find(s => s.itemData == data);
            if (existingSlot != null)
            {
                existingSlot.AddQuantity(count);
                UpdateTotalWeight();
                return true;
            }
        }

        // 새 슬롯 추가
        slots.Add(new InventorySlot(data, count));
        UpdateTotalWeight();
        return true;
    }

    // 아이템 버리기 메서드 (UI에서 호출하거나 단축키로 호출)
    public void DropItem(InventorySlot slot)
    {
        if (slot.itemData.dropPrefab != null)
        {
            // 바닥에 프리팹 생성
            GameObject obj = Instantiate(slot.itemData.dropPrefab, dropPosition.position, Quaternion.identity);

            // 생성된 오브젝트에 데이터 주입
            WorldItem worldItem = obj.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                worldItem.Initialize(slot.itemData, slot.quantity);
            }

            // 물리 효과를 위해 조금 튕겨나가게 하기
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