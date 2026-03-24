using System.Collections.Generic;


public enum GuideType { None, Town, Explore, Shop }

[System.Serializable]
public class QuestSaveData
{
    public string questID;
    public int[] currentAmounts; // 각 목표별 현재 진행도
    public bool isRewardClaimed; // 보상수령 여부
}
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

    public bool isTutorialCompleted = false;
    public List<GuideType> viewedGuides = new List<GuideType>();

    public int currentDay = 0;
    public GAME_TIME currentTime = GAME_TIME.None;

    // 레벨 데이터
    public int hpLevel = 0;
    public int staminaLevel = 0;
    public int inventorySizeLevel = 0;
    public int equipmentSizeLevel = 0;
    public int displayStandSizeLevel = 0;
    public int containerSizeLevel = 0;

    // 플레이어 
    public float currentHp;
    public float currentStamina;

    // 탐사
    public int exploreMaxUnlockedLevel = 0;

    // 자산
    public int money;
    public List<string> ownedKeyItems = new List<string>(); // 특수 아이템(업그레이드 등)

    // 플레이어 인벤토리
    public List<ItemSaveData> inventorySlots = new List<ItemSaveData>();

    //플레이어 장착 슬롯
    public List<ItemSaveData> equipmentSlots = new List<ItemSaveData>();

    //상점 진열대
    public List<ItemSaveData> displayStandSlots = new List<ItemSaveData>();
    public List<int> displayStandPrices = new List<int>(); //진열대 별 가격

    // 컨테이너 박스
    public List<ItemSaveData> containerSlots = new List<ItemSaveData>();

    //퀘스트
    public List<string> completedQuestIDs = new List<string>(); // 완료한 퀘스트id
    public List<QuestSaveData> activeQuests = new List<QuestSaveData>(); // 진행중인 퀘스트 정보

    public void InitNewGame(PlayerData blueprint)
    {
        metadata = new SaveMetadata();
        metadata.playTime = 0;

        currentDay = 0;
        currentTime = GAME_TIME.Morning;

        isTutorialCompleted = false;
        viewedGuides = new List<GuideType>();

        hpLevel = 0;
        staminaLevel = 0;
        inventorySizeLevel = 0;
        equipmentSizeLevel = 0;
        displayStandSizeLevel = 0;
        containerSizeLevel = 0;

        currentHp = blueprint.GetMaxHpValue(0);
        currentStamina = blueprint.GetMaxStaminaValue(0);

        exploreMaxUnlockedLevel = 0;

        money = 0;

        int invSize = blueprint.GetInventorySize(this.inventorySizeLevel);
        inventorySlots = new List<ItemSaveData>();
        for (int i = 0; i < invSize; i++) inventorySlots.Add(new ItemSaveData());

        int eqipSize = blueprint.GetEquipmentSize(this.equipmentSizeLevel);
        equipmentSlots = new List<ItemSaveData>();
        for (int i = 0; i < eqipSize; i++) equipmentSlots.Add(new ItemSaveData());

        int dsSize = blueprint.GetDisplayStandSize(this.displayStandSizeLevel);
        displayStandSlots = new List<ItemSaveData>();
        for (int i = 0; i < dsSize; i++) displayStandSlots.Add(new ItemSaveData());

        int cbSize = blueprint.GetContainerBoxSize(this.containerSizeLevel);
        containerSlots = new List<ItemSaveData>();
        for (int i = 0; i < cbSize; i++) containerSlots.Add(new ItemSaveData());

    }
}