using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;

public class GridMapSpawner : MonoBehaviour
{
    public event Action OnMapGenerationComplete;

    [Header("Map Settings")]
    public Vector2Int mapSize = new Vector2Int(20, 20);
    public float cellSize = 3.0f;
    public GameObject groundPrefab;
    public NavMeshSurface navSurface;

    [Header("Objects")]
    private List<ExploreObjectData> currentMapObjects;
    private List<ExploreObjectData> currentMineralObjects;
    private List<ExploreObjectData> currentEnemyObjects;

    public Transform landingSpotSpawnPoint;
    public GameObject landingSpotPrefab;
    public GameObject defaultLandingSpotPrefab;

    private PlayerController playerRef;

    [Header("UI")]
    public GameObject loadingPanel;
    public TextMeshProUGUI statusText;  // 맵 생성 중 표시 텍스트

    private List<Vector2Int> allCoordinates;

    // 맵의 상태 저장하는 2차원 배열 (이미 오브젝트 배치되어있을경우 true)
    private bool[,] gridMap;


    public void InitAndGenerateMap(ExploreStageData stageData, int currentLevel, PlayerController player)
    {
        this.currentMapObjects = stageData.mapObjects;
        this.currentMineralObjects = stageData.mineralObjects;
        this.currentEnemyObjects = stageData.enemyObjects;

        this.playerRef = player;

        CleanupMap();
        StopAllCoroutines();
        StartCoroutine(GenerateMapRoutine(currentLevel));
    }

    private IEnumerator GenerateMapRoutine(int currentLevel)
    {
        yield return null;

        // 로딩 시작 
        loadingPanel.SetActive(true);
        statusText.text = "맵 로딩중...";

        //InputControlManager.Instance.LockInput();

        InitializeMap();

        yield return new WaitForSeconds(0.5f);  //연출용 지연

        // ---맵 생성---
        GenerateGround();
        yield return null;

        // --- 착륙장 배치 ---
        SpawnExitObject(currentLevel);
        yield return null;

        // ---맵 오브젝트 배치---
        if (currentMapObjects != null)
        {
            foreach (var objData in currentMapObjects)
            {
                SpawnObject(objData);
                yield return null;
            }
        }

        // --- 광물 배치 ---
        if (currentMineralObjects != null)
        {
            foreach (var objData in currentMineralObjects)
            {
                SpawnObject(objData);
                yield return null;
            }
        }

        // ---런타임 navmesh 베이크---
        navSurface.RemoveData();
        navSurface.BuildNavMesh();
        yield return null;     // nav mesh 베이크


        //---적 배치---
        if (currentEnemyObjects != null)
        {
            foreach (var objData in currentEnemyObjects)
            {
                SpawnObject(objData);
                yield return null;
            }
        }

        // ---로딩 완료---
        OnMapGenerationComplete?.Invoke();

        if (statusText != null) statusText.text = "탐사 준비 완료!";
        yield return new WaitForSeconds(1.0f); //로딩 완료 연출

        if (loadingPanel != null) loadingPanel.SetActive(false);
        //InputControlManager.Instance.UnlockInput(); //매니저에서 하도록

    }

    // 착륙장 배치
    private void SpawnExitObject(int level)
    {
        GameObject prefabToSpawn = (level == 1 || level % 5 == 0) ? landingSpotPrefab : defaultLandingSpotPrefab;
        if (prefabToSpawn == null) return;

        GameObject instance = Instantiate(prefabToSpawn, landingSpotSpawnPoint.position, prefabToSpawn.transform.rotation);
        instance.transform.SetParent(this.transform);
    }

