using UnityEngine;

public enum UpgradeType { HP, Stamina, Inventory, DisplayStand, Container }

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }


    public void UpgradeStat(UpgradeType type)
    {
        GameData data = GameSaveManager.Instance.savedData;
        PlayerData blueprint = GameSaveManager.Instance.defaultPlayerData;
        PlayerCondition playerCondition = FindObjectOfType<PlayerCondition>();

        switch (type)
        {
            case UpgradeType.HP:
                data.hpLevel++;
                playerCondition.FullHealthRecovery();
                Debug.Log($"{type} 업그레이드 완료! 현재 레벨: {data.hpLevel}");
                //NotificationUIManager.Instance.ShowNotification(
                break;

            case UpgradeType.Stamina:
                data.staminaLevel++;
                playerCondition.FullStaminaRecovery();
                Debug.Log($"{type} 업그레이드 완료! 현재 레벨: {data.staminaLevel}");

                break;

            case UpgradeType.Inventory:
                data.inventorySizeLevel++;
                PlayerInventory.Instance.LoadInventoryFromManager();
                Debug.Log($"{type} 업그레이드 완료! 현재 레벨: {data.inventorySizeLevel}");

                break;

            case UpgradeType.DisplayStand:
                data.displayStandSizeLevel++;
                DisplayStand diplayStand = FindObjectOfType<DisplayStand>();
                if (diplayStand != null) diplayStand.LoadDisplayStandFromManager();
                Debug.Log($"{type} 업그레이드 완료! 현재 레벨: {data.displayStandSizeLevel}");

                break;

            case UpgradeType.Container:
                data.containerSizeLevel++;
                ContainerBox container = FindObjectOfType<ContainerBox>();
                if (container != null) container.LoadContainerBoxFromManager();
                Debug.Log($"{type} 업그레이드 완료! 현재 레벨: {data.containerSizeLevel}");
                break;
        }

        // 업그레이드 직후 자동 저장을 하거나, 특정 시점에 SaveCurrentGame() 호출
    }

}