using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "Explore/Explore Stage Data")]

[Serializable]
public class SpawnInfo
{
    public ExploreObjectData objectData;
    public int spawnCount;
}

[System.Serializable]
public struct DynamicSpawnInfo
{
    public ExploreObjectData objectData;

    [Tooltip("X축: 레벨(1~15), Y축: 생성 개수")]
    public AnimationCurve spawnRateCurve;
}

public class ExploreStageData : ScriptableObject
{
    [Header("Stage Settings")]
    public string stageName = "초원 지대";

    [Header("Spawn Objects")]
    public GameObject groundObject;
    public List<SpawnInfo> mapObjects;
    public List<DynamicSpawnInfo> mineralObjects;
    public List<DynamicSpawnInfo> enemyObjects;

}