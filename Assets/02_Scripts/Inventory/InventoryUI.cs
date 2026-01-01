using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Connection")]
    public InventoryHolder connectedInventory;
    public MouseItemData mouseItemData;
    public GameObject slotPrefab;
    public Transform contentPanel;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    private InventorySystem currentInventorySystem;

    void Start()
    {
        if (connectedInventory != null && connectedInventory.InventorySystem != null)
        {
            connectedInventory.InventorySystem.OnInventoryUpdated += RefreshUI;

            InitializeUI();
        }
    }
    void OnDestroy() // 또는 OnDisable()
    {
        if (connectedInventory != null && connectedInventory.InventorySystem != null)
        {
            connectedInventory.InventorySystem.OnInventoryUpdated -= RefreshUI;
        }
    }

    // 외부에서 인벤토리 설정해주는 함수
    public void SetInventorySystem(InventorySystem newSystem)
    {
        // 기존 시스템이 연결 끊기
        if (currentInventorySystem != null)
        {
            currentInventorySystem.OnInventoryUpdated -= RefreshUI;
        }

        // 새로운 시스템으로
        currentInventorySystem = newSystem;

        //새 시스템 구독 및 화면 초기화
        if (currentInventorySystem != null)
        {
            currentInventorySystem.OnInventoryUpdated += RefreshUI;
            InitializeUI(); // 슬롯 개수 다시 계산해서 생성
        }
    }

    // 설정된 개수(만큼 빈 슬롯을 미리 생성
    void InitializeUI()
    {
        // 기존에 남아있던 슬롯 모두 삭제
        foreach (Transform child in contentPanel) Destroy(child.gameObject);
        uiSlots.Clear();

        // 인벤토리 최대 크기만큼 슬롯 생성
        int size = connectedInventory.InventorySystem.maxSlots;
        for (int i = 0; i < size; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, contentPanel);
            InventorySlotUI uiScript = newSlot.GetComponent<InventorySlotUI>();

            if (uiScript != null)
            {
                uiScript.Init(this, i);
                uiSlots.Add(uiScript);
            }
        }

        RefreshUI();
    }

    // 데이터(System)랑 화면(UI) 동기화
    public void RefreshUI()
    {
        var system = connectedInventory.InventorySystem;

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < system.slots.Count)
            {
                uiSlots[i].SetSlot(system.slots[i]);
            }
        }
    }
}