using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable, IQuestInteractable
{
    private NPCController controller;

    private void Awake()
    {
        controller = GetComponent<NPCController>();
    }

    // 기본 상호작용
    public string GetInteractPrompt()
    {
        if (controller.npcData != null)
            return $"{controller.npcData.npcName} 대화하기";
        else
            return controller.npcData.promptText;
    }

    public void OnInteract()
    {
        controller.OnInteract();
    }

    // 퀘스트 상호작용
    public string GetQuestPrompt()
    {
        if (controller.npcData != null)
            return controller.npcData.questPromptText;
        return "퀘스트 보기";
    }

    public void OnQuestInteract()
    {
        controller.OnQuestInteract();
    }

    public bool HasAvailableQuest()
    {
        return controller.Brain.HasAvailableQuest();
    }
}