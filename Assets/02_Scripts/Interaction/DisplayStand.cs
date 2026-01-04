using System.Collections.Generic;
using UnityEngine;

public class DisplayStand : InventoryHolder, IInteractable
{
    public PlayerData playerData; // SO 
    private PlayerInventory playerInventory;
    public List<Transform> displayPoints; //아이템이 놓일 위치
    private List<GameObject> spawnedVisualItems = new List<GameObject>();

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

        // 데이터 바뀌면 자동 저장
        inventorySystem.OnInventoryUpdated += UpdateVisuals;
    }

    private void OnDestroy()
    {
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryUpdated -= UpdateVisuals;
        }
    }

    private void UpdateVisuals()
    {
        // 1. 기존에 진열된 모델들 싹 청소
        foreach (var obj in spawnedVisualItems)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedVisualItems.Clear();

        // 2. 현재 인벤토리 슬롯을 돌면서 모델 생성
        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            // 진열 위치(Point)가 모자라면 중단 (예: 슬롯은 10개인데 진열대는 3칸일 수 있음)
            if (i >= displayPoints.Count) break;

            var slot = inventorySystem.slots[i];

            // 슬롯에 아이템이 있고 + 그 아이템에 프리팹(dropPrefab)이 설정되어 있다면
            if (!slot.IsEmpty && slot.itemData.dropPrefab != null)
            {
                // 해당 위치(displayPoints[i])에 생성
                GameObject visualObj = Instantiate(
                    slot.itemData.dropPrefab,
                    displayPoints[i].position,
                    Quaternion.identity, // 혹은 displayPoints[i].rotation (방향 맞추고 싶으면)
                    displayPoints[i]     // 부모를 Point로 설정
                );

                // 장식용이니까 물리 기능 끄기 (안 끄면 굴러떨어짐)
                var rb = visualObj.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true;

                var col = visualObj.GetComponent<Collider>();
                if (col) col.enabled = false; // 클릭 방해 안 되게 콜라이더도 끄기

                spawnedVisualItems.Add(visualObj);
            }
            else
            {
                // 빈 칸이면 리스트 자릿수 맞추기 위해 null 추가
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

        // 데이터 채우기
        var savedSlots = data.displayStandSlots;
        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            if (i < savedSlots.Count)
                inventorySystem.slots[i].UpdateSlot(savedSlots[i].itemData, savedSlots[i].amount);
        }
        UpdateVisuals();


    }

}
