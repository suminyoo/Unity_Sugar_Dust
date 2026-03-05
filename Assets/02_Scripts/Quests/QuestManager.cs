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

    public Quest GetActiveQuest(string questID)
    {
        return activeQuests.Find(q => q.data.questID == questID);
    }

    // 퀘스트 수락
    public void AddQuest(QuestData questData)
    {
        if (activeQuests.Exists(q => q.data.questID == questData.questID)) return;

        Quest newQuest = new Quest(questData);
        activeQuests.Add(newQuest);

        newQuest.StartQuest();
        newQuest.EvaluateAll();
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
                        ItemData itemData = ItemDataManager.Instance.GetItemByID(obj.targetID);
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

            completedQuestIDs.Add(quest.data.questID);
            activeQuests.Remove(quest);

            // 보상 골드
            if (quest.data.rewardGold > 0)
            {
                PlayerAssetsManager.Instance.AddMoney(quest.data.rewardGold);
                NotificationUIManager.Instance.
                    ShowNotification($"퀘스트 보상 {quest.data.rewardGold}{CustomerPaymentSystem.CURRENCY_SYMBOL} 획득");
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
                            NotificationUIManager.Instance.
                                ShowNotification($"퀘스트 보상 {itemData.itemName} {reward.amount}개 획득 ");
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
                questID = quest.data.questID,
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
                // [중요!] 인벤토리 로드할 때처럼, ID를 바탕으로 원본 ScriptableObject를 찾아옵니다.
                // (주의: QuestDataManager는 ItemDataManager처럼 질문자님이 만드셔야 합니다!)
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
}