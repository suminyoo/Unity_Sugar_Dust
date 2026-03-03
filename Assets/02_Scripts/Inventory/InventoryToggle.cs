using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject playerBagPanel;
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Sound")]
    public AudioClip inventoryOpenSound;

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
                if(inventoryOpenSound != null) SoundManager.Instance.PlaySFX2D(inventoryOpenSound);

                bool isActive = !playerBagPanel.activeSelf;
                playerBagPanel.SetActive(isActive);

                if (!isActive)
                {
                    StorageUIManager.Instance.TryClearMouseItem();
                }
            }
        }
    }
}