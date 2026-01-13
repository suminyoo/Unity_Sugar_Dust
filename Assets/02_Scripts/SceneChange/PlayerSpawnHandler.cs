using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PlayerSpawnHandler : MonoBehaviour
{
    public static PlayerSpawnHandler Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    public void SpawnPlayer(SPAWN_ID targetID)
    {
        StopAllCoroutines();
        StartCoroutine(MovePlayerCor(targetID));
    }

    private IEnumerator MovePlayerCor(SPAWN_ID targetID)
    {
        // 물리 업데이트 대기
        yield return new WaitForFixedUpdate();

        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
        SpawnPoint targetPoint = System.Array.Find(points, p => p.spawnID == targetID);

        if (targetPoint == null)
        {
            Debug.LogWarning($"[PlayerSpawnHandler] {targetID} 스폰 포인트를 찾지 못했습니다.");
            yield break;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        // 컴포넌트 제어
        CharacterController cc = player.GetComponent<CharacterController>();
        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

        if (cc != null) cc.enabled = false;
        if (agent != null) agent.enabled = false;

        // 위치 이동
        player.transform.position = targetPoint.transform.position;
        player.transform.rotation = targetPoint.transform.rotation;

        // NavMeshAgent 특수 처리
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(targetPoint.transform.position);
        }

        yield return new WaitForFixedUpdate();

        if (cc != null) cc.enabled = true;

        // 병원 침대시 부활/상태 회복
        if (targetID == SPAWN_ID.HOSPITAL_BED)
        {
            player.GetComponent<PlayerCondition>()?.Revive(10f);
        }

        Debug.Log($"[PlayerSpawnHandler] 플레이어를 {targetID} 위치로 배치 완료.");

    }
}

