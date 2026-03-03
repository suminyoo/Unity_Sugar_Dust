using UnityEngine;

[CreateAssetMenu(fileName = "NewToolData", menuName = "Game/Tool Data")]
public class ToolData : ScriptableObject
{
    [Header("Basic Info")]
    public string toolName;
    public GameObject toolPrefab;

    [Header("Stats")]
    public float power;
    public float range;
    public float cooldown;

    [Header("Critical")]
    [Range(0f, 1f)] public float criticalChance;
    public float criticalMultiplier;

    [Header("Sounds")]
    public SoundData equipSound;
    public SoundData actionSound;

}