using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CustomerDialogueDB", menuName = "NPC/Dialogue Database")]
public class CustomerDialogueDatabase : ScriptableObject
{
    [System.Serializable]
    public struct DialogueEntry
    {
        public CustomerType type;
        public List<DialogueData> dialogueOptions;
    }

    [Header("모든 손님 대사 리스트")]
    public List<DialogueEntry> dialogueList;

    private Dictionary<CustomerType, List<DialogueData>> dialogueMap;

    private void Init()
    {
        if (dialogueMap != null) return;

        dialogueMap = new Dictionary<CustomerType, List<DialogueData>>();
        foreach (var entry in dialogueList)
        {
            if (!dialogueMap.ContainsKey(entry.type))
            {
                dialogueMap.Add(entry.type, entry.dialogueOptions);
            }
        }
    }

    public DialogueData GetDialogueByType(CustomerType type)
    {
        Init();

        if (dialogueMap.ContainsKey(type))
        {
            List<DialogueData> options = dialogueMap[type];

            if (options != null && options.Count > 0)
            {
                int randomIndex = Random.Range(0, options.Count);
                return options[randomIndex];
            }
        }

        Debug.LogWarning($"[DB] {type}에 해당하는 대사가 없거나 비어있음");
        return null;
    }
}