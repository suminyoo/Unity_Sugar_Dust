using UnityEngine;

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
    public float actionRange = 3.5f;
    public LayerMask actionLayer;

    [Header("Stats")]
    public float attackDamage = 10f;
    public float miningPower = 20f;

    [Header("Cooldown")]
    public float attackCooldown = 1f;
    public float mineCooldown = 1f;

    private float attackTimer = 0f;
    private float mineTimer = 0f;

    private PlayerController playerController;

    private ActionType currentActionType; //현재행동

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        DrawDebugRay(); // 디버그용
        HandleActionInput();
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

    void HandleActionInput()
    {
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
        playerController.Wield(actionType); //플레이어 애니메이션
    }

    public void ExecuteAction()
    {
        if (firePoint == null) return;

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        // TODO: 소리 재생

        if (!Physics.Raycast(ray, out hit, actionRange, actionLayer))
            return;

        switch (currentActionType)
        {
            case ActionType.Attack:
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(attackDamage);
                    Debug.Log("공격 적중 (이벤트 호출)");
                }
                break;

            case ActionType.Mine:
                IMineable mineral = hit.collider.GetComponent<IMineable>();
                if (mineral != null)
                {
                    mineral.OnMine(miningPower);
                    Debug.Log("채광 성공 (이벤트 호출)");
                }
                break;
        }
    }
}