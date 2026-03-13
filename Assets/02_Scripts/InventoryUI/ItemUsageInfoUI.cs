using UnityEngine;
using TMPro;
using UnityEngine.UI;

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


    private void Start()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }

    public void OpenPanel(ItemData data)
    {
        currentData = data;

        defaultPanel.SetActive(false);
        itemInfoPanel.SetActive(true);

        icon.sprite = data.icon;
        nameText.text = data.GetItemName();
        typeText.text = LocalizationHelper.Main("ITEM_INFO_TYPE", data.GetItemTypeName());
        weightText.text = LocalizationHelper.Main("ITEM_INFO_WEIGHT", data.weight);
        infoText.text = data.GetDescription();

        if (useButton != null)
        {
            useButton.gameObject.SetActive(data.IsUsable() || data is ToolData);

            var btnText = useButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = (data is ToolData) ? "장착하기" : "사용하기";
            }
        }
    }
    public void OnClickUseButton()
    {
        if (currentData is ToolData tool)
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