    void CleanupMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    // 맵 초기화
    void InitializeMap()
    {
        gridMap = new bool[mapSize.x, mapSize.y];

        allCoordinates = new List<Vector2Int>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allCoordinates.Add(new Vector2Int(x, y));
            }
        }
    }

    // FisherYates  리스트 랜덤섞기
    void MixCoordinates()
    {
        for (int i = 0; i < allCoordinates.Count; i++)
        {
            Vector2Int temp = allCoordinates[i];
            int randomIndex = UnityEngine.Random.Range(i, allCoordinates.Count);
            allCoordinates[i] = allCoordinates[randomIndex];
            allCoordinates[randomIndex] = temp;
        }
    }

    void GenerateGround()
    {
        if (groundPrefab == null) return;

        // 생성
        GameObject ground = Instantiate(groundPrefab, transform);
        ground.name = "Exploration_Ground";

        // 레이어설정 
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex != -1)
            ground.layer = groundLayerIndex;

        // 스케일 계산 
        float totalWidth = mapSize.x * cellSize;  // 가로
        float totalHeight = mapSize.y * cellSize; // 세로

        ground.transform.localScale = new Vector3(totalWidth / 10f, 1f, totalHeight / 10f); // 10으로 나눠야 정확한 스케일

        // 위치 보정
        // Grid는 (0,0)에서 ground Pivot은 중앙 (0.5)만큼
        ground.transform.localPosition = new Vector3(totalWidth * 0.5f, 0f, totalHeight * 0.5f);
    }

    void SpawnObject(ExploreObjectData data)
    {
        if (data.prefab == null) return;
        int spawnedCount = 0;

        MixCoordinates();

        foreach (Vector2Int cor in allCoordinates)
        {
            if (spawnedCount >= data.spawnCount) break;

            int x = cor.x;
            int y = cor.y;

            if (x + data.size.x > mapSize.x || y + data.size.y > mapSize.y)
                continue;

            if (CheckArea(x, y, data.size.x, data.size.y))
            {
                PlaceObject(x, y, data);
                spawnedCount++;
            }
        }
        if (spawnedCount < data.spawnCount)
        {
            Debug.Log($"공간이 부족하여 {data.objectName} {data.spawnCount - spawnedCount}개를 배치하지 못했습니다.");
        }
    

        // 영역 비었는지 확인
        bool CheckArea(int startX, int startY, int sizeX, int sizeY)
        {
            for (int x = startX; x < startX + sizeX; x++)
            {
                for (int y = startY; y < startY + sizeY; y++)
                {
                    if (gridMap[x, y]) return false;
                }
            }
            return true;
        }

        void PlaceObject(int x, int y, ExploreObjectData data)
        {
            for (int i = x; i < x + data.size.x; i++)
            {
                for (int j = y; j < y + data.size.y; j++)
                {
                    gridMap[i, j] = true;
                }
            }

            // 위치 (중앙정렬)
            float worldX = x * cellSize;
            float worldZ = y * cellSize;
            float offsetX = data.size.x * cellSize * 0.5f;
            float offsetZ = data.size.y * cellSize * 0.5f;

            Vector3 finalPos = new Vector3(worldX + offsetX, 0, worldZ + offsetZ) + transform.position;

            //회전
            Quaternion finalRot = Quaternion.identity;

            switch (data.rotationType)
            {
                case RotationType.Fixed: finalRot = Quaternion.identity; break;
                case RotationType.Random90: finalRot = Quaternion.Euler(0, UnityEngine.Random.Range(0, 4) * 90f, 0); break;
                case RotationType.RandomFull: finalRot = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0); break;
                case RotationType.SlightJitter: finalRot = Quaternion.Euler(0, UnityEngine.Random.Range(-20f, 20f), 0); break;
            }

            // 생성
            GameObject go = Instantiate(data.prefab, finalPos, finalRot);
            go.transform.SetParent(this.transform);
            go.name = $"{data.objectName}_[{x},{y}]";

            // 적이면 플레이어 정보 넘기면서 셋업
            Enemy enemyScript = go.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.Setup(this.playerRef);
            }
        }
    }

    // 디버그 에디터 그리드 확인용
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 startPos = transform.position;

        // 전체 맵 테두리
        float totalW = mapSize.x * cellSize;
        float totalH = mapSize.y * cellSize;

        // 바닥 격자
        for (int x = 0; x <= mapSize.x; x++)
        {
            Gizmos.DrawLine(startPos + new Vector3(x * cellSize, 0, 0), startPos + new Vector3(x * cellSize, 0, totalH));
        }
        for (int y = 0; y <= mapSize.y; y++)
        {
            Gizmos.DrawLine(startPos + new Vector3(0, 0, y * cellSize), startPos + new Vector3(totalW, 0, y * cellSize));
        }
    }
}