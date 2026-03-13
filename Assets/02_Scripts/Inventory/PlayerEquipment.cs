using UnityEngine;

public class PlayerEquipment : InventoryHolder
{
    public static PlayerEquipment Instance;

    public ActionSystem actionSystem;

    public ToolData bareHandSword;
    public ToolData bareHandPickaxe;

    protected override void Awake()
    {
        inventorySystem = new InventorySystem(2);

        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        inventorySystem.OnInventoryUpdated += UpdateActionSystem;
        UpdateActionSystem();
    }

    public void EquipFromBag(ToolData newTool)
    {
        int targetSlot = (newTool.toolActionType == ActionType.Attack) ? 0 : 1;
        var bag = PlayerInventory.Instance;

        InventorySlot currentSlot = inventorySystem.Slots[targetSlot];
        ItemData oldItem = currentSlot.ItemData;

        bag.InventorySystem.ConsumeItem(newTool, 1);

        currentSlot.UpdateSlot(newTool, 1);

        if (oldItem != null && oldItem != bareHandSword && oldItem != bareHandPickaxe)
        {
            bag.AddItem(oldItem, 1);
        }

        //inventorySystem.NotifyUpdated();
    }

    public override int AddItem(ItemData item, int count)
    {
        if (item is ToolData tool)
        {
            int targetSlot = (tool.toolActionType == ActionType.Attack) ? 0 : 1;
            return base.AddItem(item, count);
        }
        return count;
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
}