using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Settings")]
    public NPCID npcID;

    public float moveSpeed = 3.5f;
    public float waitTimeAtPoint = 2.0f; // 패트롤 지점 대기 시간
    public float detectRange = 5.0f;

    [Header("Quests")]
    public List<QuestData> questsToGive;

}