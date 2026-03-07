using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI saveTimeText;
    public TextMeshProUGUI inGameDayTimeText;
    public TextMeshProUGUI playTimeText;
    public GameObject emptySlotText;

    public int slotNumber;
    public bool isSubSlot = false;
    private string fullFilePath;
    private SaveLoadUIManager titleManager;
    private Button myButton;

    public TextMeshProUGUI slotNameText;

    public void UpdateSlotUI(SaveMetadata data, string path = "", SaveLoadUIManager manager = null, bool isSub = false)
    {
    
        if (slotNameText != null && !isSub)
        {
            slotNameText.text = LocalizationHelper.L("UI_MENU_SAVE", slotNumber + 1);
        }
    
        fullFilePath = path;
        titleManager = manager;
        isSubSlot = isSub;

        myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.RemoveAllListeners();
            myButton.onClick.AddListener(OnClickThisSlot);
        }

        if (data == null)
        {
            emptySlotText.SetActive(true);
            SetTextActive(false);
            return;
        }
        string timeText = LocalizationHelper.GetGameTimeText(data.inGameTime);

        string timeName = LocalizationHelper.GetGameTimeText(data.inGameTime);
        emptySlotText.SetActive(false);
        SetTextActive(true);

        saveTimeText.text = data.saveTime;

        inGameDayTimeText.text = LocalizationHelper.L("UI_DAY_AND_TIME", data.inGameDay, timeName);

        int hours = Mathf.FloorToInt(data.playTime / 3600);
        int minutes = Mathf.FloorToInt((data.playTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(data.playTime % 60);
        playTimeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public void OnClickThisSlot()
    {
        if (titleManager == null) return;

        if (isSubSlot)
        {
            if (string.IsNullOrEmpty(fullFilePath)) return;
            titleManager.LoadSaveFile(fullFilePath, slotNumber);
        }
        else
        {
            titleManager.OnSlotClick(slotNumber);
        }
    }

    private void SetTextActive(bool active)
    {
        saveTimeText.gameObject.SetActive(active);
        inGameDayTimeText.gameObject.SetActive(active);
        playTimeText.gameObject.SetActive(active);
    }
}
