using UnityEngine;
using UnityEngine.Localization.Settings;

public class NPCInteraction : MonoBehaviour, IInteractable, IQuestInteractable
{
    private NPCController controller;

    private void Awake()
    {
        controller = GetComponent<NPCController>();
    }

    public string GetInteractPrompt() => $"{LocalizationHelper.L("PROMPT_TALK_TO_NPC", controller.npcData.GetNpcName())}";

    public string GetQuestPrompt() => LocalizationHelper.L("PROMPT_VIEW_QUEST");

    public void OnInteract() => controller.OnInteract();
    
    public void OnQuestInteract() => controller.OnQuestInteract();

    public bool HasAvailableQuest() => controller.Brain.HasAvailableQuest();
    
}