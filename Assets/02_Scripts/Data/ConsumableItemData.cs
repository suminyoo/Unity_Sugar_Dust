using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Game/Item Data/Consumable")]
public class ConsumableItemData : ItemData 
{
    [Header("Consumable Stats")]
    public float hpRecoveryAmount;
    public float staminaRecoveryAmount;

    public override string GetDescription()
    {
        string baseDesc = base.GetDescription();
        string finalDesc = baseDesc;

        if (hpRecoveryAmount > 0)
        {
            finalDesc += " " + LocalizationHelper.L("HEALTH_RECOVER_RATE", hpRecoveryAmount);
        }

        if (staminaRecoveryAmount > 0)
        {
            finalDesc += " " + LocalizationHelper.L("STAMINA_RECOVER_RATE", staminaRecoveryAmount);
        }

        return finalDesc;
    }
}