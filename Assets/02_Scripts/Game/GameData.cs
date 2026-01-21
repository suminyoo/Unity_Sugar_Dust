using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // PlayerCondition 데이터
    public float currentHp = 100f;
    public float currentStamina = 50f;

    // 탐사
    public int explorationLevel = 1;

    // 자산
    public int money;
    public List<string> ownedKeyItems = new List<string>(); // 특수 아이템(업그레이드 등)

    // 플레이어 인벤토리
    public int inventorySize;
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    //상점 진열대
    public int displayStandSize; 
    public List<InventorySlot> displayStandSlots = new List<InventorySlot>();
    public List<int> displayStandPrices = new List<int>(); //진열대 별 가격

    // 씬 내 스토리지(진열대나 상자 등 여러개가 존재하는 스토리지) 데이터 저장소
    // 키는 스트링으로 고유 아이디 값은 itemslotlist
    public Dictionary<string, List<InventorySlot>> worldStorageData = new Dictionary<string, List<InventorySlot>>();



    // 초기화용 (새 게임 시작 시)
    public void InitNewGame(float maxHp, float maxStamina, int invSize, int dsSize)
    {
        currentHp = maxHp;
        currentStamina = maxStamina;

        explorationLevel = 1;

        money = 1000; //돈 초기 금액

        inventorySize = invSize;
        inventorySlots.Clear();
        inventorySlots = new List<InventorySlot>();
        for (int i = 0; i < invSize; i++) inventorySlots.Add(new InventorySlot());

        displayStandSize = dsSize;
        displayStandSlots.Clear();
        displayStandPrices.Clear();
        displayStandSlots = new List<InventorySlot>();
        for (int i = 0; i < dsSize; i++) displayStandSlots.Add(new InventorySlot());


        //if (worldStorageData != null) worldStorageData.Clear();
        //else worldStorageData = new Dictionary<string, List<InventorySlot>>();
    }
}