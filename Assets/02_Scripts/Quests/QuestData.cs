using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Quest", menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questID;
    public string questName;
    [TextArea] public string description;

    public List<QuestObjective> objectives;

    public int rewardGold;
    public QuestData nextQuest;
}

public enum QuestType { Hunt, Collect, EarnMoney, ReachLevel, TalkToNPC }

[System.Serializable]
public class QuestObjective
{
    public QuestType type;
    public string targetID; // 몬스터ID 아이템ID NPCID 등등등등
    public int requiredAmount;
    public int currentAmount;

    public bool IsComplete => currentAmount >= requiredAmount;
}