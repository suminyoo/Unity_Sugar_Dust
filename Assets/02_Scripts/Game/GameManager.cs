using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerData playerData;

    // 씬이 넘어가도 살아있는 데이터 보관함
    public GameData savedData = new GameData();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // 게임 시작시 초기화 (테스트용)
            // 타이틀 에서 새 게임 때 호출하거나 파일 로드 시 덮어씌움
            savedData.InitNewGame(
                playerData.maxHp,
                playerData.maxStamina,
                playerData.inventorySizes[0],
                playerData.displayStandSizes[0]
                );
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region 씬 데이터 세이브

    // 씬 넘어가기 전에 플레이어의 상태를 매니저에 기록
    public void SavePlayerState(float hp, float stamina, List<InventorySlot> slots)
    {
        savedData.currentHp = hp;
        savedData.currentStamina = stamina;

        savedData.inventorySlots.Clear();
        foreach (var slot in slots)
        {
            // 빈 슬롯이든 아이템이든 그대로 상태 복사
            savedData.inventorySlots.Add(new InventorySlot(slot.itemData, slot.amount));
        }

        Debug.Log("GameManager: 플레이어 데이터 저장 완료");
    }

    // 플레이어의 데이터 요청
    public GameData LoadSceneSaveData()
    {
        return savedData;
    }


    // 씬 넘어가기 전에 플레이어의 상태를 매니저에 기록
    public void SaveDisplayStand(List<InventorySlot> slots)
    {
        savedData.displayStandSlots.Clear();
        foreach (var slot in slots)
        {
            // 빈 슬롯이든 아이템이든 그대로 상태 복사
            savedData.displayStandSlots.Add(new InventorySlot(slot.itemData, slot.amount));
        }

        Debug.Log("GameManager: 디스플레이 스탠드 저장 완료");
    }

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

}