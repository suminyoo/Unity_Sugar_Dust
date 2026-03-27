using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
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

    #region Click & Pointer Event

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.dragging) return; // 드래그 중일 땐 클릭 무시

            // Shift 좌클릭: 빠른 이동
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (StorageUIManager.Instance != null && StorageUIManager.Instance.rootCanvas.activeSelf)
                {
                    StorageUIManager.Instance.HandleItemTransfer(mySlotIndex, managerUI);
                }
                return;
            }

            // 마우스에 아이템을 들고 있다면? => 클릭으로 내려놓기
            if (managerUI.mouseItemData.HasItem)
            {
                HandleDropLogic();
            }
            // 마우스가 비어있고 슬롯에 아이템이 있다면 => 아이템 정보창/상점창 띄우기
            else if (!mySlot.IsEmpty)
            {
                managerUI.HandleSlotInfoClick(mySlotIndex);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (managerUI.contextType == InventoryContext.NPCShop) return;

        // 우클릭: 1개 놓기 or 절반 들기
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (managerUI.mouseItemData.HasItem)
            {
                HandleDropOneItem();
            }
            else if (!mySlot.IsEmpty)
            {
                HandlePickHalfItem();
            }
        }
    }

    #endregion

    #region Drag & Drop Event

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (managerUI.contextType == InventoryContext.NPCShop) return;

        // 좌클릭: 맨손일 때 슬롯의 아이템 전체 집기
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!mySlot.IsEmpty && !managerUI.mouseItemData.HasItem)
            {
                int amountToPick = mySlot.Amount;
                InventorySlot tempSlot = new InventorySlot(mySlot.ItemData, amountToPick);

                managerUI.mouseItemData.UpdateMouseSlot(tempSlot.ItemData, tempSlot.Amount);
                managerUI.connectedInventory.InventorySystem.RemoveItemAtIndex(mySlotIndex, amountToPick);
            }
        }
        // 우클릭: OnPointerDown에서 이미 절반을 집었으므로 드래그 시작만 통과시킴
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            return;
        }
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnDrop(PointerEventData eventData)
    {
        // 좌클릭이든 우클릭이든 마우스 버튼을 떼면 드롭 시도
        if (eventData.button == PointerEventData.InputButton.Left || eventData.button == PointerEventData.InputButton.Right)
        {
            HandleDropLogic();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭이 끝난 후에도 마우스에 아이템이 남아있다면
        if (managerUI.mouseItemData.HasItem)
        {
            var mouseData = managerUI.mouseItemData.MouseSlot;
            InventorySlot originalSlot = managerUI.connectedInventory.InventorySystem.Slots[mySlotIndex];

            // 원래 자리가 비어있다면 => 복구
            if (originalSlot.IsEmpty)
            {
                managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(mySlotIndex, mouseData.ItemData, mouseData.Amount);
                managerUI.mouseItemData.ClearSlot();
            }
            // 원래 슬롯에 같은 아이템이 있다면 => 합치기 복구
            else if (originalSlot.ItemData == mouseData.ItemData)
            {
                int total = originalSlot.Amount + mouseData.Amount;
                int maxStack = mouseData.ItemData.maxStackAmount;

                if (total <= maxStack)
                {
                    managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(mySlotIndex, mouseData.ItemData, total);
                    managerUI.mouseItemData.ClearSlot();
                }
                else
                {
                    // 꽉 차면 꽉 채우고 남은 건 마우스에 유지
                    managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(mySlotIndex, mouseData.ItemData, maxStack);
                    managerUI.mouseItemData.UpdateMouseSlot(mouseData.ItemData, total - maxStack);
                }
            }
        }
    }

    #endregion

    #region Drop & Pick Logic Methods

    private void HandlePickHalfItem()
    {
        int halfAmount = Mathf.CeilToInt(mySlot.Amount / 2.0f);

        // 절반을 마우스로 복사하고 현재 슬롯에서 차감
        managerUI.mouseItemData.UpdateMouseSlot(mySlot.ItemData, halfAmount);
        managerUI.connectedInventory.InventorySystem.RemoveItemAtIndex(mySlotIndex, halfAmount);
    }

    private void HandleDropOneItem()
    {
        ItemData mouseItem = managerUI.mouseItemData.MouseSlot.ItemData;

        // 슬롯이 비어있거나, 같은 아이템이면서 최대 스택을 넘지 않을 때만 1개 놓기 가능
        if (mySlot.IsEmpty || (mySlot.ItemData == mouseItem && mySlot.Amount < mouseItem.maxStackAmount))
        {
            // 인벤토리 시스템에 추가
            managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(mySlotIndex, mouseItem, mySlot.Amount + 1);

            // 마우스 쪽에서는 1개 차감
            int leftover = managerUI.mouseItemData.MouseSlot.Amount - 1;
            if (leftover > 0)
                managerUI.mouseItemData.UpdateMouseSlot(mouseItem, leftover);
            else
                managerUI.mouseItemData.ClearSlot(); // 마우스 비우기
        }
    }

    private void HandleDropLogic()
    {
        if (!managerUI.mouseItemData.HasItem) return;
        if (managerUI.contextType == InventoryContext.NPCShop) return;

        ItemData mouseItem = managerUI.mouseItemData.MouseSlot.ItemData;

        // 인벤토리 홀더가 이 아이템을 거절하면 취소
        if (!managerUI.connectedInventory.CanAcceptItem(mySlotIndex, mouseItem)) return;

        var system = managerUI.connectedInventory.InventorySystem;
        InventorySlot mouseSlot = managerUI.mouseItemData.MouseSlot;
        InventorySlot targetSlot = system.Slots[mySlotIndex];

        // 대상 슬롯이 비어있을 때
        if (targetSlot.IsEmpty)
        {
            system.UpdateSlotAtIndex(mySlotIndex, mouseItem, mouseSlot.Amount);
            managerUI.mouseItemData.ClearSlot();
        }
        // 같은 아이템일 때 (합치기 & 최대 스택 방어)
        else if (targetSlot.ItemData == mouseItem)
        {
            int total = targetSlot.Amount + mouseSlot.Amount;
            int maxStack = mouseItem.maxStackAmount;

            if (total <= maxStack)
            {
                // 전부 합치기
                system.UpdateSlotAtIndex(mySlotIndex, mouseItem, total);
                managerUI.mouseItemData.ClearSlot();
            }
            else
            {
                // 최대 스택까지만 채우고 남은 건 마우스에
                system.UpdateSlotAtIndex(mySlotIndex, mouseItem, maxStack);
                managerUI.mouseItemData.UpdateMouseSlot(mouseItem, total - maxStack);
            }
        }
        // 다른 아이템일 때
        else
        {
            // InventorySystem에서 자리를 바꾸기
            system.SwapWithExternal(mySlotIndex, mouseSlot);

            // 아이템 정보 갱신
            managerUI.mouseItemData.UpdateMouseSlot(mouseSlot.ItemData, mouseSlot.Amount);
        }
    }

    #endregion
}