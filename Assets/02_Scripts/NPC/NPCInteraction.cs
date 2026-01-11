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
            return controller.npcData.promptText;

        return $"[E] {controller.npcData.npcName} 대화하기"; // 기본값
    }

    public void OnInteract()
    {
        controller.OnInteract();
    }
}