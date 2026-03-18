using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;

[System.Serializable]
public class QuestItemReward
{
    public ItemID itemID;
    public int amount;
}

public enum QuestType 
{ 
    Hunt, 
    Collect,
    TalkToNPC,
    ReachPoint,
    ReachDailyIncome, 
    PossessMoney, //특정액수 돈 소지 (소지할때만)

}
[System.Serializable]
public class QuestObjective
{
    public QuestType type;

    public ItemID itemID;
    public EnemyID enemyID;
    public NPCID npcID;
    public PointID pointID;

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
            switch (type)
            {
                case QuestType.Collect:
                    {
                        var item = ItemDataManager.Instance.GetItemByID(itemID);
                        descText = item != null ? item.GetItemName() : "알 수 없는 아이템";
                        break;
                    }
                case QuestType.Hunt:
                    {
                        var enemy = EnemyDataManager.Instance.GetEnemyByID(enemyID);
                        descText = enemy != null ? enemy.GetEnemyName() + " 처치" : "알 수 없는 몬스터 처치";
                        break;
                    }
                case QuestType.ReachPoint:
                    descText = "목표 장소 도달";
                    requiredAmount = 1;
                    break;
                case QuestType.ReachDailyIncome:
                    descText = "장사 수익 누적";
                    break;
                case QuestType.PossessMoney:
                    descText = "목표 금액 소지";
                    break;
                default:
                    descText = "알 수 없는 목표";
                    break;
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

    public List<QuestObjective> objectives = new List<QuestObjective>();

    public int rewardGold;
    public List<QuestItemReward> rewardItems;
    public QuestID requiredQuestID;
    public bool requireClaimToUnlockNext = false; //제출퀘스트만 키면 됨
    public string GetQuestName() => questName.GetLocalizedString();
    public string GetDescription() => description.GetLocalizedString();
}