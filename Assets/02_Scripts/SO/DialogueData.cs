using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "NPC/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Speaker")]
    public string speakerName; // 화자 이름 (예: 상점 주인)

    [Header("Content")]
    [TextArea(3, 5)]
    public string[] sentences; // 대사 목록

    // [추후 구현: 선택지 시스템 예시]
    // [System.Serializable]
    // public struct Choice {
    //     public string choiceText; // "구매한다"
    //     public int nextDialogueID; // 선택 시 이어질 대화 ID
    // }
    // public Choice[] choices;
}