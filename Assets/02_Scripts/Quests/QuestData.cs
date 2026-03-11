using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
public class QuestItemReward
{
    public ItemID itemID;
    public int amount;
}

public enum QuestType 
{ 
    Hunt, 
    Collect,
    Talk,
    Destination,
    EarnMoney, 
    ReachLevel

}
[System.Serializable]
public class QuestObjective
{
    public QuestType type;

    public ItemID itemID;
    public EnemyID enemyID;
    public NPCID npcID;
    public DestinationID destinationID;

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
                var item = ItemDataManager.Instance.GetItemByID(itemID);
                descText = item != null ? item.GetItemName() : "알 수 없는 아이템";
            }
            else if (type == QuestType.Hunt)
            {
                var enemy = EnemyDataManager.Instance.GetEnemyByID(enemyID);
                descText = enemy != null ? enemy.GetEnemyName() + " 처치" : "알 수 없는 몬스터 처치";
            }
            else if (type == QuestType.Destination)
            {
                descText = "목표 장소 도달";
                requiredAmount = 1;
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

    public List<QuestObjective> objectives = new List<QuestObjective>();

    public int rewardGold;
    public List<QuestItemReward> rewardItems;
    public QuestID requiredQuestID;

    public string GetQuestName() => questName.GetLocalizedString();
    public string GetDescription() => description.GetLocalizedString();
}