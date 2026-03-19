using UnityEngine;

public class PlayerEquipment : InventoryHolder, ISaveable
{
    public static PlayerEquipment Instance;
    public ActionSystem actionSystem;
    public InventoryUI equipmentUI;
    public ToolData bareHandSword;
    public ToolData bareHandPickaxe;

    protected override void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        base.Awake();
    }

    private void Start()
    {
        // ui 연결 먼저
        if (equipmentUI != null)
        {
            equipmentUI.connectedInventory = this;
        }
        // 데이터 로드
        LoadEquipmentFromManager();
        RefreshUI();

        inventorySystem.OnInventoryUpdated += UpdateActionSystem;
        inventorySystem.OnInventoryUpdated += PlayerInventory.Instance.RefreshTotalWeight;

        UpdateActionSystem();
    }

    // 우클릭 장착
    public void EquipFromBag(ToolData newTool)
    {
        int targetSlot = (newTool.toolActionType == ActionType.Attack) ? 0 : 1;
        var bag = PlayerInventory.Instance;

        ItemData oldItem = inventorySystem.Slots[targetSlot].ItemData;

        bag.InventorySystem.ConsumeItem(newTool, 1);

        inventorySystem.UpdateSlotAtIndex(targetSlot, newTool, 1);

        if (oldItem != null && oldItem != bareHandSword && oldItem != bareHandPickaxe)
        {
            bag.AddItem(oldItem, 1);
        }
    }

    public override int AddItem(ItemData item, int count)
    {
        if (item is ToolData tool)
        {
            int targetSlot = (tool.toolActionType == ActionType.Attack) ? 0 : 1;

            // base.AddItem을 쓰지 않고 정해진 슬롯이 비었을 때만 시스템을 통해 직접 할당
            if (inventorySystem.Slots[targetSlot].IsEmpty)
            {
                inventorySystem.UpdateSlotAtIndex(targetSlot, item, count);
                return 0;
            }
        }
        return count;
    }
    public void RefreshUI()
    {
        if (equipmentUI != null)
        {
            equipmentUI.SetInventorySystem(this.inventorySystem);
        }
    }

    // 드래그 앤 드롭 가능한지 위치 확인
    public override bool CanAcceptItem(int slotIndex, ItemData item)
    {
        if (!(item is ToolData tool)) return false;
        int targetSlot = (tool.toolActionType == ActionType.Attack) ? 0 : 1;

        return slotIndex == targetSlot;
    }

    private void UpdateActionSystem()
    {
        ToolData sword = inventorySystem.Slots[0].IsEmpty ? bareHandSword : (ToolData)inventorySystem.Slots[0].ItemData;
        ToolData pickaxe = inventorySystem.Slots[1].IsEmpty ? bareHandPickaxe : (ToolData)inventorySystem.Slots[1].ItemData;

        if (actionSystem != null)
        {
            actionSystem.UpdateEquippedWeapons(sword, pickaxe);
        }
    }

    public float GetEquipmentWeight()
    {
        float weight = 0f;
        foreach (var slot in inventorySystem.Slots)
        {
            if (!slot.IsEmpty)
            {
                weight += slot.ItemData.weight * slot.Amount;
            }
        }
        return weight;
    }

    public void UnequipTool(ToolData tool)
    {
        int targetSlot = (tool.toolActionType == ActionType.Attack) ? 0 : 1;

        int remaining = PlayerInventory.Instance.AddItem(tool, 1);

        if (remaining == 0)
        {
            inventorySystem.UpdateSlotAtIndex(targetSlot, null, 0);
        }
        else
        {
            NotificationUIManager.Instance.ShowNotification("가방이 꽉 차서 장착을 해제할 수 없습니다.");
        }
    }

    public void SaveData()
    {
        GameSaveManager.Instance.SavePlayerEquipment(InventorySystem.Slots);
    }


    public void LoadEquipmentFromManager()
    {
        if (GameSaveManager.Instance == null) return;

        var data = GameSaveManager.Instance.LoadPlayerEquipment();

        inventorySystem = new InventorySystem(data.size);

        var savedSlots = data.slots;
        for (int i = 0; i < inventorySystem.Slots.Count; i++)
        {
            if (i < savedSlots.Count)
                inventorySystem.Slots[i].UpdateSlot(savedSlots[i].ItemData, savedSlots[i].Amount);
        }

        // ui에게 재연결
        if (equipmentUI != null)
        {
            equipmentUI.connectedInventory = this;

            equipmentUI.SetInventorySystem(this.inventorySystem);
        }

        UpdateActionSystem();
    }

}