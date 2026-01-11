using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Settings")]
    public string npcName;
    [TextArea] public string description;

    public float moveSpeed = 3.5f;
    public float waitTimeAtPoint = 2.0f; // 패트롤 지점 대기 시간

    [Header("Interaction")]
    public string promptText = $"[E] 대화하기"; // 상호작용 텍스트
    public DialogueData defaultDialogue;

    public string defaultGreeting;
    public float detectRange = 5.0f;

}