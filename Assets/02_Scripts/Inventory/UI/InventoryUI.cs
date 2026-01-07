using System.Collections.Generic;
using UnityEngine;

public interface IShopSource
{    
    int GetPrice(int slotIndex); //해당칸의 가격정보를 전달
    //가격 정보가 필요한 NPC Shop 이나 Display Stand가 구현해야함
}

public enum InventoryContext
{
    Player,     // 플레이어 가방 (사용/장착)
    Chest,      // 보관함 (이동)
    MyShop,     // 내 진열대 (가격 설정, 판매 취소)
    NPCShop     // 남의 상점 (구매)
}

public class InventoryUI : MonoBehaviour
{
    #region Variables & Data

    [Header("Connection")]
    public InventoryHolder connectedInventory;
    public MouseItemData mouseItemData;
    public GameObject slotPrefab;
    public Transform contentPanel;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    private InventorySystem currentInventorySystem;

    public InventoryContext contextType = InventoryContext.Player; // 기본값

    private IShopSource currentShopSource; //가격정보 전달해주는 인터페이스 구현한 인벤토리

    #endregion

    #region Unity Lifecycle

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

    #endregion

    #region UI Setup & Initialization

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
    public void InitShopMode(IShopSource shopSource, InventoryContext context)
    {
        contextType = context;       // MyShop 인지 NPCShop 인지 저장
        currentShopSource = shopSource; // 가격 알려주는 애
        InitializeUI();
    }

    #endregion


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
                if (contextType == InventoryContext.MyShop || contextType == InventoryContext.NPCShop)
                {
                    if (currentShopSource != null)
                    {
                        // 가격 받아오기
                        itemPrice = currentShopSource.GetPrice(i);
                    }
                }

                // 슬롯에게 데이터와 컨텍스트, 가격을 넘김
                uiSlots[i].DecideSlotVisual(system.slots[i], contextType, itemPrice);
            }
        }
    }


    #region Click Event Handling

    //우클릭 이벤트 분기 처리 
    public void HandleSlotRightClick(int slotIndex)
    {
        var slot = connectedInventory.InventorySystem.slots[slotIndex];
        if (slot.IsEmpty) return;

        switch (contextType)
        {
            case InventoryContext.Player:
            case InventoryContext.Chest:
                // 플레이어나 상자: 정보창
                ItemUIPopupManager.Instance.ShowItemInfo(slot.itemData);
                break;

            case InventoryContext.MyShop:
                // 팝업매니저가 꼭 구체적인 DisplayStand 타입을 원한다면 여기서만 변환
                if (currentShopSource is DisplayStand stand)
                {
                    ItemUIPopupManager.Instance.ShowPriceSetting(stand, slotIndex, slot.itemData);
                }
                break;

            case InventoryContext.NPCShop:
                // 구매창 띄우기 (가격만 필요하면 인터페이스 그대로 사용)
                int price = (currentShopSource != null) ? currentShopSource.GetPrice(slotIndex) : 0;
                ItemUIPopupManager.Instance.ShowBuyConfirm(slot.itemData, price);
                break;
        }
    }
    #endregion
}