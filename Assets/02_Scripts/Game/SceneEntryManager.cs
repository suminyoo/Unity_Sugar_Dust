using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SceneEntryManager : MonoBehaviour
{
    private void Start()
    {
        if (SceneController.Instance == null) return;

        SPAWN_ID targetID = SceneController.Instance.targetSpawnPointID;

        // 타겟 스폰 포인트 찾기
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
        SpawnPoint targetPoint = null;

        foreach (var p in points)
        {
            if (p.spawnID == targetID)
            {
                targetPoint = p;
                break;
            }
        }

        if (targetPoint != null)
        {
            StartCoroutine(MovePlayerSafely(targetPoint));
        }
        else
        {
            Debug.LogWarning("목표 스폰 포인트를 찾지 못함");
        }
    }

    private IEnumerator MovePlayerSafely(SpawnPoint targetPoint)
    {
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

            if (cc != null) cc.enabled = false;
            if (agent != null) agent.enabled = false;

            // 이동 및 회전
            player.transform.position = targetPoint.transform.position;
            player.transform.rotation = targetPoint.transform.rotation;

            yield return null;

            // 컴포넌트 다시 켜기
            if (cc != null) cc.enabled = true;
            if (agent != null)
            {
                agent.Warp(targetPoint.transform.position); // NavMesh라 Warp
                agent.enabled = true;
            }

            if (targetPoint.spawnID == SPAWN_ID.Town_Hospital)
            {
                var condition = player.GetComponent<PlayerCondition>();
                if (condition != null)
                {
                    // "일어나!" 신호 보내기 (체력은 이미 10으로 로드되어 있음)
                    // 이 함수가 OnRevive 이벤트를 발생시켜 컨트롤러를 리셋함
                    float reviveAmount = 10f;
                    condition.Revive(reviveAmount); // 10% 비율로 부활 (혹은 그냥 호출용)
                }
            }

            Debug.Log($"플레이어를 스폰 포인트 {targetPoint.spawnID}번으로 이동 완료");
        }
    }
}