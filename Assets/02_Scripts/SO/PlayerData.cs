using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "SO/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 5f;
    public float jumpForce = 5f;
    public float actionRotationSpeed = 20f;

    [Header("Weight Penalty")]
    public float heavySpeed = 3f;
    public float tooHeavySpeed = 1f;

    [Header("Inventory")]
    public int[] inventorySizes = { 4, 6, 8, 10, 12 };
    public int GetInventorySize(int level)
    {
        // 범위 벗어나면 마지막 사이즈로
        if (level >= inventorySizes.Length) return inventorySizes[inventorySizes.Length - 1];
        if (level < 0) return inventorySizes[0];
        return inventorySizes[level];
    }
}