using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // PlayerCondition 데이터
    public float currentHp;
    public float currentStamina;
    public int inventoryLevel = 0; //인벤토리크기

    // Inventory 데이터
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    // 초기화용 (새 게임 시작 시)
    public void InitNewGame(float maxHp, float maxStamina, int invSize)
    {
        currentHp = maxHp;
        currentStamina = maxStamina;
        inventorySlots = new List<InventorySlot>();
        for (int i = 0; i < invSize; i++) inventorySlots.Add(new InventorySlot());
    }
}