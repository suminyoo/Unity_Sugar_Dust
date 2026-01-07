using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemInfoUI : MonoBehaviour
{    
    [Header("UI")]
    public GameObject itemInfoPanel;
    public GameObject defaultPanel;

    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;
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
        nameText.text = data.itemName;
        weightText.text = $"¹«°Ô: {data.weight:F1}kg";
        infoText.text = data.description;
    }


    public void Close()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }

}
