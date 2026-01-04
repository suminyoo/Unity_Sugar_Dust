using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // PlayerCondition 데이터
    public float currentHp = 100f;
    public float currentStamina = 50f;
    public int inventorySize; //인벤토리크기
    public int displayStandSize; // 가판대 크기


    public List<InventorySlot> inventorySlots = new List<InventorySlot>();     // Inventory 데이터
    public List<InventorySlot> displayStandSlots = new List<InventorySlot>();     // display stand 데이터


    // 씬 내 스토리지(진열대나 상자 등 여러개가 존재하는 스토리지) 데이터 저장소
    // 키는 스트링으로 고유 아이디 값은 itemslotlist
    public Dictionary<string, List<InventorySlot>> worldStorageData = new Dictionary<string, List<InventorySlot>>();

    // 초기화용 (새 게임 시작 시)
    public void InitNewGame(float maxHp, float maxStamina, int invSize, int dsSize)
    {
        currentHp = maxHp;
        currentStamina = maxStamina;

        inventorySize = invSize;
        displayStandSize = dsSize;


        inventorySlots = new List<InventorySlot>();
        displayStandSlots = new List<InventorySlot>();

        for (int i = 0; i < invSize; i++) inventorySlots.Add(new InventorySlot());
        for (int i = 0; i < dsSize; i++) displayStandSlots.Add(new InventorySlot());

        //if (worldStorageData != null) worldStorageData.Clear();
        //else worldStorageData = new Dictionary<string, List<InventorySlot>>();
    }
}