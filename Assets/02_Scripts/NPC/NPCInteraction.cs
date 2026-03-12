using UnityEngine;
using UnityEngine.Localization.Settings;

public class NPCInteraction : MonoBehaviour, IInteractable, IQuestInteractable
{
    private NPCController controller;

    private void Awake()
    {
        controller = GetComponent<NPCController>();
    }

    public string GetInteractPrompt() => $"{LocalizationHelper.Main("PROMPT_TALK_TO_NPC", controller.GetNpcName())}";

    public string GetQuestPrompt() => LocalizationHelper.Main("PROMPT_VIEW_QUEST");

    public void OnInteract() => controller.OnInteract();
    
    public void OnQuestInteract() => controller.OnQuestInteract();

    public bool HasAvailableQuest() => controller.Brain.HasAvailableQuest();
    
}