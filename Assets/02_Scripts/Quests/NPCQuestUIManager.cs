using UnityEngine;
using System;
using System.Collections.Generic;

public class NPCQuestUIManager : MonoBehaviour
{
    public static NPCQuestUIManager Instance;

    public GameObject uiPanel;
    public Transform contentParent;
    public GameObject questSlotPrefab;

    private Action onPanelClosed;

    private List<QuestData> currentQuestsToOffer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        uiPanel.SetActive(false);
    }

    // 인자가 NPCBrain 하나로 줄었습니다.
    public void OpenInteractionUI(List<QuestData> quests, Action onClose)
    {
        InputControlManager.Instance.LockInput();
        currentQuestsToOffer = quests;
        onPanelClosed = onClose;

        RefreshUI(); // 화면 그리기

        uiPanel.SetActive(true);
    }

    public void RefreshUI()
    {
        if (currentQuestsToOffer == null) return;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 퀘스트 리스트를 돌면서 UI 생성
        foreach (var questData in currentQuestsToOffer) 
        {
            // 완료 퀘스트는 안그림
            if (QuestManager.Instance.completedQuestIDs.Contains(questData.questID)) continue;

            GameObject slotObj = Instantiate(questSlotPrefab, contentParent);
            QuestSlotUI slotUI = slotObj.GetComponent<QuestSlotUI>();

            Quest activeQuest = QuestManager.Instance.GetActiveQuest(questData.questID);

            if (activeQuest == null)
            {
                slotUI.SetupSlot(questData, null, "수락하기", true, () => AcceptQuest(questData));
            }
            else if (activeQuest.IsAllObjectivesComplete())
            {
                slotUI.SetupSlot(questData, activeQuest, "보상받기", true, () => ClaimReward(activeQuest));
            }
            else
            {
                slotUI.SetupSlot(questData, activeQuest, "진행 중", false, null);
            }
        }
    }

    private void AcceptQuest(QuestData data)
    {
        QuestManager.Instance.AddQuest(data);
        RefreshUI();
    }

    private void ClaimReward(Quest activeQuest)
    {
        QuestManager.Instance.ClaimReward(activeQuest);
        RefreshUI();
    }

    public void CloseUI()
    {
        uiPanel.SetActive(false);
        InputControlManager.Instance.UnlockInput();
        onPanelClosed?.Invoke();
    }
}