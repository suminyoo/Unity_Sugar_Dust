using UnityEngine;
using System.Collections.Generic;
public class QuestItemReward
{
    public string itemID;
    public int amount;
}
public enum QuestType { Hunt, Collect, EarnMoney, ReachLevel, TalkToNPC, ArriveAtPoint }

[System.Serializable]
public class QuestObjective
{
    public QuestType type;
    public string targetID; // 몬스터ID 아이템ID NPCID 등등등등
    public int requiredAmount;
    public int currentAmount;
    public string objectiveDescription;
    public bool IsComplete => currentAmount >= requiredAmount;
    public string GetProgressText(int current)
    {
        if (string.IsNullOrEmpty(objectiveDescription))
        {
            if (type == QuestType.Collect)
            {
                var item = ItemDataManager.Instance.GetItemByID(targetID);
                objectiveDescription = item != null ? item.itemName : "아이템";
            }
            else if (type == QuestType.Hunt)
            {
                objectiveDescription = targetID; 
            }
        }
        return $"{objectiveDescription} ({current}/{requiredAmount})";
    }
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Game/Quest Data")]

public class QuestData : ScriptableObject
{
    public string questID;
    public string questName;
    [TextArea] public string description;

    public List<QuestObjective> objectives;

    public int rewardGold;
    public List<QuestItemReward> rewardItems;

    public string requiredQuestID;
}

