using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//인벤 한칸
[System.Serializable]
public class InventorySlot
{
    public ItemData ItemData => itemData;
    private ItemData itemData;

    public int Amount => amount;
    private int amount;

    //초기화용
    public InventorySlot() // 빈 슬롯으로 시작시
    {
        itemData = null;
        amount = 0;
    }

    //생성 및 복사. 세이브나 이미 아이템정보가 있을때
    public InventorySlot(ItemData item, int count)
    {
        itemData = item;
        amount = count;
    }

    //교체용
    // 빈 슬롯에 새 아이템이 들어오거나 아이템의 자리를 맞바꿀 때 (스왑)
    public void UpdateSlot(ItemData item, int count)
    {
        itemData = item;
        amount = count;
    }

    //삭제용
    //아이템을 바닥에 다 버렸거나, 전부 소모할경우
    public void ClearSlot()
    {
        itemData = null;
        amount = 0;
    }

    public bool IsEmpty => itemData == null;
    public void SetAmount(int value) => amount = value;
    public void AddAmount(int value) => amount += value;
    public void RemoveAmount(int value) => amount -= value;
}

//==========================================================//

//인벤토리 시스템: 인벤토리 아이템 관리
// 저장에 용이, 인벤토리뿐 아니라 상점과 판매대에서 사용 가능
[System.Serializable]
public class InventorySystem
{
    //List지만 고정된 크기로 사용 인덱스가 중요해서
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    public IReadOnlyList<InventorySlot> Slots => slots;

    [SerializeField] private int maxSlots;
    public int MaxSlots => maxSlots;

    public event UnityAction OnInventoryUpdated;

    #region Initialization

    // 초기화
    public InventorySystem(int size)
    {
        maxSlots = size;

        //크기에 맞는 빈 슬롯 생성
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    #endregion

    #region Add Logic

    // ADD: 이미 있으면 갯수 더하기, 없으면 슬롯 리스트에 새로 추가
    // auto라서 빈곳부터 채움 (지정 인덱스로 넣지 않음)
    public int AddItemToSlots(ItemData item, int count)
    {
        // 중첩 가능한 아이템인 경우 기존 슬롯 먼저 채우기
        int remainingCount = count;
        if (item.isStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemData == item && slots[i].Amount < item.maxStackAmount)
                {
                    int canAdd = item.maxStackAmount - slots[i].Amount;
                    int amountToAdd = Mathf.Min(remainingCount, canAdd);

                    slots[i].AddAmount(amountToAdd);
                    remainingCount -= amountToAdd;

                    if (remainingCount <= 0) break;
                }
            }
        }

