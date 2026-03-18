using System.Collections.Generic;
using UnityEngine;


public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<Quest> activeQuests = new List<Quest>();
    public List<string> completedQuestIDs = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void OnEnable()
    {
        GameEvents.OnQuestProgressUpdated += RefreshQuestAlertStatus;
        GameEvents.OnQuestAccepted += HandleQuestAccepted;
    }

    private void OnDisable()
    {
        GameEvents.OnQuestProgressUpdated -= RefreshQuestAlertStatus;
        GameEvents.OnQuestAccepted -= HandleQuestAccepted;
    }
    public Quest GetActiveQuest(string questID)
    {
        return activeQuests.Find(q => q.data.questID.ToString() == questID);
    }

    // 퀘스트 수락
    public void AddQuest(QuestData questData)
    {
        if (activeQuests.Exists(q => q.data.questID == questData.questID)) return;

        Quest newQuest = new Quest(questData);
        activeQuests.Add(newQuest);

        newQuest.StartQuest();
        newQuest.EvaluateAll();

        GameEvents.OnQuestAccepted?.Invoke(questData.questID);
    }

    public void RefreshAllQuestProgress()
    {
        foreach (var quest in activeQuests)
        {
            quest.EvaluateAll();
        }
    }

    // 보상 
    public void ClaimReward(Quest quest)
    {
        quest.EvaluateAll();

        if (quest.IsAllObjectivesComplete() && !quest.isRewardClaimed)
        {
            // 제출 퀘스트면 인벤토리에서 아이템 빼가기
            foreach (var obj in quest.data.objectives)
            {
                if (obj.type == QuestType.Collect)
                {
                    if (ItemDataManager.Instance != null && PlayerInventory.Instance != null)
                    {
                        ItemData itemData = ItemDataManager.Instance.GetItemByID(obj.itemID.ToString());
                        if (itemData != null)
                        {
                            PlayerInventory.Instance.ConsumeItem(itemData, obj.requiredAmount);
                        }
                    }
                }
            }

            // 상태 변경 및 리스트 정리
            quest.isRewardClaimed = true;
            quest.StopQuest();

            completedQuestIDs.Add(quest.data.questID.ToString());
            activeQuests.Remove(quest);
            GameEvents.OnQuestCompleted?.Invoke(quest.data.questID);

            // 보상 골드
            if (quest.data.rewardGold > 0)
            {
                PlayerAssetsManager.Instance.AddMoney(quest.data.rewardGold);
                NotificationUIManager.Instance.ShowNotification(
                    LocalizationHelper.Main(
                        "NOTI_QUEST_REWARD_GOLD",
                        quest.data.rewardGold,
                        CustomerPaymentSystem.CURRENCY_SYMBOL
                    )
                );
            }

            // 보상 아이템
            if (quest.data.rewardItems != null && quest.data.rewardItems.Count > 0)
            {
                foreach (var reward in quest.data.rewardItems)
                {
                    if (ItemDataManager.Instance != null && PlayerInventory.Instance != null)
                    {
                        ItemData itemData = ItemDataManager.Instance.GetItemByID(reward.itemID);
                        if (itemData != null)
                        {
                            PlayerInventory.Instance.AddItem(itemData, reward.amount);
                            NotificationUIManager.Instance.ShowNotification(
                                LocalizationHelper.Main(
                                    "NOTI_QUEST_REWARD_ITEM",
                                    itemData.GetItemName(),
                                    reward.amount
                                )
                            );
                        }
                    }
                }
            }

            // UI 새로고침
            if (PlayerQuestUIManager.Instance != null) 
                PlayerQuestUIManager.Instance.UpdateQuestUI();
            if (NPCQuestUIManager.Instance != null && NPCQuestUIManager.Instance.npcQuestPanel.activeSelf)
                NPCQuestUIManager.Instance.RefreshUI();
        }
    }

    public void SaveData()
    {
        // 세이브용 QuestSaveData로 변환
        List<QuestSaveData> activeDataList = new List<QuestSaveData>();
        foreach (var quest in activeQuests)
        {
            activeDataList.Add(new QuestSaveData
            {
                questID = quest.data.questID.ToString(),
                currentAmounts = (int[])quest.currentAmounts.Clone(),
                isRewardClaimed = quest.isRewardClaimed
            });
        }

        GameSaveManager.Instance.SaveQuestData(completedQuestIDs, activeDataList);
    }

    public void LoadSavedQuests()
    {
        var loadedData = GameSaveManager.Instance.LoadQuestData();

        // 완료 퀘스트
        completedQuestIDs = loadedData.completedIDs ?? new List<string>();

        // 진행 중 퀘스트
        activeQuests.Clear();
        if (loadedData.activeQuestData != null)
        {
            foreach (var saveData in loadedData.activeQuestData)
            {
                QuestData data = QuestDataManager.Instance.GetQuestByID(saveData.questID);

                if (data != null)
                {
                    // 빈 퀘스트에 정보 넣기
                    Quest restoredQuest = new Quest(data);

                    // 진행도
                    restoredQuest.currentAmounts = (int[])saveData.currentAmounts.Clone();
                    restoredQuest.isRewardClaimed = saveData.isRewardClaimed;

                    restoredQuest.StartQuest(); //퀘스트 추적시작
                    activeQuests.Add(restoredQuest); //추가
                }
            }
        }
    }

    private void HandleQuestAccepted(QuestID id)
    {
        PlayerQuestUIManager.Instance.ShowQuestAlert();
    }

    public void RefreshQuestAlertStatus()
    {
        bool hasReadyToClaim = false;

        foreach (var quest in activeQuests)
        {
            if (quest.IsAllObjectivesComplete())
            {
                hasReadyToClaim = true;
                break;
            }
        }
        if (hasReadyToClaim)
        {
            PlayerQuestUIManager.Instance.ShowQuestAlert();
        }
        else
        {
            PlayerQuestUIManager.Instance.HideQuestAlert();
        }
    }
}