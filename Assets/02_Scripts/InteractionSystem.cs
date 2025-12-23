using UnityEngine;

public interface IMineable { void OnMine(float power); }
public interface IDamageable { void OnDamage(float damage); }

public class InteractionSystem : MonoBehaviour
{
    [Header("Settings")]
    public Transform firePoint;             
    public float interactionRange = 3.5f; 
    public LayerMask interactionLayer; //상호작용 가능한 오브젝트에 붙히는레이어

    [Header("Stats")]
    public float attackDamage = 10f;
    public float miningPower = 20f;

    [Header("References")]
    public Animator animator;

    void Update()
    {
        DrawDebugRay(); //디버그 레이

        if (Input.GetMouseButtonDown(0)) TryAction(true);
        if (Input.GetMouseButtonDown(1)) TryAction(false);
    }

    void DrawDebugRay()
    {
        if (firePoint == null) return;

        // 레이 생성: 발사 지점 위치에서, 발사 지점의 정면(forward) 방향으로
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red);
        }
    }

    void TryAction(bool isAttack)
    {
        //if (animator != null) animator.SetTrigger(isAttack ? "Attack" : "Mining");
        if (firePoint == null) return;

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            if (isAttack)
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