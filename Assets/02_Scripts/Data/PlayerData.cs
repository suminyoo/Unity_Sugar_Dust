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
    public float[] hpLevels = { 100f, 120f, 150f, 200f };
    public float[] staminaLevels = { 50f, 80f, 100f, 120f };

    [Header("Storages")]
    public int[] inventorySizes = { 6, 8, 10, 12 };
    public int[] displayStandSizes = { 3, 5, 7, 9 };
    public int[] containerBoxSizes = { 30, 50, 70, 90 };

    [Header("Sounds")]
    public SoundData hitSound;
    public SoundData deathSound;
    public SoundData exhaustedSound;

    public float GetMaxHpValue(int level) => GetValueSafe(hpLevels, level);
    public float GetMaxStaminaValue(int level) => GetValueSafe(staminaLevels, level);
    public int GetInventorySize(int level) => (int)GetValueSafe(inventorySizes, level);
    public int GetDisplayStandSize(int level) => (int)GetValueSafe(displayStandSizes, level);
    public int GetContainerBoxSize(int level) => (int)GetValueSafe(containerBoxSizes, level);

    private float GetValueSafe(System.Array array, int level)
    {
        if (array == null || array.Length == 0) return 0;
        int index = Mathf.Clamp(level, 0, array.Length - 1);
        return (float)System.Convert.ToDouble(array.GetValue(index));
    }

    public bool IsMaxHpLevel(int level) => IsMaxLevelSafe(hpLevels, level);
    public bool IsMaxStaminaLevel(int level) => IsMaxLevelSafe(staminaLevels, level);
    public bool IsMaxInventoryLevel(int level) => IsMaxLevelSafe(inventorySizes, level);
    public bool IsMaxDisplayStandLevel(int level) => IsMaxLevelSafe(displayStandSizes, level);
    public bool IsMaxContainerBoxLevel(int level) => IsMaxLevelSafe(containerBoxSizes, level);

    // 배열을 받아 안전하게 끝인지 검사하는 공통 로직
    private bool IsMaxLevelSafe(System.Array array, int currentLevel)
    {
        if (array == null || array.Length == 0) return true;
        return currentLevel >= array.Length - 1;
    }
}