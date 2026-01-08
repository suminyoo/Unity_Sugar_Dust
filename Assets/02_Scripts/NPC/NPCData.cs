using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "SO/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Basic Info")]
    public string npcName;
    [TextArea] public string description;

    [Header("Settings")]
    public float moveSpeed = 3.5f;
    public float waitTimeAtPoint = 2.0f; // 패트롤 지점 대기 시간

    [Header("Interaction")]
    public string promptText = "[E] 대화하기"; // 상호작용 텍스트

    // 나중에 대화 데이터, 상점 인벤토리 데이터 등을 여기에 추가
    // public DialogueData dialogue; 
    // public ShopData shopData;
}