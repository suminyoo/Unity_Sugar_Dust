using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Quest", menuName = "Game/Quest Data")]
public class QuestItemReward
{
    public string itemID;
    public int amount;
}
public class QuestData : ScriptableObject
{
    public string questID;
    public string questName;
    [TextArea] public string description;

    public List<QuestObjective> objectives;

    public int rewardGold;
    public List<QuestItemReward> rewardItems;

    public QuestData nextQuest;
}

public enum QuestType { Hunt, Collect, EarnMoney, ReachLevel, TalkToNPC, ArriveAtPoint }

[System.Serializable]
public class QuestObjective
{
    public QuestType type;
    public string targetID; // 몬스터ID 아이템ID NPCID 등등등등
    public int requiredAmount;
    public int currentAmount;
    public string customDescription;
    public bool IsComplete => currentAmount >= requiredAmount;
    public string GetProgressText(int currentAmount)
    {
        string titleText = customDescription;

        // Collect경우 ItemManager를 통해정보 가져오기
        if (type == QuestType.Collect)
        {
            if (ItemManager.Instance != null)
            {
                ItemData item = ItemManager.Instance.GetItemByID(targetID);
                if (item != null) titleText = item.itemName;
            }
            else
            {
                titleText = targetID;
                Debug.Log("TargetID: " + targetID);
            }
        }
        return $"{titleText} ({currentAmount}/{requiredAmount})";
    }
}
