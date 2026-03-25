using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics;

public class ItemUsageInfoUI : MonoBehaviour
{    
    [Header("UI")]
    public GameObject itemInfoPanel;
    public GameObject defaultPanel;
    public GameObject useButton;
    private ItemData currentData;

    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI basePriceText;

    private InventoryContext currentContext;

    private void Start()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }

    public void OpenPanel(ItemData data, InventoryContext context)
    {
        currentData = data;
        currentContext = context;

        defaultPanel.SetActive(false);
        itemInfoPanel.SetActive(true);

        icon.sprite = data.icon;
        nameText.text = data.GetItemName();
        typeText.text = LocalizationHelper.Main("ITEM_INFO_TYPE", data.GetItemTypeName());
        weightText.text = LocalizationHelper.Main("ITEM_INFO_WEIGHT", data.weight);
        basePriceText.text = LocalizationHelper.Main("ITEM_INFO_BASE_PRICE", data.basePrice, CustomerPaymentSystem.CURRENCY_SYMBOL);
        infoText.text = data.GetDescription();

        if (useButton != null)
        {
            useButton.gameObject.SetActive(data.IsUsable() || data is ToolData);

            var btnText = useButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                if (currentContext == InventoryContext.Equipment)
                {
                    btnText.text = LocalizationHelper.Main("INVEN_UNEQUIP");
                }
                else
                {
                    btnText.text = LocalizationHelper.Main((data is ToolData) ? "INVEN_EQUIP" : "INVEN_USE");
                }
            }
        }
    }
    public void OnClickUseButton()
    {
        if (currentContext == InventoryContext.Equipment && currentData is ToolData unequipTool)
        {
            PlayerEquipment.Instance.UnequipTool(unequipTool);
            Close();
        }
        else if (currentData is ToolData tool)
        {
            PlayerEquipment.Instance.EquipFromBag(tool);
            Close();
        }
        else if (currentData != null)
        {
            PlayerInventory.Instance.UseItem(currentData);
            Close();
        }
    }

    public void Close()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }

}
