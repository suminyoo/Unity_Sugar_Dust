using UnityEngine;

public class StorageBox : InventoryHolder, IInteractable
{
    public string boxID; // (Box_01, Box_Kitchen 등)
    public PlayerInventory playerInventory;

    public string GetInteractPrompt() => "[E] 상자 열기";


    protected override void Awake()
    {
        base.Awake();
        LoadData();
        inventorySystem.OnInventoryUpdated += SaveData;
    }

    private void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
    }

    // 상자는 WorldObject 함수 사용
    private void LoadData()
    {
        var data = GameManager.Instance.LoadWorldStorage(boxID);
        // 데이터 채우기 로직...
    }

    private void SaveData()
    {
        GameManager.Instance.SaveWorldStorage(boxID, inventorySystem.slots);
    }

    public void OnInteract()
    {
        // 필요하다면 UI 타입을 "Box"로 설정 가능
        StorageUIManager.Instance.OpenStorage(playerInventory, this, "Common");
    }
}