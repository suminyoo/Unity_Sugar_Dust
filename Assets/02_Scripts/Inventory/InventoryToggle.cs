using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject playerBagPanel;
    public KeyCode toggleKey = KeyCode.Tab;

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
                bool isActive = !playerBagPanel.activeSelf;
                playerBagPanel.SetActive(isActive);

            }
        }
    }
}