using UnityEngine;

public interface IMineable { void OnMine(float power); }
public interface IDamageable { void OnDamage(float damage); }



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

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        DrawDebugRay(); //디버그용
        HandleActionInput();
    }

    //디버그용
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
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (mineTimer > 0)
            mineTimer -= Time.deltaTime;

        if (Input.GetMouseButton(0) && attackTimer <= 0f)
        {
            TryAction(ActionType.Attack);
            attackTimer = attackCooldown;
        }

        if (Input.GetMouseButton(1) && mineTimer <= 0f)
        {
            TryAction(ActionType.Mine);
            mineTimer = mineCooldown;
        }

    }
    void TryAction(ActionType actionType)
    {
        if (firePoint == null) return;

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        playerController.OnWield(actionType);

        if (!Physics.Raycast(ray, out hit, actionRange, actionLayer))
            return;

        switch (actionType)
        {
            case ActionType.Attack:
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                    target.OnDamage(attackDamage);
                break;


            case ActionType.Mine:
                IMineable mineral = hit.collider.GetComponent<IMineable>();
                if (mineral != null)
                    mineral.OnMine(miningPower);
                break;
        }
    }

}