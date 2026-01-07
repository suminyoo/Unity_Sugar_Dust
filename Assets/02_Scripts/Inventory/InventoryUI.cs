using System.Collections.Generic;
using UnityEngine;

public enum InventoryContext
{
    Player,     // 플레이어 가방 (사용/장착)
    Chest,      // 보관함 (이동)
    MyShop,     // 내 진열대 (가격 설정, 판매 취소)
    NPCShop     // 남의 상점 (구매)
}

public class InventoryUI : MonoBehaviour
{
    [Header("Connection")]
    public InventoryHolder connectedInventory;
    public MouseItemData mouseItemData;
    public GameObject slotPrefab;
    public Transform contentPanel;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    private InventorySystem currentInventorySystem;


    public InventoryContext contextType = InventoryContext.Player; // 기본값

    // 연결된 진열대 (내 상점일 때 가격 가져오기 위함)
    private DisplayStand connectedDisplayStand;

    void Start()
    {
        if (connectedInventory != null && connectedInventory.InventorySystem != null)
        {
            connectedInventory.InventorySystem.OnInventoryUpdated += RefreshUI;

            InitializeUI();
        }
    }
    void OnDestroy()
    {
        if (connectedInventory != null && connectedInventory.InventorySystem != null)
        {
            connectedInventory.InventorySystem.OnInventoryUpdated -= RefreshUI;
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

    // 외부에서 인벤토리 설정해주는 함수
    public void SetInventorySystem(InventorySystem newSystem)
    {
        //씬 전환시 인벤토리 연결 관리

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


    // 초기화 (진열대 열 때 호출)
    public void InitShopMode(DisplayStand stand)
    {
        contextType = InventoryContext.MyShop;
        connectedDisplayStand = stand;
        InitializeUI();
    }

    public void InitPlayerMode()
    {
        contextType = InventoryContext.Player;
        connectedDisplayStand = null;
        InitializeUI(); // 혹은 RefreshUI
    }
    // 화면 갱신
    public void RefreshUI()
    {
        var system = connectedInventory.InventorySystem;

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < system.slots.Count)
            {
                int itemPrice = 0;

                // 가격 정보 가져오기 로직
                if (contextType == InventoryContext.MyShop && connectedDisplayStand != null)
                {
                    itemPrice = connectedDisplayStand.GetSlotPrice(i);
                }
                else if (contextType == InventoryContext.NPCShop)
                {
                    // NPC 상점이라면 아이템 기본 가격이나 상점 데이터에서 가져옴
                    if (system.slots[i].itemData != null)
                        itemPrice = 100; // 예시: system.slots[i].itemData.basePrice;
                }

                // 슬롯에게 데이터와 컨텍스트, 가격을 넘김
                uiSlots[i].UpdateSlotVisual(system.slots[i], contextType, itemPrice);
            }
        }
    }
     
    //우클릭 이벤트 분기 처리 
    public void HandleSlotRightClick(int slotIndex)
    {
        var slot = connectedInventory.InventorySystem.slots[slotIndex];
        if (slot.IsEmpty) return;

        switch (contextType)
        {
            case InventoryContext.Player:
                // [플레이어] 아이템 사용/정보 팝업
                // ItemActionManager.Instance.ShowUseMenu(slot.itemData);
                Debug.Log($"[Player] {slot.itemData.name} 정보 및 사용 창 열기");
                break;

            case InventoryContext.Chest:
                // [상자] 바로 이동시키거나 정보 창
                Debug.Log($"[Chest] {slot.itemData.name} 정보 창 열기");
                break;

            case InventoryContext.MyShop:
                // [내 상점] 가격 설정 팝업
                Debug.Log($"[MyShop] {slot.itemData.name} 가격 설정 창 열기 (현재 가격: {connectedDisplayStand.GetSlotPrice(slotIndex)})");
                // PopupManager.Instance.ShowPriceSettingPopup(connectedDisplayStand, slotIndex);
                break;

            case InventoryContext.NPCShop:
                // [NPC 상점] 구매 확인 팝업
                Debug.Log($"[NPCShop] {slot.itemData.name} 구매하시겠습니까? (가격: 100G)");
                // PopupManager.Instance.ShowBuyConfirmPopup(slot.itemData, 100);
                break;
        }
    }
}