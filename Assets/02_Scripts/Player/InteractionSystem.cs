using TMPro;
using UnityEngine;

public interface IInteractable
{
    void OnInteract();
    string GetInteractPrompt();
}

public class InteractionSystem : MonoBehaviour
{
    public BoxCollider interactionBoxCollider;
    public LayerMask interactionLayer;    // Interaction 레이어

    [Header("UI")]
    public GameObject interactionPanel;
    public TextMeshProUGUI interactionText;

    private IInteractable currentInteractable;

    void Update()
    {
        CheckForInteractable();
        HandleInput();
    }

    void CheckForInteractable()
    {
        if (interactionBoxCollider == null) return;

        // 센터 위치: 로컬 좌표를 월드 좌표로
        Vector3 center = interactionBoxCollider.transform.TransformPoint(interactionBoxCollider.center);

        // 크기: 콜라이더 크기에 스케일을 곱하고 절반으로 나눔
        Vector3 halfExtents = Vector3.Scale(interactionBoxCollider.size, interactionBoxCollider.transform.lossyScale) * 0.5f;

        // 회전: 콜라이더의 회전값 그대로 사용
        Quaternion rotation = interactionBoxCollider.transform.rotation;

        // OverlapBox 실행
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation, interactionLayer);

        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        // 가장 가까운 물체 찾기
        foreach (Collider hit in hitColliders)
        {
            // 자기 자신은 무시
            if (hit == interactionBoxCollider) continue;

            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable != null)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        // UI 처리
        if (closestInteractable != null)
        {
            currentInteractable = closestInteractable;
            if (interactionPanel != null)
            {
                interactionPanel.SetActive(true);
                if (interactionText != null) interactionText.text = closestInteractable.GetInteractPrompt();
            }
        }
        else
        {
            currentInteractable = null;
            if (interactionPanel != null) interactionPanel.SetActive(false);
        }
    }

    void HandleInput()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.OnInteract();
        }
    }

    void OnDrawGizmos()
    {
        if (interactionBoxCollider == null) return;

        Gizmos.color = new Color(1, 0, 0, 0.3f);

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(
            interactionBoxCollider.transform.TransformPoint(interactionBoxCollider.center),
            interactionBoxCollider.transform.rotation,
            Vector3.Scale(interactionBoxCollider.size, interactionBoxCollider.transform.lossyScale)
        );
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}