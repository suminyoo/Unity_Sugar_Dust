using System.Collections.Generic;
using UnityEngine;

public class DisplayStand : InventoryHolder, IInteractable, IShopSource, ISaveable
{
    #region Variables & Data

    public PlayerData playerData; // SO 

    private List<Transform> displayPoints = new List<Transform>();
    private List<GameObject> spawnedVisualItems = new List<GameObject>();
    private List<GameObject> spawnedStandObjects = new List<GameObject>();

    public GameObject displayStandPrefab;
    public Transform gridOrigin;
    public int columns = 4;
    public Vector2 spacing = new Vector2(1.5f, 1.5f);

    public List<int> slotPrices = new List<int>();    // 각 인벤토리 슬롯에 대응하는 판매 가격 리스트
    public List<bool> slotActiveStates = new List<bool>(); //각 인벤토리 슬롯 활성화 여부 리스트

    #endregion

    #region Interaction (IInteractable)

    public string GetInteractPrompt() => "[E] 진열대 열기";

    public void OnInteract()
    {
        StorageUIManager.Instance.OpenStorage(this, "MyShop");
    }
    #endregion

    #region Price (IShopSource)

    public int GetPrice(int slotIndex)
    {
        // 활성화되지 않은 슬롯은 판매 안함
        //if (!IsSlotActive(slotIndex)) return -1; // -1로 구매 불가 표시
        // TODO: 화면에 -1로 표기되는것보다 다른 텍스트 대치 고려
        return GetSlotPrice(slotIndex);
    }

