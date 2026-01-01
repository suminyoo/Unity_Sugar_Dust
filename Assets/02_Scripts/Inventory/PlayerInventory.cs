using TMPro;
using UnityEngine;

//플레이어 인벤토리, 인벤홀더 상속
public class PlayerInventory : InventoryHolder
{
    public PlayerData playerData; // SO 연결 필요
    public MouseItemData mouseItemData;
    public InventoryUI inventoryUI;

    [Header("Weight")]
    public float maxWeight = 100f; //TODO: 가방 종류 혹은 플레이어 능력에 따른 무게 한계치
    public float currentWeight = 0f;
    public TextMeshProUGUI weightText;

    [Header("Text Colors")]
    public Color normalColor = Color.white;
    public Color warningColor = new Color(1f, 0.6f, 0f); // 주황
    public Color exceedColor = Color.red;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            LoadInventoryFromManager();
        }

        inventorySystem.OnInventoryUpdated += RefreshTotalWeight;
        mouseItemData.OnMouseItemChanged += RefreshTotalWeight;
    }
    private void OnDestroy()
    {
        inventorySystem.OnInventoryUpdated -= RefreshTotalWeight;
        mouseItemData.OnMouseItemChanged -= RefreshTotalWeight;
    }

    private void LoadInventoryFromManager()
    {
        if (GameManager.Instance == null) return;

        GameData data = GameManager.Instance.LoadPlayerState();

        // 새로 만들기
        int size = playerData.GetInventorySize(data.inventoryLevel);
        inventorySystem = new InventorySystem(size);

        // 데이터 채우기
        var savedSlots = data.inventorySlots;
        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            if (i < savedSlots.Count)
                inventorySystem.slots[i].UpdateSlot(savedSlots[i].itemData, savedSlots[i].amount);
        }

        // ui에게 재연결
        if (inventoryUI != null)
        {
            inventoryUI.SetInventorySystem(this.inventorySystem);
        }

        UpdateWeightUI();
        Debug.Log("인벤토리 로드 및 UI 재연결 완료");
    }

    public void RefreshTotalWeight()
    {
        float totalWeight = 0f;
        foreach (var slot in inventorySystem.slots)
        {
            if (!slot.IsEmpty)
            {
                float slotWeight = slot.itemData.weight * slot.amount;
                totalWeight += slotWeight;
                Debug.Log($"슬롯: {slot.itemData.itemName} x {slot.amount} = {slotWeight:F1}kg");
            }
        }
        if(mouseItemData.HasItem)
        {
            float mouseWeight = mouseItemData.GetMouseItemWeight();
            totalWeight += mouseWeight;
            Debug.Log($"마우스 아이템 무게 추가: {mouseWeight:F1}kg");
        }

        currentWeight = totalWeight;
        UpdateWeightUI();
        Debug.Log($"총 무게 업데이트: {currentWeight:F1}kg / {maxWeight:F1}kg");
    }

    public void UpdateWeightUI()
    {
        if (weightText == null) return;

        weightText.text = $"{currentWeight:F1}kg / {maxWeight:F1}kg";

        float weightRatio = currentWeight / maxWeight;

        if (weightRatio >= 1f) weightText.color = exceedColor;
        else if (weightRatio >= 0.8f) weightText.color = warningColor;
        else weightText.color = normalColor;
    }

    // 아이템 얻을때 무게 계산 로직
    public override bool AddItem(ItemData item, int count)
    {
        float extraCapacity = maxWeight * (0.8f / Mathf.Log(maxWeight + 1, 10));
        float limit = maxWeight + extraCapacity;

        // 무게 체크
        if (currentWeight + (item.weight * count) > limit)
        {
            Debug.Log($"무게 초과로 습득 불가 (현재: {currentWeight}, 한계: {limit})");
            return false;
        }

        return base.AddItem(item, count);
    }

    // 바닥에 버릴 때 무게 감소
    public override void DropItemAtIndex(int index, int count)
    {
        // 인덱스 안전 검사
        if (index < 0 || index >= inventorySystem.slots.Count) return;

        base.DropItemAtIndex(index, count);
    }


    public override void ConsumeItem(ItemData item, int count)
    {
        // 실제 시스템에 아이템이 충분한지 확인
        int currentCount = inventorySystem.GetItemCount(item);
        if (currentCount >= count)
        {
            base.ConsumeItem(item, count);
            Debug.Log($"{item.itemName} {count}개 소비 완료");
        }
        else
        {
            Debug.LogWarning("소비할 아이템 수량이 부족합니다.");
        }

    }
}