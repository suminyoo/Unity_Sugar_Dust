using System.Collections.Generic;
using UnityEngine;

//가격 정보가 필요한 NPC Shop 이나 Display Stand가 구현해야함
public interface IShopSource
{    
    int GetPrice(int slotIndex); //해당칸의 가격정보를 전달
    bool IsSlotActive(int slotIndex);  // 활성화 여부
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

    // 현재 선택된 슬롯 인덱스 (없ㅅ으면 -1)
    private int selectedSlotIndex = -1;

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
    void OnDisable()
    {
        // 선택된 슬롯 인덱스 초기화
        selectedSlotIndex = -1;

        // 아이템 정보창 닫기
        if (ItemUIPopupManager.Instance != null)
        {
            ItemUIPopupManager.Instance.CloseAllPopups();
        }

        // 슬롯 선택 비주얼 해제
        foreach (var slot in uiSlots) slot.SetSelected(false);
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
        if (system == null) return;

        // 선택된 슬롯이 비었으면 팝업닫기
        if (selectedSlotIndex != -1 && selectedSlotIndex < system.slots.Count)
        {
            if (system.slots[selectedSlotIndex].IsEmpty)
            {
                selectedSlotIndex = -1; // 선택 해제
                ItemUIPopupManager.Instance.CloseAllPopups(); // 팝업 닫기
            }
        }

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < system.slots.Count)
            {
                int itemPrice = 0;
                bool isActive = false;

                if ((contextType == InventoryContext.MyShop || contextType == InventoryContext.NPCShop)
                                     && currentShopSource != null)
                {
                    // 설정한 가격 요청
                    itemPrice = currentShopSource.GetPrice(i);

                    // 판매 상태로 색 바꿈
                    isActive = currentShopSource.IsSlotActive(i); // 인터페이스 메서드
                }
                // NPC 상점은 항상 파는 가격이 중요하므로 기존 유지
                else if (contextType == InventoryContext.NPCShop && currentShopSource != null)
                {
                    itemPrice = currentShopSource.GetPrice(i);
                }

                // 슬롯에 데이터 전달
                uiSlots[i].DecideSlotVisual(system.slots[i], contextType, itemPrice);

                // 색상 업데이트 (진열대일 때만)
                if (contextType == InventoryContext.MyShop)
                {
                    uiSlots[i].UpdatePriceVisuals(itemPrice, isActive);
                    uiSlots[i].SetSelected(i == selectedSlotIndex); //선택된 슬롯인지 체크
                }
                else
                {
                    uiSlots[i].SetSelected(false); // 다른 모드에선 선택 효과 끔 (TODO:고려)
                }
            }
        }
    }


    #region Click Event Handling

    //우클릭 이벤트 분기 처리 
    public void HandleSlotRightClick(int slotIndex)
    {
        var slot = connectedInventory.InventorySystem.slots[slotIndex];
        if (slot.IsEmpty) return;

        //이전 선택된 슬롯 효과 끄고 새 슬롯 선택 효과 적용
        if (selectedSlotIndex != -1 && selectedSlotIndex < uiSlots.Count)
            uiSlots[selectedSlotIndex].SetSelected(false);
        selectedSlotIndex = slotIndex;
        uiSlots[selectedSlotIndex].SetSelected(true);

        switch (contextType)
        {
            case InventoryContext.Player:
            case InventoryContext.Chest:
                // 플레이어나 상자: 정보창
                ItemUIPopupManager.Instance.ShowItemInfo(slot.itemData);
                break;

            case InventoryContext.MyShop:
                // 진열대: 가격 설정 창
                if (currentShopSource is DisplayStand stand)
                {
                    InventorySlotUI targetSlotUI = uiSlots[slotIndex];

                    int currentPrice = stand.GetSlotPrice(slotIndex);
                    bool currentActive = stand.IsSlotActive(slotIndex);

                    ItemUIPopupManager.Instance.ShowPriceInfo(
                        slot.itemData,
                        currentPrice,
                        currentActive,

                        // 가격 바뀔 때
                        (newPrice) =>
                        {
                            stand.SetSlotPrice(slotIndex, newPrice);
                            targetSlotUI.UpdatePriceVisuals(newPrice, stand.IsSlotActive(slotIndex));
                        },

                        // 활성화 버튼 눌렀을 때
                        (newActiveState) =>
                        {
                            stand.SetSlotActive(slotIndex, newActiveState);
                            targetSlotUI.UpdatePriceVisuals(stand.GetSlotPrice(slotIndex), newActiveState);
                        }
                    );
                }
                break;

            case InventoryContext.NPCShop:
                if (currentShopSource is NPCShop shop)
                {
                    // 비활성화 상태면 무시
                    if (!shop.IsSlotActive(slotIndex)) return;

                    int price = shop.GetPrice(slotIndex);

                    // 구매 팝업 띄우기
                    ItemUIPopupManager.Instance.ShowPurchaseInfo(
                        slot.itemData,
                        price,
                        // 확인 버튼 콜백
                        () =>
                        {
                            // 플레이어 인벤토리 찾기 (잠깐 찾는거니까 갠춘)
                            var playerInv = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();

                            // 구매 시도
                            bool success = shop.TryPurchaseItem(slotIndex, playerInv);

                            if (success)
                            {
                                Debug.Log("구매 성공!");
                            }
                        }
                    );
                }
                break;
        }
    }
    #endregion
}