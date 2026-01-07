using System.Collections.Generic;
using UnityEngine.Events;

//인벤 한칸
[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int amount;

    //초기화용
    public InventorySlot() // 빈 슬롯으로 시작시
    {
        itemData = null;
        amount = 0;
    }

    //생성 및 복사
    //세이브나 이미 아이템정보 가 있을때
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
    public void Clear()
    {
        itemData = null;
        amount = 0;
    }

    public bool IsEmpty => itemData == null;

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
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int maxSlots; // 최대 칸 수

    public UnityAction OnInventoryUpdated;

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

    public void RestoreData(int newSize)
    {
        maxSlots = newSize;
        slots.Clear(); // 기존 슬롯 다 지우고

        // 새 크기만큼 빈 슬롯 채우기
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }

        // 여기서 OnInventoryUpdated를 호출하지 않아도 됨 
        // (데이터 채워넣은 뒤에 한 번만 호출하면 되니까)
    }

    #endregion

    #region Add Logic
    // ADD: 이미 있으면 갯수 더하기, 없으면 슬롯 리스트에 새로 추가
    // auto라서 빈곳부터 채움 (지정 인덱스로 넣지 않음)
    public bool AddItemToSlots(ItemData item, int count)
    {
        // 있는 경우
        if (item.isStackable)
        {
            // 순서대로 탐색, 가장 앞쪽에 있는 슬롯부터 채우기
            for (int i = 0; i < slots.Count; i++)
            {
                //비어있지 않음, 아이템 같음, 꽉차지 않음
                if (!slots[i].IsEmpty && slots[i].itemData == item && slots[i].amount < item.maxStackAmount)
                {
                    slots[i].AddAmount(count);
                    OnInventoryUpdated?.Invoke();
                    return true;
                }
            }
        }

        // 없는 경우
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty) // 비어있는 첫 번째 칸
            {
                slots[i].UpdateSlot(item, count); //빈 슬롯 내용 바꾸기
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        //꽉참
        return false;
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

    // 아이템 제ㅣ거 
    //아이템위치 중요: 사용자의 마우스 드래그나 슬롯 클릭시 사용
    public void RemoveItemAtIndex(int index, int count)
    {
        //유효성 검사 (범위 밖이거나 빈칸)) 이면
        if (index < 0 || index >= slots.Count || slots[index].IsEmpty) return;

        slots[index].amount -= count;

        if (slots[index].amount <= 0)
        {
            slots[index].Clear(); // 내용비우기
        }

        OnInventoryUpdated?.Invoke();
    }

    //스왑, 드래그앤 드롭
    //인벤토리 내부 스왑용
    public void SwapItems(int indexA, int indexB)
    {
        if (indexA == indexB) return;
        if (indexA >= slots.Count || indexB >= slots.Count) return;

        //임시 변수에 A데이터를 복사
        InventorySlot temp = new InventorySlot(slots[indexA].itemData, slots[indexA].amount);

        // A에 B 내용 덮어쓰기
        slots[indexA].UpdateSlot(slots[indexB].itemData, slots[indexB].amount);

        // B에 Temp(A) 내용 덮기
        slots[indexB].UpdateSlot(temp.itemData, temp.amount);

        OnInventoryUpdated?.Invoke();
    }

    #endregion

    #region Search & Consume Item
    // 제작/퀘스트용: 아이템 데이터로 찾아서 개수만큼 소모
    // 위치 상관없이 인벤에서 꺼내서 없앰
    public void ConsumeItem(ItemData item, int count)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            // 필요한 개수를 다 채웠으면 중단
            if (count <= 0) break;

            // 슬롯에서 아이템 찾기
            if (!slots[i].IsEmpty && slots[i].itemData == item)
            {
                // 아이템 개수 충분
                if (slots[i].amount >= count)
                {
                    slots[i].amount -= count;
                    count = 0;

                    // 개수가 0이 되면 슬롯 비우기
                    if (slots[i].amount == 0) slots[i].Clear();
                }
                // 아이템 개수 부족
                else
                {
                    count -= slots[i].amount;
                    slots[i].Clear(); //ㅑ슬롯 비우기
                }
            }
        }

        // 작업이 끝났으니 UI 갱신 알림
        OnInventoryUpdated?.Invoke();
    }

    //아이템 몇개 있는지
    // 퀘스트 제출 전에 아이템 있는지 확인용?
    public int GetItemCount(ItemData item)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.itemData == item)
            {
                total += slot.amount;
            }
        }
        return total;
    }
    #endregion
}