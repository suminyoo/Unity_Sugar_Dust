using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject playerBagPanel;
    public KeyCode toggleKey = KeyCode.Tab;

    public SoundData inventoryOpenSound;

    void Start()
    {
        playerBagPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (playerBagPanel != null)
            {
                bool nextState = !playerBagPanel.activeSelf;

                if (nextState) OpenInventory();
                else CloseInventory();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playerBagPanel != null && playerBagPanel.activeSelf)
            {
                CloseInventory();
            }
        }
    }

    private void OpenInventory()
    {
        if (inventoryOpenSound.clip != null) SoundManager.Instance.PlaySFX2D(inventoryOpenSound);
        playerBagPanel.SetActive(true);
    }

    private void CloseInventory()
    {
        if (inventoryOpenSound.clip != null) SoundManager.Instance.PlaySFX2D(inventoryOpenSound);

        playerBagPanel.SetActive(false);

        if (StorageUIManager.Instance != null)
        {
            StorageUIManager.Instance.TryClearMouseItem();
        }
    }
}