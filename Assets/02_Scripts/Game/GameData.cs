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
    public float playTime; // 누적 플레이 타임

    public int inGameDay;
    public GAME_TIME inGameTime;
}

[System.Serializable]
public class GameData
{
    //게임 데이터
    public SaveMetadata metadata = new SaveMetadata();
    public int currentDay = 1;
    public GAME_TIME currentTime = GAME_TIME.Morning;

    // 레벨 데이터
    public int hpLevel = 0;
    public int staminaLevel = 0;
    public int inventorySizeLevel = 0;
    public int displayStandSizeLevel = 0;

    // 플레이어 
    public float currentHp;
    public float currentStamina;

    // 탐사
    public int exploreMaxUnlockedLevel = 1;

    // 자산
    public int money;
    public List<string> ownedKeyItems = new List<string>(); // 특수 아이템(업그레이드 등)

    // 플레이어 인벤토리
    public List<ItemSaveData> inventorySlots = new List<ItemSaveData>();

    //상점 진열대
    public List<ItemSaveData> displayStandSlots = new List<ItemSaveData>();
    public List<int> displayStandPrices = new List<int>(); //진열대 별 가격

    // 씬 내 스토리지(진열대나 상자 등 여러개가 존재하는 스토리지) 데이터 저장소
    // 키는 스트링으로 고유 아이디 값은 itemslotlist
    public Dictionary<string, List<InventorySlot>> worldStorageData = new Dictionary<string, List<InventorySlot>>();


    public void InitNewGame(PlayerData blueprint)
    {
        metadata = new SaveMetadata();
        metadata.playTime = 0;

        currentDay = 1;
        currentTime = GAME_TIME.Morning;

        hpLevel = 0;
        staminaLevel = 0;
        inventorySizeLevel = 0;
        displayStandSizeLevel = 0;

        currentHp = blueprint.GetMaxHpValue(0);
        currentStamina = blueprint.GetMaxStaminaValue(0);

        exploreMaxUnlockedLevel = 1;

        money = 1000; //돈 초기 금액(수정가능)

        int invSize = blueprint.GetInventorySize(this.inventorySizeLevel);
        inventorySlots = new List<ItemSaveData>();
        for (int i = 0; i < invSize; i++) inventorySlots.Add(new ItemSaveData());

        int dsSize = blueprint.GetDisplayStandSize(this.displayStandSizeLevel);
        displayStandSlots = new List<ItemSaveData>();
        for (int i = 0; i < dsSize; i++) displayStandSlots.Add(new ItemSaveData());

        //if (worldStorageData != null) worldStorageData.Clear();
        //else worldStorageData = new Dictionary<string, List<InventorySlot>>();
    }
}