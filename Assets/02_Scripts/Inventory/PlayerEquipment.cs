//using UnityEngine;

//public class PlayerEquipment : InventoryHolder
//{
//    public ActionSystem actionSystem;
//    public InventoryUI equipmentUI;

//    protected override void Awake()
//    {
//        inventorySystem = new InventorySystem(2);
//    }

//    private void Start()
//    {
//        if (equipmentUI != null)
//        {
//            equipmentUI.SetInventorySystem(this.inventorySystem);
//        }

//        inventorySystem.OnInventoryUpdated += UpdateActionSystem;

//        UpdateActionSystem();
//    }

//    private void OnDestroy()
//    {
//        if (inventorySystem != null)
//        {
//            inventorySystem.OnInventoryUpdated -= UpdateActionSystem;
//        }
//    }

//    public override int AddItem(ItemData item, int count)
//    {
//        if (!(item is ToolData tool))
//        {
//            return count;
//        }

//        if (tool.toolActionType == ActionType.Attack)
//        {
//            if (!inventorySystem.Slots[0].IsEmpty) return count;

//            inventorySystem.Slots[0].UpdateSlot(item, count);
//            UpdateActionSystem();
//            return 0;
//        }

//        if (tool.toolActionType == ActionType.Mine)
//        {
//            if (!inventorySystem.Slots[1].IsEmpty) return count;

//            inventorySystem.Slots[1].UpdateSlot(item, count);
//            UpdateActionSystem();
//            return 0;
//        }

//        return count;
//    }

//    private void UpdateActionSystem()
//    {
//        ToolData swordData = inventorySystem.Slots[0].IsEmpty
//            ? null
//            : inventorySystem.Slots[0].ItemData as ToolData;

//        ToolData pickaxeData = inventorySystem.Slots[1].IsEmpty
//            ? null
//            : inventorySystem.Slots[1].ItemData as ToolData;

//        if (actionSystem != null)
//        {
//            actionSystem.UpdateEquippedWeapons(swordData, pickaxeData);
//        }
//    }
//}