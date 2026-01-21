using UnityEngine;
using System.Collections.Generic;

public interface ISaveable
{
    void SaveData();// 각 클래스가 자기 데이터를 GameManager에 어떻게 저장할지 스스로 정의
}

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance;
    public PlayerData playerData;

    // 씬이 넘어가도 살아있는 데이터 보관함
    public GameData savedData = new GameData();
    public int currentLevel = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            InitData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitData()
    {
        // 게임 시작시 초기화 (테스트용)
        // 타이틀 에서 새 게임 때 호출하거나 파일 로드 시 덮어씌움
        // TODO: 로드 로직
        if (playerData != null)
        {
            savedData.InitNewGame(
                playerData.maxHp,
                playerData.maxStamina,
                0, //인벤 사이즈 레벨
                0  // 진열대 사이즈 레벨
            );
        }
    }

    // 씬 넘어가기 전에 플레이어의 상태를 매니저에 기록
    // 업데이트 될때 부를지는 고민

    #region [씬 세이브 로드]

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

    public void SavePlayerState(float hp, float stamina)
    {
        savedData.currentHp = hp;
        savedData.currentStamina = stamina;

        Debug.Log("GameManager: 플레이어 컨디션 저장 완료");
    }
    
    public (float hp, float stamina) LoadPlayerCondition()
    {
        return (savedData.currentHp, savedData.currentStamina);
    }

    #endregion

    #region 플레이어 인벤토리 데이터 세이브로드

    public void SavePlayerInventory(List<InventorySlot> slots)
    {
        // 아이템 슬롯 리스트 저장
        savedData.inventorySlots.Clear();
        foreach (var slot in slots)
        {
            // 빈 슬롯이든 아이템이든 그대로 상태 복사
            savedData.inventorySlots.Add(new InventorySlot(slot.itemData, slot.amount));
        }
        Debug.Log("GameManager: 플레이어 인벤토리 저장 완료");

    }
    
    public (int size, List<InventorySlot> slots) LoadPlayerInventory()
    {
        return (savedData.inventorySize, savedData.inventorySlots);
    }
    #endregion

    #region 진열대 데이터 세이브로드

    public void SaveDisplayStand(List<InventorySlot> slots, List<int> prices)
    {
        // 아이템 슬롯 리스트 저장
        savedData.displayStandSlots.Clear();
        foreach (var slot in slots)
        {
            savedData.displayStandSlots.Add(new InventorySlot(slot.itemData, slot.amount));
        }

        // 가격 리스트 저장
        savedData.displayStandPrices.Clear();
        if (prices != null)
        {
            foreach (var price in prices)
            {
                savedData.displayStandPrices.Add(price);
            }
        }

        Debug.Log("GameManager: 진열대 (아이템 + 가격) 저장 완료");
    }
    
    public (int size, List<InventorySlot> slots, List<int> prices) LoadDisplayStand()
    {
        return (savedData.displayStandSize, savedData.displayStandSlots, savedData.displayStandPrices);
    }

    #endregion

    #region 상자 데이터 세이브로드

    // 세이브
    public void SaveWorldStorage(string objectID, List<InventorySlot> slots)
    {
        if (string.IsNullOrEmpty(objectID)) return;

        // 딕셔너리에 들어갈 새로운 리스트 생성 (깊은 복사)
        List<InventorySlot> slotsToSave = new List<InventorySlot>();
        foreach (var slot in slots)
        {
            slotsToSave.Add(new InventorySlot(slot.itemData, slot.amount));
        }

        // 딕셔너리에 저장 (이미 있으면 덮어쓰기)
        if (savedData.worldStorageData.ContainsKey(objectID))
        {
            savedData.worldStorageData[objectID] = slotsToSave;
        }
        else
        {
            savedData.worldStorageData.Add(objectID, slotsToSave);
        }

        Debug.Log($"오브젝트 저장 완료: {objectID}");
    }

    // 로드 (아이디에 해당하는 데이터로드
    public List<InventorySlot> LoadWorldStorage(string objectID)
    {
        if (string.IsNullOrEmpty(objectID)) return null;

        if (savedData.worldStorageData.ContainsKey(objectID))
        {
            return savedData.worldStorageData[objectID];
        }

        return null; // 데이터 없음
    }

    #endregion

    #region 탐사 레벨 세이브 로드

    public void SaveExploreLevel(int level)
    {
        savedData.explorationLevel = level;
        Debug.Log($"[SaveManager] 탐사 레벨 저장됨: {level}");
    }

    public int LoadExploreLevel()
    {
        return savedData.explorationLevel;
    }

    #endregion

    #endregion
}