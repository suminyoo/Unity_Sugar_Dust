using System.Collections.Generic;
using UnityEngine;

public class DisplayStand : InventoryHolder, IInteractable
{
    public PlayerData playerData; // SO 
    private PlayerInventory playerInventory;
    public List<Transform> displayPoints; //아이템이 놓일 위치
    private List<GameObject> spawnedVisualItems = new List<GameObject>();

    public List<int> slotPrices = new List<int>();    // 각 인벤토리 슬롯에 대응하는 판매 가격 리스트

    public string GetInteractPrompt() => "[E] 진열대 열기";

    private void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
    }

    public void OnInteract()
    {
        StorageUIManager.Instance.OpenStorage(playerInventory, this, "MyShop");
    }

    protected override void Awake()
    {
        base.Awake();

        LoadDisplayStandFromManager(); // 시작할 때 로드

        // 인벤토리 크기만큼 가격 리스트 크기 맞추기 (안전장치)
        while (slotPrices.Count < inventorySystem.slots.Count)
        {
            slotPrices.Add(0); // 일단 0원으로 초기화
        }

    }

    private void OnEnable()
    {
        // 데이터 바뀌면 자동 저장
        inventorySystem.OnInventoryUpdated += UpdateVisuals;
    }

    private void OnDestroy()
    {

        inventorySystem.OnInventoryUpdated -= UpdateVisuals;
    }

    // 진열대 아이템 가격 설정 함수
    //TODO: UI 구현 필요ㅕ
    public void SetSlotPrice(int slotIndex, int newPrice)
    {
        if (slotIndex >= 0 && slotIndex < slotPrices.Count)
        {
            slotPrices[slotIndex] = newPrice;
            Debug.Log($"[Shop] {slotIndex}번 슬롯 가격 변경: {newPrice}원");
            // 변경된 가격 저장 로직 필요 시 호출
        }
    }
    //가격 조회
    public int GetSlotPrice(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotPrices.Count)
        {
            return slotPrices[slotIndex];
        }
        return 0;
    }
    public bool TryGetRandomSellableItem(out int slotIndex, out Transform itemLocation)
    {
        // 현재 판매 가능한(비어있지 않은) 모든 슬롯의 인덱스를 수집
        List<int> validIndices = new List<int>();

        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            // 시각적 위치(displayPoints) 범위 내에 있고, 아이템이 존재하는지 확인
            if (i < displayPoints.Count &&
                !inventorySystem.slots[i].IsEmpty &&
                inventorySystem.slots[i].amount > 0)
            {
                validIndices.Add(i);
            }
        }

        // 팔 물건이 하나도 없다면 실패
        if (validIndices.Count == 0)
        {
            slotIndex = -1;
            itemLocation = null;
            return false;
        }

        // 유효한 슬롯 중 랜덤으로 하나 선택
        int randomIndex = UnityEngine.Random.Range(0, validIndices.Count);
        slotIndex = validIndices[randomIndex];

        // NPC가 이동할 목표 위치 (아이템이 놓인 곳)
        itemLocation = displayPoints[slotIndex];
        return true;
    }

    //NPC 구매 확정시 호출
    public bool TrySellItemToNPC(int slotIndex)
    {
        // 인덱스 검증
        if (slotIndex < 0 || slotIndex >= inventorySystem.slots.Count) return false;

        var slot = inventorySystem.slots[slotIndex];

        // 더블 체크 (NPC가 오는 사이에 플레이어가 빼면 안됨)
        if (!slot.IsEmpty && slot.amount > 0)
        {
            // 가격 
            int finalPrice = GetSlotPrice(slotIndex);

            // 물건차감
            // TODO: 개수 정하는 로직?
            inventorySystem.RemoveItemAtIndex(slotIndex, 1);

            // 돈 추가
            PlayerAssetsManager.Instance.AddMoney(finalPrice);

            return true; // 판매 성공
        }

        return false; // 판매 실패
    }

    private void UpdateVisuals()
    {
        // 진열된 아이템 지우기
        foreach (var obj in spawnedVisualItems)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedVisualItems.Clear();

        // 인벤토리 슬롯 돌면서 모델 생성
        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            // 진열대 부족하면 중단 (부족하지 않아야함 수가 같음)
            if (i >= displayPoints.Count) break;

            var slot = inventorySystem.slots[i];

            if (!slot.IsEmpty && slot.itemData.dropPrefab != null)
            {
                // 아이템 진열대에 생성
                GameObject visualObj = Instantiate(
                    slot.itemData.dropPrefab,
                    displayPoints[i].position,
                    Quaternion.identity,
                    displayPoints[i]
                );

                visualObj.GetComponent<Collider>().enabled = false; //콜라이더끄기?

                spawnedVisualItems.Add(visualObj);
            }
            else
            {
                // 빈 칸 null 추가
                spawnedVisualItems.Add(null);
            }
        }
    }
    private void LoadDisplayStandFromManager()
    {
        if (GameManager.Instance == null) return;

        GameData data = GameManager.Instance.LoadSceneSaveData();

        // 새로 만들기
        int size = playerData.GetDisplayStandSize(data.displayStandSize);
        inventorySystem = new InventorySystem(size);

        //가격 정보
        slotPrices = new List<int>(new int[size]); // 0으로 채워진 리스트

        // 데이터 채우기
        var savedSlots = data.displayStandSlots;
        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            if (i < savedSlots.Count)
            {
                inventorySystem.slots[i].UpdateSlot(savedSlots[i].itemData, savedSlots[i].amount);
            }

        }
        

        //진열대 가격 정보 로드
        slotPrices.Clear();
        if (data.displayStandPrices != null && data.displayStandPrices.Count > 0)
        {
            slotPrices.AddRange(data.displayStandPrices);
        }
        else
        {
            // 데이터가 없으면 슬롯 수만큼 0으로 초기화
            for (int i = 0; i < inventorySystem.slots.Count; i++) slotPrices.Add(0);
        }

        UpdateVisuals();


    }


}