        // 남은 수량이 있다면 빈 슬롯 찾아서 넣기
        if (remainingCount > 0)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty)
                {
                    int amountToAdd = Mathf.Min(remainingCount, item.maxStackAmount);
                    slots[i].UpdateSlot(item, amountToAdd);
                    remainingCount -= amountToAdd;

                    if (remainingCount <= 0) break;
                }
            }
        }

        if (remainingCount < count) OnInventoryUpdated?.Invoke();
        return remainingCount; // 0이면 다 들어감, 0보다 크면 공간 부족으로 남은 것
    
    }
    #endregion

    #region Slot Manipulation (Index-based)

    // 아이템 업데이트 (지정 인덱스 슬롯)
    public void UpdateSlotAtIndex(int index, ItemData item, int count)
    {
        if (index < 0 || index >= slots.Count) return;

        slots[index].UpdateSlot(item, count);

        OnInventoryUpdated?.Invoke();
    }

    // 아이템 제거 
    //아이템위치 중요: 사용자의 마우스 드래그나 슬롯 클릭시 사용
    public void RemoveItemAtIndex(int index, int count)
    {
        //유효성 검사 (범위 밖이거나 빈칸)) 이면
        if (index < 0 || index >= slots.Count || slots[index].IsEmpty) return;

        slots[index].RemoveAmount(count);

        if (slots[index].Amount <= 0)
        {
            slots[index].ClearSlot(); // 내용비우기
        }

        OnInventoryUpdated?.Invoke();
    }

    //스왑, 드래그앤 드롭 인벤토리 내부 스왑용 (현재미사용)
    public void SwapItems(int indexA, int indexB)
    {
        if (indexA == indexB) return;
        if (indexA >= slots.Count || indexB >= slots.Count) return;

        //임시 변수에 A데이터를 복사
        InventorySlot temp = new InventorySlot(slots[indexA].ItemData, slots[indexA].Amount);

        // A에 B 내용 덮어쓰기
        slots[indexA].UpdateSlot(slots[indexB].ItemData, slots[indexB].Amount);

        // B에 Temp(A) 내용 덮기
        slots[indexB].UpdateSlot(temp.ItemData, temp.Amount);

        OnInventoryUpdated?.Invoke();
    }

    #endregion

    #region Search & Consume Item
    // 제작/퀘스트용: 아이템 데이터로 찾아서 개수만큼 소모
    // 위치 상관없이 인벤에서 꺼내서 없앰
    public bool ConsumeItem(ItemData item, int count)
    {
        if (GetItemCount(item) < count) return false;

        for (int i = 0; i < slots.Count; i++)
        {
            // 필요한 개수를 다 채웠으면 중단
            if (count <= 0) break;

            // 슬롯에서 아이템 찾기
            if (!slots[i].IsEmpty && slots[i].ItemData == item)
            {
                // 아이템 개수 충분
                if (slots[i].Amount >= count)
                {
                    slots[i].RemoveAmount(count);
                    count = 0;

                    // 개수가 0이 되면 슬롯 비우기
                    if (slots[i].Amount == 0) slots[i].ClearSlot();
                }
                // 아이템 개수 부족
                else
                {
                    count -= slots[i].Amount;
                    slots[i].ClearSlot(); //슬롯 비우기
                }
            }
        }

        // 작업이 끝났으니 UI 갱신 알림
        OnInventoryUpdated?.Invoke();
        return true;
    }

    //아이템 몇개 있는지
    // 퀘스트 제출 전에 아이템 있는지 확인용?
    public int GetItemCount(ItemData item)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.ItemData == item)
            {
                total += slot.Amount;
            }
        }
        return total;
    }
    #endregion

    #region External Slot Interaction

    // 인덱스 위치의 슬롯에서 1개를 꺼내서 전달받은 외부 슬롯(마우스)에 넣어주는 규칙
    public void PickOneToExternal(int index, InventorySlot externalSlot)
    {
        InventorySlot mySlot = slots[index];
        if (mySlot.IsEmpty) return;

        // 외부 슬롯이 비었거나 같은 아이템일 때만 1개 이동
        if (externalSlot.IsEmpty || externalSlot.ItemData == mySlot.ItemData)
        {
            // 외부 데이터 갱신
            externalSlot.UpdateSlot(mySlot.ItemData, externalSlot.Amount + 1);

            // 내부 리스트 갱신
            RemoveItemAtIndex(index, 1);
        }
    }

    // 인덱스 위치의 슬롯과 외부 슬롯을 통째로 교환하거나 합치는 규칙
    public void SwapWithExternal(int index, InventorySlot externalSlot)
    {
        // [방어 코드] 인덱스 범위 확인 및 외부 슬롯 존재 확인
        if (index < 0 || index >= slots.Count || externalSlot == null) return;

        InventorySlot mySlot = slots[index];

        // 같은 아이템 합치기
        if (!mySlot.IsEmpty && !externalSlot.IsEmpty &&
            mySlot.ItemData == externalSlot.ItemData && mySlot.ItemData.isStackable)
        {
            int maxStack = mySlot.ItemData.maxStackAmount;
            int currentAmount = mySlot.Amount;

            // 남은 공간 계산 후 옮기기
            int canAddCount = maxStack - currentAmount;

            int amountToAdd = Mathf.Min(canAddCount, externalSlot.Amount); // 실제로 옮길 개수 (여유 공간과 마우스가 가진 개수 중 작은 값)

            if (amountToAdd > 0)
            {
                UpdateSlotAtIndex(index, mySlot.ItemData, currentAmount + amountToAdd);

                // 외부 슬롯 업데이트
                int remaining = externalSlot.Amount - amountToAdd;
                if (remaining > 0)
                    externalSlot.UpdateSlot(externalSlot.ItemData, remaining);
                else
                    externalSlot.ClearSlot();
            }
            else
            {
                // 합칠 수 없는 경우 Swap
                Swap(index, externalSlot);
            }
        }
        // 아예 다른 아이템이거나 빈칸인 경우 swpap
        else
        {
            Swap(index, externalSlot);
        }
    }

    // 순수 교환 로직
    private void Swap(int index, InventorySlot externalSlot)
    {
        InventorySlot mySlot = slots[index];

        // 임시 저장
        ItemData tempItem = mySlot.ItemData;
        int tempAmount = mySlot.Amount;

        // 1. 내 슬롯을 외부 데이터로 교체
        UpdateSlotAtIndex(index, externalSlot.ItemData, externalSlot.Amount);

        // 2. 외부 슬롯을 내 원래 데이터로 교체
        if (tempItem != null)
            externalSlot.UpdateSlot(tempItem, tempAmount);
        else
            externalSlot.ClearSlot();
    }
    #endregion

}