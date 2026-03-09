using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
public class QuestItemReward
{
    public string itemID;
    public int amount;
}

public enum QuestType 
{ 
    Hunt, 
    Collect,
    TalkToNPC,
    ArriveAtPoint,
    EarnMoney, 
    ReachLevel

}

[System.Serializable]
public struct QuestTargetID
{
    public ItemID itemID;
    public EnemyID enemyID;
    public NPCID npcID;

    public string GetID(QuestType type)
    {
        return type switch
        {
            QuestType.Collect => itemID.ToString(),
            QuestType.Hunt => enemyID.ToString(),
            _ => "None"
        };
    }
}

[System.Serializable]
public class QuestObjective
{
    public QuestType type;
    public QuestTargetID targetID;
    public int requiredAmount;
    public int currentAmount;
    public LocalizedString objectiveDescription;
    public bool IsComplete => currentAmount >= requiredAmount;
    public string GetProgressText(int current)
    {
        string descText = "";

        if (objectiveDescription != null && !objectiveDescription.IsEmpty)
        {
            descText = objectiveDescription.GetLocalizedString();
        }
        else
        {
            if (type == QuestType.Collect)
            {
                var item = ItemDataManager.Instance.GetItemByID(targetID.itemID);
                descText = item.GetItemName();
                
            }
            else if (type == QuestType.Hunt)
            {
                var enemy = EnemyDataManager.Instance.GetEnemyByID(targetID.enemyID);
                descText = enemy.GetEnemyName();
            }
            else
            {
                descText = "알 수 없는 목표";
            }
        }

        return $"{descText} ({current}/{requiredAmount})";
    }
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Game/Quest Data")]

public class QuestData : ScriptableObject
{
    public QuestID questID;
    [SerializeField] private LocalizedString questName;
    [SerializeField] private LocalizedString description;

    public List<QuestObjective> objectives;

    public int rewardGold;
    public List<QuestItemReward> rewardItems;

    public string requiredQuestID;

    public string GetQuestName()
    {
        return questName.GetLocalizedString();
    }

    public string GetDescription()
    {
        return description.GetLocalizedString();
    }
}

