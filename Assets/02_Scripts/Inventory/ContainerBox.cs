using System.Collections.Generic;
using UnityEngine;

public class ContainerBox : InventoryHolder, IInteractable, ISaveable
{
    #region Interaction (IInteractable)

    public string GetInteractPrompt() => LocalizationHelper.L("PROMPT_OPEN_STORAGE");

    public void OnInteract()
    {
        StorageUIManager.Instance.OpenStorage(this, InventoryContext.Container);
    }
    #endregion

    #region Lifecycle & Initialization

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        LoadContainerBoxFromManager();
    }

    public void LoadContainerBoxFromManager()
    {
        if (GameSaveManager.Instance == null) return;

        var data = GameSaveManager.Instance.LoadContainerBox();

        // 새로 만들기
        inventorySystem = new InventorySystem(data.size);

        // 데이터 채우기
        var savedSlots = data.slots;
        for (int i = 0; i < inventorySystem.Slots.Count; i++)
        {
            if (i < savedSlots.Count)
            {
                inventorySystem.Slots[i].UpdateSlot(savedSlots[i].ItemData, savedSlots[i].Amount);
            }
        }
    }

    #endregion

    public void SaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveContainerBox(InventorySystem.Slots);
        }
    }


}