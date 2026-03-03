using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public interface ISaveable
{
    void SaveData();// 각 클래스가 자기 데이터를 GameManager에 어떻게 저장할지 스스로 정의
}

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance;

    public PlayerData defaultPlayerData;

    public int currentSaveSlot = 1;
    private int selectedExploreLevel;

    public bool isTimerActive = false;

    // 씬이 넘어가도 살아있는 데이터 보관함
    public GameData savedData = new GameData();

    public SoundData writingSound;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Update()
    {
        if (isTimerActive && savedData != null && savedData.metadata != null)
        {
            savedData.metadata.playTime += Time.deltaTime;
        }
    }

    public void SetTimerActive(bool active)
    {
        isTimerActive = active;
    }

    public void InitData()
    {
        // 게임 시작시 초기화
        savedData.InitNewGame(defaultPlayerData);
    }

    #region 게임 데이터 슬롯에 저장

    public void SaveCurrentGame()
    {
        if (writingSound.clip == null) SoundManager.Instance.PlaySFX2D(writingSound);

        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
        foreach (var saveable in saveables)
        {
            saveable.SaveData();
        }

        savedData.metadata.saveTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm");

        // 현재 플레이 중인 슬롯의 폴더 경로 설정 (Saves/Slot1 등)
        string directoryPath = Path.Combine(Application.persistentDataPath, $"Saves/Slot{currentSaveSlot}");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 밀어내기 로직 실행 (5개
        ManageRollingSaves(directoryPath, 5);

        // 새 파일 이름 생성
        string fileName = $"Save_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.json";
        string filePath = Path.Combine(directoryPath, fileName);

        // 데이터를 JSON으로
        string json = JsonUtility.ToJson(savedData, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"[{currentSaveSlot}번 슬롯] 세이브 완료! 경로: {filePath}");
    }

    private void ManageRollingSaves(string directoryPath, int maxFiles)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
        FileInfo[] files = dirInfo.GetFiles("*.json");

        // 파일이 10개 이상
        if (files.Length >= maxFiles)
        {
            var sortedFiles = files.OrderBy(f => f.CreationTime).ToList();

            // 초과된 만큼 삭제
            int deleteCount = files.Length - maxFiles + 1;
            for (int i = 0; i < deleteCount; i++)
            {
                sortedFiles[i].Delete();
                Debug.Log($"[GameManager] 밀어내기: 오래된 세이브 파일 삭제됨 -> {sortedFiles[i].Name}");
            }
        }
    }

    #endregion

    // 씬 넘어가기 전에 플레이어의 상태를 매니저에 기록


    #region 게임 데이터 세이브로드
    public void SaveGameState(int day, GAME_TIME time)
    {
        savedData.currentDay = day;
        savedData.currentTime = time;

        savedData.metadata.inGameDay = day;
        savedData.metadata.inGameTime = time;
    }
    public (int day, GAME_TIME time) LoadGameState()
    {
        return (savedData.currentDay, savedData.currentTime);
    }

    #endregion

    #region 자산 데이터 세이브로드

    public void SavePlayerAssets(int money, HashSet<string> keyItems)
    {
        savedData.money = money;

        // HashSet -> List 변환 저장
        savedData.ownedKeyItems = new List<string>(keyItems);

        Debug.Log($"[GameManager] 자산 저장 완료: {money} Gold");
    }

    public (int money, List<string> keyItems) LoadPlayerAssets()
    {
        return (savedData.money, savedData.ownedKeyItems);
    }

    #endregion

    #region 플레이어 상태 데이터 세이브로드

    public void SavePlayerCondition(float hp, float stamina, int hpLevel, int staLevel)
    {
        savedData.currentHp = hp;
        savedData.currentStamina = stamina;
        savedData.hpLevel = hpLevel;
        savedData.staminaLevel = staLevel;

        Debug.Log("GameManager: 플레이어 컨디션 저장 완료");
    }

    public (float hp, float stamina, int hpLevel, int staminaLevel) LoadPlayerCondition()
    {
        return (savedData.currentHp, savedData.currentStamina, savedData.hpLevel, savedData.staminaLevel);
    }

    #endregion

    #region 플레이어 인벤토리 데이터 세이브로드

    public void SavePlayerInventory(IReadOnlyList<InventorySlot> slots)
    {
        savedData.inventorySlots.Clear();
        foreach (var slot in slots)
        {
            // 빈 슬롯이면 아이디를 비워둠
            string id = slot.IsEmpty ? "" : slot.ItemData.itemID; 
            savedData.inventorySlots.Add(new ItemSaveData { itemID = id, amount = slot.Amount });
        }
    }

    public (int size, List<InventorySlot> slots) LoadPlayerInventory()
    {
        List<InventorySlot> loadedSlots = new List<InventorySlot>();

        foreach (var savedSlot in savedData.inventorySlots)
        {
            if (string.IsNullOrEmpty(savedSlot.itemID))
            {
                // 빈 슬롯 복원
                loadedSlots.Add(new InventorySlot());
            }
            else
            {
                // ItemManager에게 저장된 ID로 SO 요청
                ItemData item = ItemManager.Instance.GetItemByID(savedSlot.itemID);
                loadedSlots.Add(new InventorySlot(item, savedSlot.amount));
            }
        }

        int inventorySize = defaultPlayerData.GetInventorySize(savedData.inventorySizeLevel);
        return (inventorySize, loadedSlots);
    }
    #endregion

    #region 진열대 데이터 세이브로드

    public void SaveDisplayStand(IReadOnlyList<InventorySlot> slots, List<int> prices)
    {
        // 아이템 슬롯 리스트 저장
        savedData.displayStandSlots.Clear();
        foreach (var slot in slots)
        {
            string id = slot.IsEmpty ? "" : slot.ItemData.itemID;
            savedData.displayStandSlots.Add(new ItemSaveData { itemID = id, amount = slot.Amount });
        }

        // 가격 리스트 저장
        savedData.displayStandPrices.Clear();
        if (prices != null)
        {
            savedData.displayStandPrices.AddRange(prices);
        }

        Debug.Log("GameManager: 진열대 (아이템 ID + 가격) 저장 완료");
    }

    public (int size, List<InventorySlot> slots, List<int> prices) LoadDisplayStand()
    {
        List<InventorySlot> loadedSlots = new List<InventorySlot>();

        // 저장된 ID를 읽어서 다시 원본 ItemData로 복구
        foreach (var savedSlot in savedData.displayStandSlots)
        {
            if (string.IsNullOrEmpty(savedSlot.itemID))
            {
                loadedSlots.Add(new InventorySlot()); // 빈 슬롯
            }
            else
            {
                ItemData item = ItemManager.Instance.GetItemByID(savedSlot.itemID);
                loadedSlots.Add(new InventorySlot(item, savedSlot.amount)); // 복구된 슬롯
            }
        }
        int displayStandSize = defaultPlayerData.GetDisplayStandSize(savedData.displayStandSizeLevel);
        return (displayStandSize, loadedSlots, savedData.displayStandPrices);
    }

    #endregion

    #region 상자 데이터 세이브로드

    public void SaveContainerBox(IReadOnlyList<InventorySlot> slots)
    {
        savedData.containerSlots.Clear();
        foreach (var slot in slots)
        {
            // 빈 슬롯이면 아이디를 비워둠
            string id = slot.IsEmpty ? "" : slot.ItemData.itemID;
            savedData.containerSlots.Add(new ItemSaveData { itemID = id, amount = slot.Amount });
        }
    }

    public (int size, List<InventorySlot> slots) LoadContainerBox()
    {
        List<InventorySlot> loadedSlots = new List<InventorySlot>();

        foreach (var savedSlot in savedData.containerSlots)
        {
            if (string.IsNullOrEmpty(savedSlot.itemID))
            {
                // 빈 슬롯 복원
                loadedSlots.Add(new InventorySlot());
            }
            else
            {
                // ItemManager에게 저장된 ID로 SO 요청
                ItemData item = ItemManager.Instance.GetItemByID(savedSlot.itemID);
                loadedSlots.Add(new InventorySlot(item, savedSlot.amount));
            }
        }
        int containerSize = defaultPlayerData.GetContainerBoxSize(savedData.containerSizeLevel);
        return (containerSize, loadedSlots);
    }

    #endregion

    #region 탐사 레벨 세이브 로드

    public void SaveSelectedExploreLevel(int level)
    {
        selectedExploreLevel = level;
        Debug.Log($"[GameManager] 탐사 시도 레벨 설정: {level}");
    }

    public int LoadSelectedExploreLevel()
    {
        return selectedExploreLevel;
    }

    public void SaveExploreMaxUnlockedLevel(int level)
    {
        if (level > savedData.exploreMaxUnlockedLevel)
        {
            savedData.exploreMaxUnlockedLevel = level;
        }
    }

    public int LoadExploreMaxUnlockedLevel()
    {
        return savedData.exploreMaxUnlockedLevel;
    }

    #endregion

}