using UnityEngine;

public interface IMineable { void OnMine(float power); }
public interface IDamageable { void OnDamage(float damage); }

public class ActionSystem : MonoBehaviour
{
    [Header("Settings")]
    public Transform firePoint;
    public float actionRange = 3.5f;
    public LayerMask actionLayer;

    [Header("Stats")]
    public float attackDamage = 10f;
    public float miningPower = 20f;

    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        DrawDebugRay(); //디버그용

        if (Input.GetMouseButtonDown(0)) TryAction(true);
        if (Input.GetMouseButtonDown(1)) TryAction(false);
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

    void TryAction(bool isWield)
    {
        if (firePoint == null) return;

        if (playerController != null)
        {
            playerController.OnWield();
        }

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, actionRange, actionLayer))
        {
            if (isWield)
            {
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.OnDamage(attackDamage);
                    Debug.Log("공격 적중");
                }
            }
            else
            {
                IMineable mineral = hit.collider.GetComponent<IMineable>();
                if (mineral != null)
                {
                    mineral.OnMine(miningPower);
                    Debug.Log("광질 성공");
                }
            }
        }
    }
}