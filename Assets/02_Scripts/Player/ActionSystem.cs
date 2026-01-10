using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IMineable { void OnMine(float power); }
public interface IDamageable { void TakeDamage(float damage); }

public class ActionSystem : MonoBehaviour
{
    public enum ActionType
    {
        Attack,
        Mine
    }

    [Header("Settings")]
    public Transform firePoint;
    public float actionRange = 2f;
    public LayerMask actionLayer;

    public float attackDamage = 10f;
    public float miningPower = 20f;

    public float attackCooldown = 1f;
    public float mineCooldown = 1f;

    private float attackTimer = 0f;
    private float mineTimer = 0f;

    private PlayerController playerController;
    private ActionType currentActionType; //현재행동

    public GameObject sword;
    public GameObject pickaxe;

    private bool isActionLocked = false; //입력 차단

    private RaycastHit currentHit; // 레이캐스트 결과를 저장할 변수
    private bool hasTarget;        // 타겟이 잡혔는지 여부

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        InputControlManager.Instance.OnInputStateChanged += (canInput) =>
        {
            isActionLocked = !canInput;
        };
    }

    void Update()
    {
        DrawDebugRay(); // 디버그용

        // 레이로 물체감지
        UpdateRaycast();


        HandleActionInput();
        
    }

    // 레이로 타겟 감지 하고 텍스트 변경요청
    void UpdateRaycast()
    {
        if (firePoint == null) return;

        Ray ray = new Ray(firePoint.position, firePoint.forward);

        // 레이어 마스크를 통해 적/광물만 걸러냄
        if (Physics.Raycast(ray, out currentHit, actionRange, actionLayer))
        {
            hasTarget = true;

            // 적 IDamageable 확인
            if (currentHit.collider.GetComponentInParent<IDamageable>() != null)
            {
                PromptUIManager.Instance.SetActionPrompt("[LMB] 공격");
            }
            // 광물 IMineable 확인
            else if (currentHit.collider.GetComponentInParent<IMineable>() != null)
            {
                PromptUIManager.Instance.SetActionPrompt("[RMB] 채광");
            }
            else
            {
                // 액션 레이어지만 상호작용 대상 아니면 요청 취소 (미사용
                PromptUIManager.Instance.ClearActionPrompt();
            }
        }
        else
        {
            hasTarget = false;
            // 타겟없으면요청 취소
            PromptUIManager.Instance.ClearActionPrompt();
        }
    }

    void DrawDebugRay()
    {
        if (firePoint == null) return;
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, actionRange, actionLayer))
            Debug.DrawLine(ray.origin, hit.point, Color.green);
        else
            Debug.DrawRay(ray.origin, ray.direction * actionRange, Color.red);
    }

    void HandleToolVisibility(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Attack:
                sword.SetActive(true);
                pickaxe.SetActive(false);
                break;

            case ActionType.Mine:
                sword.SetActive(false);
                pickaxe.SetActive(true);
                break;
        }
    }

    void HandleActionInput()
    {
        // UI 클릭 체크 , 액션 락 체크
        if (isActionLocked) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        

        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (mineTimer > 0) mineTimer -= Time.deltaTime;

        // 쿨타임 체크 및 애니메이션 시작 요청만 함
        if (Input.GetMouseButton(0) && attackTimer <= 0f)
        {
            StartAction(ActionType.Attack);
            attackTimer = attackCooldown;
        }

        if (Input.GetMouseButton(1) && mineTimer <= 0f)
        {
            StartAction(ActionType.Mine);
            mineTimer = mineCooldown;
        }
    }

    void StartAction(ActionType actionType)
    {
        currentActionType = actionType;
        HandleToolVisibility(actionType); //도구 보이기/숨기기
        playerController.HandleWield(actionType); //플레이어 애니메이션
    }

    public void ExecuteAction()
    {
        if (!hasTarget) return;


        // TODO: 소리 재생

        switch (currentActionType)
        {
            case ActionType.Attack:
                IDamageable target = currentHit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(attackDamage);
                    Debug.Log("공격 적중");
                }
                break;

            case ActionType.Mine:
                IMineable mineral = currentHit.collider.GetComponent<IMineable>();
                if (mineral != null)
                {
                    mineral.OnMine(miningPower);
                    Debug.Log("채광 성공");
                }
                break;
        }
    }
}