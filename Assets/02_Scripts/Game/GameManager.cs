using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 씬이 넘어가도 살아있는 데이터 보관함
    public GameData savedData = new GameData();

    [Header("Default Settings")]
    public float defaultMaxHp = 100f;
    public float defaultMaxStamina = 50f;
    public int defaultInventorySize = 2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // 게임 시작시 초기화 (테스트용)
            // 타이틀 에서 새 게임 때 호출하거나 파일 로드 시 덮어씌움
            savedData.InitNewGame(defaultMaxHp, defaultMaxStamina, defaultInventorySize);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
    public GameData LoadPlayerState()
    {
        return savedData;
    }
}