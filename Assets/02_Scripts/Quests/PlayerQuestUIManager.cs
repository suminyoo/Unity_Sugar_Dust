using UnityEngine;

public class PlayerQuestUIManager : MonoBehaviour
{
    public static PlayerQuestUIManager Instance;

    public GameObject playerQuestPanel;
    public Transform contentParent;
    public GameObject questSlotPrefab;
    public GameObject defaultPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        playerQuestPanel.SetActive(false);
    }
    private void Update()
    {
        if (InputControlManager.Instance != null && InputControlManager.Instance.IsInputLocked) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuestPanel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playerQuestPanel.activeSelf)
            {
                playerQuestPanel.SetActive(false);
            }
        }
    }
    private void ToggleQuestPanel()
    {
        bool isActive = !playerQuestPanel.activeSelf;
        playerQuestPanel.SetActive(isActive);

        if (isActive)
        {
            QuestManager.Instance.RefreshAllQuestProgress();
            UpdateQuestUI();
        }
    }
    public void UpdateQuestUI()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        var activeQuests = QuestManager.Instance.activeQuests;

        if (activeQuests == null || activeQuests.Count == 0)
        {
            defaultPanel.SetActive(true);
            return;
        }
        else
        {
            defaultPanel.SetActive(false);
        }


        foreach (Quest quest in QuestManager.Instance.activeQuests)
        {
            GameObject slotObj = Instantiate(questSlotPrefab, contentParent);
            QuestSlotUI slotUI = slotObj.GetComponent<QuestSlotUI>();

            if (quest.IsAllObjectivesComplete())
            {
                slotUI.SetupSlot(quest.data, quest, LocalizationHelper.L("QUEST_CLAIM"), true, () => ClaimReward(quest));
            }
            else
            {
                slotUI.SetupSlot(quest.data, quest, LocalizationHelper.L("QUEST_IN_PROGRESS"), false, null);
            }
        }
    }
    private void ClaimReward(Quest quest)
    {
        QuestManager.Instance.ClaimReward(quest);
    }
    public void CloseQuestPanel()
    {
        playerQuestPanel.SetActive(false);
        InputControlManager.Instance.UnlockInput();
    }
}