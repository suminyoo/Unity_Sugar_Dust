using System.Collections.Generic;
using UnityEngine;

// 실제 게임 중에 진행도를 저장 SO 변경 안할거라서
[System.Serializable]
public class Quest
{
    public QuestData data;
    public int[] currentAmounts;
    public bool isRewardClaimed = false; 

    public Quest(QuestData data)
    {
        this.data = data;
        currentAmounts = new int[data.objectives.Count];
    }

    // 목표가 달성 확인
    public bool IsAllObjectivesComplete()
    {
        for (int i = 0; i < data.objectives.Count; i++)
        {
            if (currentAmounts[i] < data.objectives[i].requiredAmount)
                return false;
        }
        return true;
    }
}

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

    // npc가 퀘스트를 줄 때 호출
    public void AddQuest(QuestData questData)
    {
        // 이미 가지고 있는 퀘스트인지
        if (activeQuests.Exists(q => q.data.questID == questData.questID)) return;

        Quest newQuest = new Quest(questData);
        activeQuests.Add(newQuest);
        Debug.Log($"퀘스트 수락됨: {questData.questName}");

        PlayerQuestUIManager.Instance.UpdateQuestUI();
    }

    public void AddProgressToCollect(string itemID, int amount)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.isRewardClaimed) continue; // 이미 보상받은 퀘스트는 무시

            for (int i = 0; i < quest.data.objectives.Count; i++)
            {
                var obj = quest.data.objectives[i];
                if (obj.type == QuestType.Collect && obj.targetID == itemID)
                {
                    quest.currentAmounts[i] += amount;
                    PlayerQuestUIManager.Instance.UpdateQuestUI();
                }
            }
        }
    }
    public Quest GetActiveQuest(string questID)
    {
        return activeQuests.Find(q => q.data.questID == questID);
    }
    // 보상
    public void ClaimReward(Quest quest)
    {
        if (quest.IsAllObjectivesComplete() && !quest.isRewardClaimed)
        {
            quest.isRewardClaimed = true;

            // 완료
            completedQuestIDs.Add(quest.data.questID);

            activeQuests.Remove(quest);

            Debug.Log($"{quest.data.rewardGold} 골드 보상 획득");

            if (PlayerQuestUIManager.Instance != null) PlayerQuestUIManager.Instance.UpdateQuestUI();
            if (NPCQuestUIManager.Instance != null && NPCQuestUIManager.Instance.npcQuestPanel.activeSelf)
                NPCQuestUIManager.Instance.RefreshUI();
        }
    }
}