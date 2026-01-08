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
        if (controller.data != null)
            return controller.data.promptText;

        return "[E] 대화하기"; // 기본값
    }

    public void OnInteract()
    {
        controller.OnInteract();
    }
}