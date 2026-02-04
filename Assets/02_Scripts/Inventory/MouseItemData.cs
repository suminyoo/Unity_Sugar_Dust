using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MouseItemData : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI amountText;
    [SerializeField] private InventorySlot mouseSlot;
    public InventorySlot MouseSlot => mouseSlot;

    public event Action OnMouseItemChanged;

    private void Awake()
    {
        itemSprite.color = Color.clear;
        itemSprite.raycastTarget = false; //마우스 클릭 방해 금지
        amountText.text = "";
    }

    public void UpdateMouseSlot(ItemData item, int amount)
    {
        mouseSlot.UpdateSlot(item, amount);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (HasItem)
        {
            itemSprite.sprite = mouseSlot.ItemData.icon;
            itemSprite.color = Color.white;
            amountText.text = mouseSlot.Amount > 1 ? mouseSlot.Amount.ToString() : "";
        }
        else
        {
            ClearSlot();
        }
        OnMouseItemChanged?.Invoke();
    }

    public void ClearSlot()
    {
        mouseSlot.ClearSlot();
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
        amountText.text = "";

        OnMouseItemChanged?.Invoke();
    }

    //마우스 아이템 무게
    public float GetMouseItemWeight()
    {
        if (HasItem) return mouseSlot.ItemData.weight * mouseSlot.Amount;
        return 0f;
    }

    public bool HasItem => mouseSlot != null && mouseSlot.ItemData != null;

    void Update()
    {
        if (HasItem) transform.position = Input.mousePosition;
    }


    // 마우스에 든 아이템을 강제로 버림
    public void DropItemAndClear()
    {
        if (!HasItem) return;

        if (mouseSlot.ItemData.dropPrefab != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 dropTransform = player.GetComponent<PlayerInventory>().itemDropPosition.position;

            GameObject droppedObj = Instantiate(mouseSlot.ItemData.dropPrefab, dropTransform, Quaternion.identity);

            var worldItem = droppedObj.GetComponent<WorldItem>();
            if (worldItem != null) worldItem.Initialize(mouseSlot.ItemData, mouseSlot.Amount);
        }

        ClearSlot();
    }
}