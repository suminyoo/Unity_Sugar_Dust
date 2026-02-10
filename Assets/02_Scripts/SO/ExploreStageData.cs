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

public class ExploreStageData : ScriptableObject
{
    [Header("Stage Settings")]
    public string stageName = "초원 지대";

    [Header("Spawn Objects")]
    public GameObject groundObject;
    public List<SpawnInfo> mapObjects;
    public List<SpawnInfo> mineralObjects;
    public List<SpawnInfo> enemyObjects;

}