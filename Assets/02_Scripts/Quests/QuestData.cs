using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
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
                var item = ItemDataManager.Instance.GetItemByID(targetID);
                if (item != null)
                {
                    descText = item.GetItemName();
                }
            }
            else if (type == QuestType.Hunt)
            {
                var enemy = EnemyDataManager.Instance.GetEnemyByID(targetID);
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
    public string questID;
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

