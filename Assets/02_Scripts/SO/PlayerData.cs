using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 5f;
    public float jumpForce = 5f;
    public float actionRotationSpeed = 20f;

    [Header("Weight")]
    public float heavySpeed = 3f;
    public float tooHeavySpeed = 1f;

    [Header("Stats")]
    public float maxHp = 100f;
    public float maxStamina = 50f;

    [Header("Storages")]
    public int[] inventorySizes = { 4, 6, 8, 10, 12 };
    public int[] displayStandSizes = { 2, 3, 4, 6, 8 };

    public int GetInventorySize(int level)
    {
        // 범위 벗어나면 마지막 사이즈로
        if (level >= inventorySizes.Length) return inventorySizes[inventorySizes.Length - 1];
        if (level < 0) return inventorySizes[0];
        return inventorySizes[level];
    }

    public int GetDisplayStandSize(int level)
    {
        // 범위 벗어나면 마지막 사이즈로
        if (level >= displayStandSizes.Length) return displayStandSizes[displayStandSizes.Length - 1];
        if (level < 0) return displayStandSizes[0];
        return displayStandSizes[level];
    }
}