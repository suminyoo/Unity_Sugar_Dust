using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // 속도 세팅
    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    // 목적지로 이동
    public void MoveTo(Vector3 destination)
    {
        // 이동 가능 상태로 변경
        if (agent.isStopped)
            agent.isStopped = false;

        agent.SetDestination(destination);
    }

    // 그 자리에 멈춤 (대화 시작 시)
    public void Stop()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    // 다시 이동 재개 (대화 끝날 때)
    public void Resume()
    {
        if (!agent.isOnNavMesh) return;

        agent.isStopped = false; // 다시 경로 따라감
    }

    // 목적지에 도착했는지
    public bool HasArrived()
    {
        // 경로 계산 중이면 아직 도착 안 함
        if (agent.pathPending) return false;

        // 거리계산 도착판단
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }

        return false;
    }

    public float GetCurrentSpeed()
    {
        if (agent == null) return 0f;
        return agent.velocity.magnitude; // 속력
    }

    public void LookAtTarget(Transform target)
    {
        if (target == null) return;

        // 대상 방향 계산
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        // 회전 (현재는 즉시 회전)
        // TODO: coroutine과 slerp으로 부드러운 회전 가능
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}