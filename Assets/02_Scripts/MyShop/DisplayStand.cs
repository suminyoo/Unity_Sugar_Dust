using System.Collections.Generic;
using UnityEngine;

public class DisplayStand : InventoryHolder, IInteractable, IShopSource
{
    #region Variables & Data

    public PlayerData playerData; // SO 
    private PlayerInventory playerInventory;

    private List<Transform> displayPoints = new List<Transform>();
    private List<GameObject> spawnedVisualItems = new List<GameObject>();
    private List<GameObject> spawnedStandObjects = new List<GameObject>();

    public GameObject displayStandPrefab;
    public Transform gridOrigin;
    public int columns = 4;
    public Vector2 spacing = new Vector2(1.5f, 1.5f);

    public List<int> slotPrices = new List<int>();    // 각 인벤토리 슬롯에 대응하는 판매 가격 리스트

    #endregion

    #region Interaction (IInteractable)

    public string GetInteractPrompt() => "[E] 진열대 열기";

    public void OnInteract()
    {
        StorageUIManager.Instance.OpenStorage(playerInventory, this, "MyShop");
    }
    #endregion

    #region Price (IShopSource)

    public int GetPrice(int slotIndex)
    {
        return GetSlotPrice(slotIndex);
    }

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

    }

    private void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();

        LoadDisplayStandFromManager(); // 시작할 때 로드

        //사이즈 맞춰 진열대 배치
        GenerateDisplayGrid();

        UpdateVisuals();


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

    #endregion

    #region Data Persistence

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

    #endregion

    

}
