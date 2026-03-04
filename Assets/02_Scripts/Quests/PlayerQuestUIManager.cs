using UnityEngine;

public class PlayerQuestUIManager : MonoBehaviour
{
    public static PlayerQuestUIManager Instance;

    public GameObject playerQuestPanel;
    public Transform contentParent;
    public GameObject questSlotPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        playerQuestPanel.SetActive(false);
    }

    private void Update()
    {
        // 팝업이 켜져있거나 npc 상호작용중에는 Q 안되게
        if (InputControlManager.Instance != null && InputControlManager.Instance.IsInputLocked) return;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool isActive = !playerQuestPanel.activeSelf;
            playerQuestPanel.SetActive(isActive);

            if (isActive)
            {
                UpdateQuestUI();
                InputControlManager.Instance.LockInput();
                Cursor.visible = true;
            }
            else
            {
                InputControlManager.Instance.UnlockInput();
            }
        }
    }

    public void UpdateQuestUI()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        foreach (Quest quest in QuestManager.Instance.activeQuests)
        {
            GameObject slotObj = Instantiate(questSlotPrefab, contentParent);
            QuestSlotUI slotUI = slotObj.GetComponent<QuestSlotUI>();

            if (quest.IsAllObjectivesComplete())
            {
                slotUI.SetupSlot(quest.data, quest, "보상 받기", true, () => ClaimReward(quest));
            }
            else
            {
                slotUI.SetupSlot(quest.data, quest, "진행 중", false, null);
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