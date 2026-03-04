using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable, IQuestInteractable
{
    private NPCController controller;

    private void Awake()
    {
        controller = GetComponent<NPCController>();
    }

    // E키
    public string GetInteractPrompt()
    {
        if (controller.npcData != null)
            return $"[E] {controller.npcData.npcName} 대화하기";
        else
            return controller.npcData.promptText;
    }

    public void OnInteract()
    {
        controller.OnInteract();
    }

    // R키
    public string GetQuestPrompt()
    {
        if (controller.npcData != null)
            return controller.npcData.questPromptText;
        return "[R] 퀘스트 보기";
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