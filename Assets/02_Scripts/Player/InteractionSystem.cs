using UnityEngine;
public interface IInteractable
{
    string GetInteractText(); // (선택) 화면에 띄울 메시지 (예: "상자 열기 [E]")
    void OnInteract();        // 실제 동작
}
public class InteractionSystem : MonoBehaviour
{
    [Header("Settings")]
    public Transform firePoint;
    public float interactRange = 2.0f; // 상호작용은 보통 공격보다 짧습니다.
    public LayerMask interactLayer;

    void Update()
    {
        // 1. 레이캐스트로 앞에 상호작용 대상이 있는지 상시 체크 (UI 표시용으로 유용)
        CheckInteractable();

        // 2. E 키 입력 시 상호작용 실행
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformInteraction();
        }
    }

    void PerformInteraction()
    {
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnInteract();
                Debug.Log($"{hit.collider.name}와 상호작용함");
            }
        }
    }

    void CheckInteractable()
    {
        // 여기서 Raycast를 쏴서 UI에 "E키를 눌러 상호작용" 같은 텍스트를 띄울 수 있습니다.
    }
}