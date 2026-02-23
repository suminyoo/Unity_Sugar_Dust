using System.Collections.Generic;

[System.Serializable]
public class ItemSaveData
{
    public string itemID;
    public int amount;
}

[System.Serializable]
public class SaveMetadata
{
    public string saveTime; // 현실 저장 시간
    public GAME_TIME inGameTime; // 인게임 시간
    public float playTime; // 누적 플레이 타임
}

[System.Serializable]
public class GameData
{
    public SaveMetadata metadata = new SaveMetadata();

    // PlayerCondition 데이터
    public float currentHp = 100f;
    public float currentStamina = 50f;

    // 탐사
    public int exploreMaxUnlockedLevel = 1;

    // 자산
    public int money;
    public List<string> ownedKeyItems = new List<string>(); // 특수 아이템(업그레이드 등)

    // 플레이어 인벤토리
    public int inventorySizeLevel;
    public List<ItemSaveData> inventorySlots = new List<ItemSaveData>();

    //상점 진열대
    public int displayStandSizeLevel; 
    public List<ItemSaveData> displayStandSlots = new List<ItemSaveData>();
    public List<int> displayStandPrices = new List<int>(); //진열대 별 가격

    // 씬 내 스토리지(진열대나 상자 등 여러개가 존재하는 스토리지) 데이터 저장소
    // 키는 스트링으로 고유 아이디 값은 itemslotlist
    public Dictionary<string, List<InventorySlot>> worldStorageData = new Dictionary<string, List<InventorySlot>>();



    // 초기화용 (새 게임 시작 시)
    public void InitNewGame(float maxHp, float maxStamina, int invSize, int dsSize)
    {
        currentHp = maxHp;
        currentStamina = maxStamina;

        exploreMaxUnlockedLevel = 1;

        money = 1000; //돈 초기 금액

        inventorySizeLevel = invSize;
        inventorySlots.Clear();
        inventorySlots = new List<ItemSaveData>();
        for (int i = 0; i < invSize; i++) inventorySlots.Add(new ItemSaveData());

        displayStandSizeLevel = dsSize;
        displayStandSlots.Clear();
        displayStandPrices.Clear();
        displayStandSlots = new List<ItemSaveData>();
        for (int i = 0; i < dsSize; i++) displayStandSlots.Add(new ItemSaveData());


        //if (worldStorageData != null) worldStorageData.Clear();
        //else worldStorageData = new Dictionary<string, List<InventorySlot>>();
    }
}