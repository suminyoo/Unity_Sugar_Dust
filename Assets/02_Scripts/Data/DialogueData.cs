using UnityEngine;
using UnityEngine.Localization;
using System;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "NPC/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Content")]
    [SerializeField] private LocalizedString dialogueBlock;

    public string[] GetSentences()
    {
        string fullText = dialogueBlock.GetLocalizedString();

        if (string.IsNullOrEmpty(fullText))
        {
            return new string[0];
        }

        string[] splitSentences = fullText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        return splitSentences;
    }
}
// TODO: 선택지 시스템 예시
// [System.Serializable]
// public struct Choice {
//     public string choiceText; // 구매텍스트
//     public int nextDialogueID; // 선택 시 이어질 대화 ID
// }
// public Choice[] choices;
