using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PlayerSpawnHandler : MonoBehaviour
{
    public static PlayerSpawnHandler Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 씬 시작시 SceneController에 저장된 ID가 있다면 이동
        if (SceneController.Instance != null && SceneController.Instance.targetSpawnPointID != SPAWN_ID.NONE)
        {
            SpawnPlayer(SceneController.Instance.targetSpawnPointID);
        }

        //if (SceneController.Instance == null) return;

        //SPAWN_ID targetID = SceneController.Instance.targetSpawnPointID;

        //// 타겟 스폰 포인트 찾기
        //SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
        //SpawnPoint targetPoint = null;

        //foreach (var p in points)
        //{
        //    if (p.spawnID == targetID)
        //    {
        //        targetPoint = p;
        //        break;
        //    }
        //}

        //if (targetPoint != null)
        //{
        //    StartCoroutine(MovePlayerSafely(targetPoint));
        //}
        //else
        //{
        //    Debug.LogWarning("목표 스폰 포인트를 찾지 못함");
        //}
    }
    public void SpawnPlayer(SPAWN_ID targetID)
    {
        StartCoroutine(MovePlayerCor(targetID));
    }

    private IEnumerator MovePlayerCor(SPAWN_ID targetID)
    {
        // 씬 로드 직후 오브젝트들이 활성화될 시간을 줌
        yield return null;

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
        if (cc != null) cc.enabled = true;

        // 부활/상태 회복 로직 (필요 시)
        if (targetID == SPAWN_ID.HOSPITAL_BED)
        {
            player.GetComponent<PlayerCondition>()?.Revive(10f);
        }

        Debug.Log($"[PlayerSpawnHandler] 플레이어를 {targetID} 위치로 배치 완료.");
    }
    //private IEnumerator MovePlayerSafely(SpawnPoint targetPoint)
    //{
    //    yield return null;

    //    GameObject player = GameObject.FindGameObjectWithTag("Player");

    //    if (player != null)
    //    {
    //        CharacterController cc = player.GetComponent<CharacterController>();
    //        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

    //        if (cc != null) cc.enabled = false;
    //        if (agent != null) agent.enabled = false;

    //        // 이동 및 회전
    //        player.transform.position = targetPoint.transform.position;
    //        player.transform.rotation = targetPoint.transform.rotation;

    //        yield return null;

    //        // 컴포넌트 다시 켜기
    //        if (cc != null) cc.enabled = true;
    //        if (agent != null)
    //        {
    //            agent.Warp(targetPoint.transform.position); // NavMesh라 Warp
    //            agent.enabled = true;
    //        }

    //        if (targetPoint.spawnID == SPAWN_ID.TOWN_HOSPITAL)
    //        {
    //            var condition = player.GetComponent<PlayerCondition>();
    //            if (condition != null)
    //            {
    //                float reviveAmount = 10f;
    //                condition.Revive(reviveAmount); //체력 조금 회복 후 부활
    //            }
    //        }

    //        Debug.Log($"플레이어를 스폰 포인트 {targetPoint.spawnID}번으로 이동 완료");
    //    }
    //}
}