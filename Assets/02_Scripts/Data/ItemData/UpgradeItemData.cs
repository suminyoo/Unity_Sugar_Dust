using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade Item", menuName = "Game/Item Data/Consumable/Upgrade")]
public class UpgradeItemData : ConsumableItemData
{
    public enum UpgradeType { Health, Stamina }

    [Header("Upgrade Settings")]
    public UpgradeType upgradeType;

    public override bool Use(GameObject target)
    {
        PlayerCondition condition = target.GetComponent<PlayerCondition>();

        if (condition != null)
        {
            if (upgradeType == UpgradeType.Health)
            {
                condition.UpgradeMaxHealth();
            }
            else if (upgradeType == UpgradeType.Stamina)
            {
                condition.UpgradeMaxStamina();
            }
            return true;
        }

        return false;
    }
}