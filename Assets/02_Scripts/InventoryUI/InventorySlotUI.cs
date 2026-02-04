using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image itemIcon;
    public TextMeshProUGUI amountText;

    private InventorySlot mySlot;
    private InventoryUI managerUI;
    private int mySlotIndex;

    [Header("Visual")]
    public Image selectionBorder;      // 선택되었을 때 켜질 테두리 이미지
    public Image priceBgImage;         // 가격표 배경 이미지

    public Color activePriceColor = new Color(0, 0.6f, 0, 0.8f); // 판매 중 색
    public Color inactivePriceColor = new Color(0, 0, 0, 0.5f);  // 판매 중지 색

    [Header("Shop Visuals")]
    public GameObject priceTagGroup;   // 가격 ui (평소엔 비활성화)
    public TextMeshProUGUI priceText;  // 가격 텍스트

    #region Initialization

    // 인벤토리 UI 매니저와 슬롯 인덱스 초기화
    public void Init(InventoryUI ui, int index)
    {
        managerUI = ui;
        mySlotIndex = index;
    }

    #endregion

    #region Visual Update

    // 인벤토리 슬롯의 아이콘과 수량 설정  
    public void SetSlot(InventorySlot slot)
    {
        this.mySlot = slot;
        if (!this.mySlot.IsEmpty)
        {
            itemIcon.sprite = slot.ItemData.icon;
            itemIcon.color = Color.white;
            amountText.text = slot.Amount > 1 ? slot.Amount.ToString() : "";
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.color = Color.clear;
            amountText.text = "";
        }
    }

    // 인벤토리 슬롯에 가격 표시 여부 결정
    public void DecideSlotVisual(InventorySlot slot, InventoryContext context, int price = 0)
    {
        SetSlot(slot); // 기존 아이콘/수량 설정

        // 컨텍스트에 따른 시각적 변화 처리
        if (context == InventoryContext.MyShop || context == InventoryContext.NPCShop)
        {
            if (!slot.IsEmpty)
            {
                priceTagGroup.SetActive(true);
                priceText.text = $"{price} {CustomerPaymentSystem.CURRENCY_SYMBOL}";
            }
            else
            {
                priceTagGroup.SetActive(false); // 빈 슬롯은 가격표 숨김
            }
        }
        else
        {
            // 플레이어나 상자면 가격표 숨김
            priceTagGroup.SetActive(false);
        }
    }

    // 가격 텍스트와 배경색을 업데이트
    public void UpdatePriceVisuals(int price, bool isActive)
    {
        if (priceTagGroup.activeSelf)
        {
            // 가격 텍스트 갱신
            priceText.text = $"{price:N0} {CustomerPaymentSystem.CURRENCY_SYMBOL}";

            // 배경 색상 변경
            if (priceBgImage != null)
            {
                priceBgImage.color = isActive ? activePriceColor : inactivePriceColor;
            }

            priceText.color = Color.white;
        }
    }

    // 슬롯 선택 효과 (테두리 켜기/끄기)
    public void SetSelected(bool isSelected)
    {

        selectionBorder.gameObject.SetActive(isSelected);

    }

    #endregion

    #region Click Event

    //  클릭 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        // ==== 우클릭 : 아이템 정보 열기
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 우클릭으로 창띄우기 (매니저에게 위임)
            managerUI.HandleSlotRightClick(mySlotIndex);

            if (mySlot.IsEmpty) return;

            return; // 우클릭 처리 완료 후 종료
        }

        // 좌클릭인 경우에만 아래 로직 수행
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (managerUI.contextType == InventoryContext.NPCShop) return;

            // ==== Ctrl 좌클릭으로 한개 집기
            // 일반 좌클릭보다 특수 키 조합을 먼저 체크하여 중복 실행 방지
            if (Input.GetKey(KeyCode.LeftControl))
            {
                HandlePickOne();
                return;
            }

            // ==== Shift 좌클릭으로 아이템 이동
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // 샵 매니저가 존재한다면 (상점/진열대 모드라면)
                if (StorageUIManager.Instance != null && StorageUIManager.Instance.rootCanvas.activeSelf)
                {
                    // 매니저에게 슬롯 인덱스로 이동요청, 자기소속 UI 전달
                    StorageUIManager.Instance.HandleItemTransfer(mySlotIndex, managerUI);
                }
                return;
            }

            // ==== 일반 좌클릭: 슬롯에 마우스 아이템 드롭 
            // Shift나 Ctrl이 눌리지 않았을 때만 실행됨
            if (managerUI.mouseItemData.HasItem)
            {
                HandleDropLogic();
            }
        }
    }

    void HandlePickOne()
    {
        // InventorySystem에 한개 집기 요청
        var system = managerUI.connectedInventory.InventorySystem;
        InventorySlot mouseSlot = managerUI.mouseItemData.MouseSlot;

        system.PickOneToExternal(mySlotIndex, mouseSlot);
        managerUI.mouseItemData.UpdateMouseSlot(mouseSlot.ItemData, mouseSlot.Amount);

    }

    #endregion

    #region Drag & Drop Event

    // 드래그 시작 (좌클릭)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (managerUI.contextType == InventoryContext.NPCShop) return;

        if (mySlot.IsEmpty || eventData.button != PointerEventData.InputButton.Left) return;

        int amountToPick = mySlot.Amount;

        // Ctrl 키 누르면 절반만 집기
        if (Input.GetKey(KeyCode.LeftControl))
        {
            amountToPick = Mathf.CeilToInt(mySlot.Amount / 2.0f);
        }

        InventorySlot tempSlot = new InventorySlot(mySlot.ItemData, amountToPick);
        managerUI.mouseItemData.UpdateMouseSlot(tempSlot.ItemData, tempSlot.Amount);
        managerUI.connectedInventory.InventorySystem.RemoveItemAtIndex(mySlotIndex, amountToPick);
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭 실패 시(허공에 놓음) 복구
        if (managerUI.mouseItemData.HasItem)
        {
            var mouseData = managerUI.mouseItemData.MouseSlot;
            InventorySlot currentSlot = managerUI.connectedInventory.InventorySystem.Slots[mySlotIndex];

            if (currentSlot.IsEmpty)
            {
                managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(mySlotIndex, mouseData.ItemData, mouseData.Amount);
            }
            else if (currentSlot.ItemData == mouseData.ItemData)
            {
                int total = currentSlot.Amount + mouseData.Amount;
                managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(mySlotIndex, mouseData.ItemData, total);
            }

            managerUI.mouseItemData.ClearSlot();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        HandleDropLogic();
    }

    void HandleDropLogic()
    {
        if (!managerUI.mouseItemData.HasItem) return;

        // InventorySystem에 스왑요청 (같으면 합치기, 다르면 교체)
        var system = managerUI.connectedInventory.InventorySystem;
        InventorySlot mouseSlot = managerUI.mouseItemData.MouseSlot;

        system.SwapWithExternal(mySlotIndex, mouseSlot);
        managerUI.mouseItemData.UpdateMouseSlot(mouseSlot.ItemData, mouseSlot.Amount);
    }

    #endregion

}