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
    public TextMeshProUGUI interactionText;
    public float interactionRange = 10.0f; //안전장치용 거리이기때문에 넉넉히 10 이상(물체 크기 고려?)

    // 현재 범위 안에 있는 상호작용 가능한 물체들을 담을 리스트
    private List<IInteractable> interactablesInRange = new List<IInteractable>();
    private IInteractable currentInteractable;

    void Update()
    {
        //팝업창이 켜져 있다면 패널끔
        if (CommonConfirmPopup.Instance != null && CommonConfirmPopup.Instance.gameObject.activeSelf)
        {
            return; 
        }

        ValidateCurrentTarget();
        UpdateCurrentInteractable();
        HandleInput();

    }

    void ValidateCurrentTarget()
    {
        if (interactablesInRange.Count == 0) return;

        for (int i = interactablesInRange.Count - 1; i >= 0; i--)
        {
            var target = interactablesInRange[i] as MonoBehaviour;

            if (target == null)
            {
                interactablesInRange.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance > interactionRange)
            {
                // 트리거안에 있어도 중심점이 멀면 지워질 수 있음
                //Debug.LogWarning($"상호작용물체와 멀어져서 삭제됨: {target.name} (거리: {distance:F2} > {interactionRange})");
                interactablesInRange.RemoveAt(i);
            }
        }
    }

    public void ResetSystem()
    {
        interactablesInRange.Clear(); // 리스트 비우기
        currentInteractable = null;   // 현재 타겟 해제
    }

    // 리스트 중 가장 가까운 물체를 현재 타겟으로
    void UpdateCurrentInteractable()
    {
        interactablesInRange.RemoveAll(item => item == null || (item as MonoBehaviour) == null);

        if (interactablesInRange.Count == 0)
        {
            currentInteractable = null;
            PromptUIManager.Instance.ClearInteractionPrompt();
            return;
        }

        currentInteractable = interactablesInRange[0];

        
        PromptUIManager.Instance.SetInteractionPrompt(currentInteractable.GetInteractPrompt());
        
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
            //Debug.Log("interact: " + other.name);
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 나간 물체 리스트에서 제거
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            //Debug.Log("interact out: " + other.name);

            interactablesInRange.Remove(interactable);
        }
    }
}