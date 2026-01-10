using UnityEngine;

public enum RotationType
{
    Fixed,          //회전 불가
    Random90,       // 격자 맞춘 회전
    RandomFull,     // 풀회전 가능
    SlightJitter    // 자연스러운 회전
}

[CreateAssetMenu(fileName = "NewExploreObject", menuName = "Explore/Explore Object")]
public class ExploreObjectData : ScriptableObject
{
    [Header("Basic Info")]
    public string objectName;
    public GameObject prefab;

    [Header("Grid Setting")]
    public Vector2Int size = new Vector2Int(1, 1);

    [Header("Spawn Setting")]
    public int spawnCount = 5;

    [Header("Variation")]
    [Tooltip("회전 방식을 선택하세요")]
    public RotationType rotationType = RotationType.RandomFull;
}