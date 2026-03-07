using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "NPC/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Content")]
    [TextArea(3, 5)]
    public string[] sentences; // 대사 목록

    // TODO: 선택지 시스템 예시
    // [System.Serializable]
    // public struct Choice {
    //     public string choiceText; // 구매텍스트
    //     public int nextDialogueID; // 선택 시 이어질 대화 ID
    // }
    // public Choice[] choices;
}