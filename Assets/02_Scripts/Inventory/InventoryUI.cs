using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Connection")]
    public InventoryHolder playerInventory;
    public MouseItemData mouseItemData;
    public GameObject slotPrefab;
    public Transform contentPanel;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    void Start()
    {
        if (playerInventory != null && playerInventory.InventorySystem != null)
        {
            playerInventory.InventorySystem.OnInventoryUpdated += RefreshUI;

            InitializeUI();
        }
    }
    void OnDestroy() // 또는 OnDisable()
    {
        if (playerInventory != null && playerInventory.InventorySystem != null)
        {
            playerInventory.InventorySystem.OnInventoryUpdated -= RefreshUI;
        }
    }

    // 설정된 개수(만큼 빈 슬롯을 미리 생성
    void InitializeUI()
    {
        // 기존에 남아있던 슬롯 모두 삭제
        foreach (Transform child in contentPanel) Destroy(child.gameObject);
        uiSlots.Clear();

        // 인벤토리 최대 크기만큼 슬롯 생성
        int size = playerInventory.InventorySystem.maxSlots;
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
        var system = playerInventory.InventorySystem;

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < system.slots.Count)
            {
                uiSlots[i].SetSlot(system.slots[i]);
            }
        }
    }
}