using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IInteractable
{
    void OnInteract();
    string GetInteractPrompt();
}
public class InteractionSystem : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactionPanel;
    public TextMeshProUGUI interactionText;

    // 현재 범위 안에 있는 상호작용 가능한 물체들을 담을 리스트
    private List<IInteractable> interactablesInRange = new List<IInteractable>();
    private IInteractable currentInteractable;

    void Update()
    {
        UpdateCurrentInteractable();
        HandleInput();
    }

    // 리스트 중 가장 가까운 물체를 현재 타겟으로
    void UpdateCurrentInteractable()
    {
        if (interactablesInRange.Count == 0)
        {
            currentInteractable = null;
            if (interactionPanel != null) interactionPanel.SetActive(false);
            return;
        }

        currentInteractable = interactablesInRange[0];

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(true);
            if (interactionText != null) interactionText.text = currentInteractable.GetInteractPrompt();
        }
    }

    void HandleInput()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.OnInteract();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 들어온 물체가 IInteractable을 가지고 있는지
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 나간 물체 리스트에서 제거
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactablesInRange.Remove(interactable);
        }
    }
}