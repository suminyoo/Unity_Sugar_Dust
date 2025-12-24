using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Stats")]
    public float weight;
    public int basePrice;
    public bool isStackable;
    public int maxStackAmount = 99;

    [Header("World Object")]
    public GameObject dropPrefab;
}