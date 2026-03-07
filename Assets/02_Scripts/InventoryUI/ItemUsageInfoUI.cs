using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemUsageInfoUI : MonoBehaviour
{    
    [Header("UI")]
    public GameObject itemInfoPanel;
    public GameObject defaultPanel;

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
        defaultPanel.SetActive(false);
        itemInfoPanel.SetActive(true);

        icon.sprite = data.icon;
        nameText.text = data.GetItemName();
        typeText.text = LocalizationHelper.L("ITEM_INFO_TYPE", data.GetItemTypeName());
        weightText.text = LocalizationHelper.L("ITEM_INFO_WEIGHT", data.weight);
        infoText.text = data.GetDescription();
    }


    public void Close()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }

}
