using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

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
                    if (ItemManager.Instance != null && PlayerInventory.Instance != null)
                    {
                        ItemData itemData = ItemManager.Instance.GetItemByID(obj.targetID);
                        if (itemData != null)
                        {
                            // 내 인벤토리에서 요구치만큼 아이템 소모!
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
                Debug.Log($"{quest.data.rewardGold} 골드 보상 획득!");
            }

            // 보상 아이템
            if (quest.data.rewardItems != null && quest.data.rewardItems.Count > 0)
            {
                foreach (var reward in quest.data.rewardItems)
                {
                    if (ItemManager.Instance != null && PlayerInventory.Instance != null)
                    {
                        ItemData itemData = ItemManager.Instance.GetItemByID(reward.itemID);
                        if (itemData != null)
                        {
                            PlayerInventory.Instance.AddItem(itemData, reward.amount);
                            Debug.Log($"보상 아이템 획득: {itemData.itemName} {reward.amount}개");
                        }
                    }
                }
            }

            // UI 새로고침
            if (PlayerQuestUIManager.Instance != null) PlayerQuestUIManager.Instance.UpdateQuestUI();
            if (NPCQuestUIManager.Instance != null && NPCQuestUIManager.Instance.npcQuestPanel.activeSelf)
                NPCQuestUIManager.Instance.RefreshUI();
        }
    }
}