    // 슬롯 활성화 여부 확인
    public bool IsSlotActive(int slotIndex)
    {
        // 인벤토리 시스템 자체가 없거나, 인덱스가 범위를 벗어나면 false
        if (inventorySystem == null || slotIndex < 0 || slotIndex >= inventorySystem.slots.Count)
            return false;

        // 아이템이 비어있으면 false
        if (inventorySystem.slots[slotIndex].IsEmpty)
            return false;

        // 활성화 리스트의 범위를 벗어나면 false (아직 초기화 안 된 경우 등)
        if (slotIndex >= slotActiveStates.Count)
            return false;

        // 실제 값 리턴
        return slotActiveStates[slotIndex];
    }

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

    }

    private void Start()
    {
        LoadDisplayStandFromManager(); // 시작할 때 로드

        //사이즈 맞춰 진열대 배치
        GenerateDisplayGrid();

        UpdateVisuals();


        // 리스트 크기 맞추기 (가격 & 활성화 상태)
        while (slotPrices.Count < inventorySystem.slots.Count) slotPrices.Add(0); //일단 0원으로 초기화
        while (slotActiveStates.Count < inventorySystem.slots.Count) slotActiveStates.Add(false); // 기본 false
    }

    private void OnEnable()
    {
        inventorySystem.OnInventoryUpdated += HandleInventoryUpdate;
    }

    private void OnDestroy()
    {
        inventorySystem.OnInventoryUpdated -= HandleInventoryUpdate;
    }

    #endregion

    #region Grid & Visual Management

    // 그리드로 진열대 생성
    private void GenerateDisplayGrid()
    {
        // 기존 오브젝트 청소
        foreach (var obj in spawnedStandObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedStandObjects.Clear();
        displayPoints.Clear();

        if (displayStandPrefab == null || gridOrigin == null) return;

        int totalCount = inventorySystem.slots.Count;
        if (totalCount == 0) return;

        // 행 개수
        int rows = Mathf.CeilToInt((float)totalCount / columns);

        // 실제 사용할 열 수 계산
        int currentCols = Mathf.Min(totalCount, columns);

        // 전체 그리드의 가로/세로 길이 계산 (간격 * (개수-1))
        float totalWidth = (currentCols - 1) * spacing.x;
        float totalDepth = (rows - 1) * spacing.y;

        // 시작 위치 계산 (중심에서 절반만큼 왼쪽, 위쪽으로 이동)
        float startX = -totalWidth / 2f;
        float startZ = totalDepth / 2f;

        for (int i = 0; i < totalCount; i++)
        {
            // 행, 열 계산
            int row = i / columns;
            int col = i % columns;

            // 각 로컬 좌표 계산
            float xPos = startX + (col * spacing.x);
            float zPos = startZ - (row * spacing.y);

            // 로컬 좌표 벡터 생성
            Vector3 localPos = new Vector3(xPos, 0, zPos);

            // 월드 좌표로 변환 (GridOrigin 기준)
            Vector3 worldPos = gridOrigin.TransformPoint(localPos);
            worldPos.y = this.transform.position.y;

            // 생성
            GameObject standObj = Instantiate(displayStandPrefab, worldPos, gridOrigin.rotation, this.transform);
            spawnedStandObjects.Add(standObj);

            // 아이템 포인트 찾기 (기존 로직 동일)
            Transform itemPoint = standObj.transform.Find("ItemPoint");

            displayPoints.Add(itemPoint);
        }
    }

    private void HandleInventoryUpdate()
    {
        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            // 아이템이 없는데 활성화 되어있거나 가격이 남아있다면 초기화
            if (inventorySystem.slots[i].IsEmpty)
            {
                // 
                slotActiveStates[i] = false; // 아이템 없으면 판매 중지
                slotPrices[i] = 0;           // 가격표 수거
            }
        }
        UpdateVisuals();
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

    #endregion

    #region Shop & NPC Logic

    // 진열대 아이템 가격 설정 함수
    public void SetSlotPrice(int slotIndex, int newPrice)
    {
        if (slotIndex >= 0 && slotIndex < slotPrices.Count)
        {
            slotPrices[slotIndex] = newPrice;
        }
    }

    // 진열대 활성화 여부 설정 (버튼으로 호출)
    public void SetSlotActive(int slotIndex, bool isActive)
    {
        if (slotIndex >= 0 && slotIndex < slotActiveStates.Count)
        {
            slotActiveStates[slotIndex] = isActive;
            //Debug.Log($"슬롯 {slotIndex} 판매 상태 변경: {isActive}");
        }
    }

    //가격 조회
    public int GetSlotPrice(int slotIndex)
    {
        // 인벤토리 시스템 자체가 없거나, 인덱스가 범위를 벗어나면 0원
        if (inventorySystem == null || slotIndex < 0 || slotIndex >= inventorySystem.slots.Count)
            return 0;

        // 아이템이 비어있으면 0원
        if (inventorySystem.slots[slotIndex].IsEmpty)
            return 0;

        // slotPrices의 범위를 벗어나면 0원 (리스트 싱크가 안 맞을 때)
        if (slotIndex >= slotPrices.Count)
            return 0;

        // 실제 가격 리턴
        return slotPrices[slotIndex];
    }
    
    public bool TryGetRandomSellableItem(out int slotIndex, out Transform itemLocation)
    {
        // 현재 판매 가능한(비어있지 않은) 모든 슬롯의 인덱스를 수집
        List<int> validIndices = new List<int>();

        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            if (i < displayPoints.Count &&
                !inventorySystem.slots[i].IsEmpty &&
                inventorySystem.slots[i].amount > 0 &&
                IsSlotActive(i))
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

    public bool TryTakeItemFromStand(int slotIndex, int amount)
    {
        // 인덱스 검증
        if (slotIndex < 0 || slotIndex >= inventorySystem.slots.Count) return false;

        //활성화 체크
        if (!IsSlotActive(slotIndex)) return false;

        var slot = inventorySystem.slots[slotIndex];

        if (!slot.IsEmpty && slot.amount >= amount)
        {
            // 물건차감
            if(slot.amount < amount)
            {
                amount = slot.amount;
            }
            inventorySystem.RemoveItemAtIndex(slotIndex, amount);
            return true;
        }
        return false;
    }

    #endregion

    #region Data Persistence
    public void SaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveDisplayStand(InventorySystem.slots, slotPrices);
        }
    }

    private void LoadDisplayStandFromManager()
    {
        if (GameSaveManager.Instance == null) return;

        var data = GameSaveManager.Instance.LoadDisplayStand();

        // 새로 만들기
        int size = playerData.GetDisplayStandSize(data.size);
        inventorySystem = new InventorySystem(size);

        // 시스템이 바뀌었으니, 이벤트도 새 시스템에 재연결
        // 기존 연결 해제 후 다시 연결
        inventorySystem.OnInventoryUpdated -= HandleInventoryUpdate;
        inventorySystem.OnInventoryUpdated += HandleInventoryUpdate;

        //가격 정보
        slotPrices = new List<int>(new int[inventorySystem.slots.Count]); //기본 0으로 된 리스트
        slotActiveStates = new List<bool>(new bool[inventorySystem.slots.Count]); // 기본값 false

        // 데이터 채우기
        var savedSlots = data.slots;
        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            if (i < savedSlots.Count)
            {
                inventorySystem.slots[i].UpdateSlot(savedSlots[i].itemData, savedSlots[i].amount);
            }
        }
        
        //진열대 가격 정보 로드
        slotPrices.Clear();
        if (data.prices != null && data.prices.Count > 0)
        {
            slotPrices.AddRange(data.prices);
        }
        else
        {
            // 데이터가 없으면 슬롯 수만큼 0으로 초기화
            for (int i = 0; i < inventorySystem.slots.Count; i++) slotPrices.Add(0);
        }

        UpdateVisuals();

    }

    #endregion

    

}
