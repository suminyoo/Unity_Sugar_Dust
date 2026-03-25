using UnityEngine;


public class InventoryUpgrade : MonoBehaviour, IInteractable
{
    [SerializeField] private int upgradeCost = 2000;


    public string GetInteractPrompt() => $"{LocalizationHelper.Main("PROMPT_UPGRADE_INVENTORY", upgradeCost, CustomerPaymentSystem.CURRENCY_SYMBOL)}";

    public void OnInteract()
    {
        var playerInv = PlayerInventory.Instance;
        if (playerInv.playerData.IsMaxInventoryLevel(GameSaveManager.Instance.savedData.inventorySizeLevel))
        {
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.Main("NOTI_MAX_LEVEL"));
            return;
        }

        if (PlayerAssetsManager.Instance.CurrentMoney < upgradeCost)
        {
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.Main("NOTI_NOT_ENOUGH_MONEY"));
            return;
        }

        CommonConfirmPopup.Instance.OpenPopup(
            $"{LocalizationHelper.Main("CONFIRM_UPGRADE_INVENTORY", upgradeCost, CustomerPaymentSystem.CURRENCY_SYMBOL)}",
        () => {
                ExecuteUpgrade();
            }
        );
    }

    private void ExecuteUpgrade()
    {
        if (PlayerAssetsManager.Instance.TrySpendMoney(upgradeCost))
        {
            PlayerInventory.Instance.UpgradeInventorySize();
            PlayerAssetsManager.Instance.SaveData();
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.Main("NOTI_UPGRADE_SUCCESS"));
        }
    }


}
