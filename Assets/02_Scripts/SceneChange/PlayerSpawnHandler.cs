using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PlayerSpawnHandler : MonoBehaviour
{
    public static PlayerSpawnHandler Instance;

    private string playerModelName = "player_model";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    public Coroutine SpawnPlayer(SPAWN_ID targetID)
    {
        StopAllCoroutines();
        return StartCoroutine(MovePlayerCor(targetID));
    }

    private IEnumerator MovePlayerCor(SPAWN_ID targetID)
    {
        // 물리 업데이트 대기
        yield return new WaitForFixedUpdate();

        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
        SpawnPoint targetPoint = System.Array.Find(points, p => p.spawnID == targetID);

        if (targetPoint == null)
        {
            Debug.LogWarning($"{targetID} 스폰 포인트를 찾지 못했습니다");
            yield break;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        // 컴포넌트 제어
        CharacterController controller = player.GetComponent<CharacterController>();
        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

        if (controller != null) controller.enabled = false;
        if (agent != null) agent.enabled = false;

        // 위치 이동
        player.transform.position = targetPoint.transform.position;
        player.transform.rotation = targetPoint.transform.rotation;

        Transform modelTr = player.transform.Find(playerModelName);
        if (modelTr != null)
        {
            modelTr.localRotation = Quaternion.identity;
        }

        yield return new WaitForFixedUpdate();

        // NavMeshAgent용 처리
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(targetPoint.transform.position);
        }
        if (controller != null) controller.enabled = true;


        //카메라 이동
        CameraFollow cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
        cam.SnapToTarget();
        

        // 병원 침대시 부활/상태 회복
        if (targetID == SPAWN_ID.HOSPITAL_BED)
        {
            player.GetComponent<PlayerCondition>()?.Revive(10f);
        }

        Debug.Log($"플레이어를 {targetID} 위치로 배치 완료");

    }
}

