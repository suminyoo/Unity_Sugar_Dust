using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "SO/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Health")]
    public float maxHp = 100f;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 5f;
    public float jumpForce = 5f;
    public float actionRotationSpeed = 20f;

    [Header("Weight Penalty")]
    public float heavySpeed = 3f;
    public float tooHeavySpeed = 1f;
}