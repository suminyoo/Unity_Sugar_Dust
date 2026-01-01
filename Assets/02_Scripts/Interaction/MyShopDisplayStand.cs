using UnityEngine;

public class MyShopDisplayStand : InventoryHolder, IInteractable
{
    private PlayerInventory playerInventory;

    private void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
    }

    public string GetInteractPrompt() => "[E] 진열대 열기";

    public void OnInteract()
    {
        StorageUIManager.Instance.OpenStorage(playerInventory, this, "MyShop");
    }
}
