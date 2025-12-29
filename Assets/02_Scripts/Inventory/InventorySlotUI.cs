using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    public static event Action<ItemData> OnItemRightClicked;

    [Header("UI Components")]
    public Image itemIcon;
    public TextMeshProUGUI amountText;

    private int _slotIndex;
    private InventorySlot _slot;
    private InventoryUI _managerUI;

    public void Init(InventoryUI ui, int index)
    {
        _managerUI = ui;
        _slotIndex = index;
    }

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
            itemIcon.sprite = null;
            itemIcon.color = Color.clear;
            amountText.text = "";
        }
    }

    //  클릭 처리 (우클릭: 1개씩 / 좌클릭: 내려놓기)
    public void OnPointerClick(PointerEventData eventData)
    {
        // -- 우클릭: 1개씩 집기
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (_slot.IsEmpty) return;

            // Ctrl 키 누른 상태여야 하나씩 집기
            if (Input.GetKey(KeyCode.LeftControl))
            {
                HandlePickOne();
            }
            // 그냥 우클릭은 아이템 정보 열기
            else
            {
                OnItemRightClicked.Invoke(_slot.itemData);
            }
        } 
        // -- 좌클릭: 드래그 없이 클릭만으로 내려놓기
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (_managerUI.mouseItemData.HasItem)
            {
                HandleDropLogic();
            }
        }
    }

    void HandlePickOne()
    {
        // 마우스가 비어있다면 1개 새로 집기
        if (!_managerUI.mouseItemData.HasItem)
        {
            InventorySlot oneItem = new InventorySlot(_slot.itemData, 1);
            _managerUI.mouseItemData.UpdateMouseSlot(oneItem);
            _managerUI.connectedInventory.InventorySystem.RemoveItemAtIndex(_slotIndex, 1);
        }
        // 마우스에 같은 아이템'이 있다면 1개 더 얹기
        else if (_managerUI.mouseItemData.mouseSlot.itemData == _slot.itemData)
        {
            _managerUI.mouseItemData.mouseSlot.AddAmount(1);
            _managerUI.mouseItemData.UpdateMouseSlot(_managerUI.mouseItemData.mouseSlot);
            _managerUI.connectedInventory.InventorySystem.RemoveItemAtIndex(_slotIndex, 1);
        }
    }

    // 드래그 시작 (좌클릭)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_slot.IsEmpty || eventData.button != PointerEventData.InputButton.Left) return;

        int amountToPick = _slot.amount;

        // Ctrl 키 누르면 절반만 집기
        if (Input.GetKey(KeyCode.LeftControl))
        {
            amountToPick = Mathf.CeilToInt(_slot.amount / 2.0f);
        }

        InventorySlot tempSlot = new InventorySlot(_slot.itemData, amountToPick);
        _managerUI.mouseItemData.UpdateMouseSlot(tempSlot);
        _managerUI.connectedInventory.InventorySystem.RemoveItemAtIndex(_slotIndex, amountToPick);
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭 실패 시(허공에 놓음) 복구
        if (_managerUI.mouseItemData.HasItem)
        {
            var mouseData = _managerUI.mouseItemData.mouseSlot;
            InventorySlot currentSlot = _managerUI.connectedInventory.InventorySystem.slots[_slotIndex];

            if (currentSlot.IsEmpty)
            {
                _managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(_slotIndex, mouseData.itemData, mouseData.amount);
            }
            else if (currentSlot.itemData == mouseData.itemData)
            {
                int total = currentSlot.amount + mouseData.amount;
                _managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(_slotIndex, mouseData.itemData, total);
            }

            _managerUI.mouseItemData.ClearSlot();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        HandleDropLogic();
    }

    void HandleDropLogic()
    {
        if (!_managerUI.mouseItemData.HasItem) return;

        var mouseData = _managerUI.mouseItemData.mouseSlot;
        InventorySlot mySlot = _managerUI.connectedInventory.InventorySystem.slots[_slotIndex];

        // 같은 아이템이면 합치기
        if (!mySlot.IsEmpty && mySlot.itemData == mouseData.itemData && mySlot.itemData.isStackable)
        {
            int total = mySlot.amount + mouseData.amount;
            _managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(_slotIndex, mySlot.itemData, total);
            _managerUI.mouseItemData.ClearSlot();
        }
        // 다르거나 빈칸이면 교체 (Swap)
        else
        {
            var tempMyData = new InventorySlot(mySlot.itemData, mySlot.amount);
            _managerUI.connectedInventory.InventorySystem.UpdateSlotAtIndex(_slotIndex, mouseData.itemData, mouseData.amount);

            if (!tempMyData.IsEmpty)
                _managerUI.mouseItemData.UpdateMouseSlot(tempMyData);
            else
                _managerUI.mouseItemData.ClearSlot();
        }
    }

}