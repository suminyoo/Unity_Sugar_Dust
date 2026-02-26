using System;
using System.Collections.Generic;
using UnityEngine;


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

    [Tooltip("X축: 레벨, Y축: 생성 개수")]
    public AnimationCurve spawnRateCurve;
}

[CreateAssetMenu(fileName = "NewStageData", menuName = "Explore/Explore Stage Data")]
public class ExploreStageData : ScriptableObject
{
    [Header("Stage Settings")]
    public string stageName;

    [Header("Spawn Objects")]
    //public GameObject groundObject;
    public List<SpawnInfo> mapObjects;
    public List<DynamicSpawnInfo> mineralObjects;
    public List<DynamicSpawnInfo> enemyObjects;

}