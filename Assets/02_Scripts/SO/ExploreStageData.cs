using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "Explore/Explore Stage Data")]
public class ExploreStageData : ScriptableObject
{
    [Header("Stage Settings")]
    public string stageName = "초원 지대";

    [Header("Spawn Objects")]
    public List<ExploreObjectData> mapObjects;
    public List<ExploreObjectData> mineralObjects;
    public List<ExploreObjectData> enemyObjects;
}