using System.Collections.Generic;
using TMPro;
using UnityEngine;

//플레이어 인벤토리, 인벤홀더 상속
public class PlayerInventory : InventoryHolder, ISaveable
{
    #region Variables
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

    #endregion

    #region Lifecycle & Initialization

    private void Start()
    {
        // 인스펙터 연결이 끊겨 있으면 씬에 존재하는 인벤토리 직접 연결
        if (inventoryUI == null)
        {
            InventoryUI[] allUIs = FindObjectsOfType<InventoryUI>(true);
            foreach (var ui in allUIs)
            {
                if (ui.contextType == InventoryContext.Player)
                {
                    inventoryUI = ui;
                    break;
                }
            }
        }

        if (GameSaveManager.Instance != null)
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
        if (GameSaveManager.Instance == null) return;

        var data = GameSaveManager.Instance.LoadPlayerInventory();

        inventorySystem = new InventorySystem(data.size);

        // 데이터 채우기
        var savedSlots = data.slots;
        for (int i = 0; i < inventorySystem.Slots.Count; i++)
        {
            if (i < savedSlots.Count)
                inventorySystem.Slots[i].UpdateSlot(savedSlots[i].ItemData, savedSlots[i].Amount);
        }

        // ui에게 재연결
        if (inventoryUI != null)
        {
            inventoryUI.SetInventorySystem(this.inventorySystem);
        }

        UpdateWeightUI();
        Debug.Log("인벤토리 로드 및 UI 재연결 완료");
    }

    #endregion

    #region Weight System (Calculation & UI)
    public void RefreshTotalWeight()
    {
        float totalWeight = 0f;
        foreach (var slot in inventorySystem.Slots)
        {
            if (!slot.IsEmpty)
            {
                float slotWeight = slot.ItemData.weight * slot.Amount;
                totalWeight += slotWeight;
                //Debug.Log($"슬롯: {slot.itemData.itemName} x {slot.amount} = {slotWeight:F1}kg");
            }
        }
        if(mouseItemData.HasItem)
        {
            float mouseWeight = mouseItemData.GetMouseItemWeight();
            totalWeight += mouseWeight;
            //Debug.Log($"마우스 아이템 무게 추가: {mouseWeight:F1}kg");
        }

        currentWeight = totalWeight;
        UpdateWeightUI();
        //Debug.Log($"총 무게 업데이트: {currentWeight:F1}kg / {maxWeight:F1}kg");
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
    #endregion

    #region Inventory Holder Overrides

    // 아이템 얻을때 무게 계산 로직
    public override int AddItem(ItemData item, int count)
    {
        float extraCapacity = maxWeight * (0.8f / Mathf.Log(maxWeight + 1, 10));
        float limit = maxWeight + extraCapacity;

        int remaining = inventorySystem.AddItemToSlots(item, count);

        if (remaining == count)
        {
            NotificationUIManager.Instance.ShowNotification("인벤토리 공간이 부족합니다.");
            return -1;
        }

        return remaining;
    }

    // 바닥에 버릴 때 무게 감소
    public override void DropItemAtIndex(int index, int count)
    {
        // 인덱스 검사
        if (index < 0 || index >= inventorySystem.Slots.Count) return;

        base.DropItemAtIndex(index, count);
    }


    public override void ConsumeItem(ItemData item, int count)
    {
        bool success = inventorySystem.ConsumeItem(item, count);

        if (!success)
        {
            NotificationUIManager.Instance.ShowNotification($"{item.itemName}이(가) 부족합니다.");
        }
    }
    #endregion

    public void SaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SavePlayerInventory(InventorySystem.Slots);
        }
    }

    #region Exploration Penalty

    // 모든 아이템 삭제 (탐사 실패 시)
    public void ClearAllInventory()
    {
        for (int i = 0; i < inventorySystem.MaxSlots; i++)
        {
            if (!inventorySystem.Slots[i].IsEmpty)
            {
                inventorySystem.RemoveItemAtIndex(i, inventorySystem.Slots[i].Amount);
            }
        }
    }

    // 무작위 아이템 분실 (탐사 완료 후 걸어서 귀환 시)
    public List<InventorySlot> LoseRandomItems(int minLoss, int maxLoss)
    {
        List<InventorySlot> lostItems = new List<InventorySlot>();
        List<int> occupiedIndices = new List<int>();
        for (int i = 0; i < inventorySystem.Slots.Count; i++)
        {
            if (!inventorySystem.Slots[i].IsEmpty) occupiedIndices.Add(i);
        }

        if (occupiedIndices.Count == 0) return lostItems;

        int loseCount = Mathf.Min(Random.Range(minLoss, maxLoss + 1), occupiedIndices.Count);

        for (int i = 0; i < loseCount; i++)
        {
            int randomIndex = Random.Range(0, occupiedIndices.Count);
            int targetSlotIndex = occupiedIndices[randomIndex];

            var targetSlot = inventorySystem.Slots[targetSlotIndex];
            lostItems.Add(new InventorySlot(targetSlot.ItemData, targetSlot.Amount));

            // 수량 전량 삭제
            inventorySystem.RemoveItemAtIndex(targetSlotIndex, inventorySystem.Slots[targetSlotIndex].Amount);
            occupiedIndices.RemoveAt(randomIndex);
        }
        return lostItems;
    }

    #endregion
}