using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;

public class GridMapSpawner : MonoBehaviour
{
    public event Action OnMapGenerationComplete;

    [Header("Map Settings")]
    public Vector2Int mapSize = new Vector2Int(20, 20);
    public float cellSize = 3.0f;

    [Header("Ground Settings")]
    public GameObject groundPrefab;

    [Header("NavMesh")]
    public NavMeshSurface navSurface;

    [Header("Objects to Spawn")]
    public List<ExploreObjectData> mapObjects;     // 배치할 맵요소 (장애물 광물 등)
    public List<ExploreObjectData> enemyObjects;    // 배치할 적들

    [Header("UI & System")]
    public GameObject loadingPanel;
    public Text statusText;         // 맵 생성 중 표시 텍스트
    public PlayerController player;       // 플레이어 (로딩 중엔 움직임 막기 위해)

    private List<Vector2Int> allCoordinates;

    // 맵의 상태 저장하는 2차원 배열 (이미 오브젝트 배치되어있을경우 true)
    private bool[,] gridMap;

    private IEnumerator Start()
    {
        // 로딩 시작 
        loadingPanel.SetActive(true);
        statusText.text = "맵 로딩중...";

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        player.Wait();

        InitializeMap();

        yield return new WaitForSeconds(0.5f);  //연출용 지연

        // ---맵 생성---
        statusText.text = "기반암 생성 중...";
        GenerateGround();
        yield return null;    //땅

        statusText.text = "오브젝트 배치 중...";
        // ---맵 오브젝트 배치---
        foreach (var objData in mapObjects)
        {
            yield return null;
            SpawnObject(objData);
        }
        yield return null; //각 오브젝트


        // ---런타임 navmesh 베이크---
        statusText.text = "지형 분석 및 경로 생성 중...";
        navSurface.RemoveData();
        navSurface.BuildNavMesh();
        yield return null;     // nav mesh 베이크


        //---적 배치---
        statusText.text = "몬스터 배치 중...";
        foreach (var objData in enemyObjects)
        {
            yield return null;
            SpawnObject(objData);
        }
        yield return null; // 적

        // ---로딩 완료---
        if (statusText != null) statusText.text = "준비 완료!";
        yield return new WaitForSeconds(1.0f); //로딩 완료 연출

        if (loadingPanel != null) loadingPanel.SetActive(false);
        player.WaitDone();


        OnMapGenerationComplete?.Invoke();
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
    void ShuffleCoordinates()
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
        int spawnedCount = 0;

        ShuffleCoordinates();

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