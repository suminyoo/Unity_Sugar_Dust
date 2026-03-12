using UnityEngine;

[CreateAssetMenu(fileName = "New Potion Item", menuName = "Game/Item Data/Consumable/Potion")]
public class PotionItemData : ItemData
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
            finalDesc += " " + LocalizationHelper.Item("ITEM_HEALTH_RECOVER_RATE", hpRecoveryAmount);
        }

        if (staminaRecoveryAmount > 0)
        {
            finalDesc += " " + LocalizationHelper.Item("ITEM_STAMINA_RECOVER_RATE", staminaRecoveryAmount);
        }

        return finalDesc;
    }

    public override bool IsUsable()
    {
        return true;
    }

    public override bool Use(GameObject target)
    {
        PlayerCondition condition = target.GetComponent<PlayerCondition>();

        if (condition != null)
        {
            if (hpRecoveryAmount > 0)
                condition.RecoverHp(hpRecoveryAmount);

            if (staminaRecoveryAmount > 0)
                condition.RecoverStamina(staminaRecoveryAmount);

            return true;
        }

        return false;
    }

}