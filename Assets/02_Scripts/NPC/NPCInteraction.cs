using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    private NPCController controller;

    private void Awake()
    {
        controller = GetComponent<NPCController>();
    }

    public string GetInteractPrompt()
    {
        if (controller.npcData != null)
            return $"[E] {controller.npcData.npcName} 대화하기"; 
        else
        
            return controller.npcData.promptText; //기본
        
    }

    public void OnInteract()
    {
        controller.OnInteract();
    }
}