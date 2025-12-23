using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;             // 아이템 이름
    [TextArea] public string description; // 아이템 설명
    public Sprite icon;                 // 인벤토리 UI에 표시될 이미지

    [Header("Stats")]
    public float weight;                // 무게
    public int basePrice;               // 상점 판매 기본 가격
    public bool isStackable;            // 겹쳐지는 아이템인지 (예: 포션, 자원)
    public int maxStackAmount = 99;     // 최대 몇 개까지 겹쳐지는지

    [Header("World Object")]
    public GameObject dropPrefab;       // 바닥에 버렸을 때 생성될 프리팹
}