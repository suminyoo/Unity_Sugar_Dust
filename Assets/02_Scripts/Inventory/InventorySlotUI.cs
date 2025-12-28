using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Components")]
    public Image itemIcon;
    public TextMeshProUGUI amountText;

    private InventoryUI _managerUI; // 관리 클래스 
    private InventorySlot _slot;    // 데이터
    private int _slotIndex;         // 몇 번째 슬롯인지

    // 초기화 함수 (InventoryUI에서 호출)
    public void Init(InventoryUI ui, int index)
    {
        _managerUI = ui;
        _slotIndex = index;
    }

    // 데이터에 따라 상태 보여주기
    public void SetSlot(InventorySlot slot)
    {
        _slot = slot;

        if (!_slot.IsEmpty)
        {
            itemIcon.sprite = slot.itemData.icon;
            itemIcon.color = Color.white;
            amountText.text = slot.amount > 1 ? slot.amount.ToString() : "";
        }
        else
        {
            itemIcon.color = Color.clear;
            amountText.text = "";
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 마우스에 아이템 존재
            if (_managerUI.mouseItemData.HasItem)
            {
                // 슬롯에 아이템 존재: 스왑이나 합치기
                if (!_slot.IsEmpty)
                {
                    // TODO: 같은 아이템이고 겹쳐진다면? 합치기
                    HandleSwap();
                }
                // 슬롯에 아이템 없음: 아이템 내려놓기
                else
                {
                    HandlePlace();
                }
            }
            // 마우스에 아이템 없음, 슬롯에 아이템이 있음: 집기
            else if (!_slot.IsEmpty)
            {
                HandlePickUp();
            }
        }
    }
    
    void HandlePickUp()
    {
        // 마우스에 내 데이터 복사
        _managerUI.mouseItemData.UpdateMouseSlot(_slot);

        // 인벤토리 시스템에 내 인덱스 슬롯 비우기
        _managerUI.playerInventory.InventorySystem.RemoveItemAtIndex(_slotIndex, _slot.amount);
    }

    void HandlePlace()
    {
        // 인벤토리 시스템의 내 인덱스 슬롯에 마우스 데이터 넣기
        _managerUI.playerInventory.InventorySystem.UpdateSlotAtIndex(
            _slotIndex,
            _managerUI.mouseItemData.mouseSlot.itemData,
            _managerUI.mouseItemData.mouseSlot.amount
        );

        // 마우스 비우기
        _managerUI.mouseItemData.ClearSlot();
    }

    void HandleSwap()
    {
        // 내 슬롯 데이터 임시 저장
        var currentSlotData = new InventorySlot(_slot.itemData, _slot.amount);
        var mouseData = _managerUI.mouseItemData.mouseSlot;

        // 시스템에게 데이터 교체 요청
        _managerUI.playerInventory.InventorySystem.UpdateSlotAtIndex(
            _slotIndex,
            mouseData.itemData,
            mouseData.amount
        );

        // 마우스 데이터 갱신
        _managerUI.mouseItemData.UpdateMouseSlot(currentSlotData);

    }
}