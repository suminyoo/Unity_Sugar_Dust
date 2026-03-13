using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Data", menuName = "Game/Item Data/Tool Data")]
public class ToolData : ItemData
{
    [Header("Basic Info")]
    public ActionType toolActionType;
    public GameObject toolPrefab;

    [Header("Stats")]
    public float power;
    public float range;
    public float cooldown;

    [Header("Critical")]
    [Range(0f, 1f)] public float criticalChance;
    public float criticalMultiplier;

    [Header("Sounds")]
    public SoundData actionSound;
}

public enum ActionType
{
    Attack,
    Mine
}
