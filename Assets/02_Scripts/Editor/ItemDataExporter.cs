using UnityEditor;
using System.Text;

public class ItemDataExporter
{
    [MenuItem("Tools/Export/Item Data (Relational)")]
    public static void Export()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("FileName,ItemID,ItemType,Weight,BasePrice,SellPrice,MaxStack,Icon,DropPrefab," +
                      "UseSound,Cooldown,Consumed," +
                      "HPRecovery,StaminaRecovery," +
                      "UpgradeType," +
                      "ToolAction,ToolPrefab,Power,Range,ToolCooldown,CritChance,CritMulti");

        string[] guids = AssetDatabase.FindAssets("t:ItemData");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (item == null) continue;

            string fileName = CSVExporterHelper.Escape(item.name);

            string itemID = item.itemID.ToString();

            string itemType = item.itemType.ToString();
            string weight = item.weight.ToString();
            string basePrice = item.basePrice.ToString();
            string sellPrice = item.sellPrice.ToString();
            string maxStack = item.maxStackAmount.ToString();
            string iconName = item.icon != null ? item.icon.name : "";
            string dropPrefabName = item.dropPrefab != null ? item.dropPrefab.name : "";

            string useSound = "", cooldown = "", consumed = "";
            string hpRec = "", staRec = "";
            string upType = "";
            string tAction = "", tPrefab = "", power = "", range = "", tCooldown = "", critC = "", critM = "";

            if (item is ConsumableItemData consumable)
            {
                useSound = consumable.useSound != null ? consumable.useSound.name : "";
                cooldown = consumable.cooldownTime.ToString();
                consumed = consumable.isConsumedOnUse.ToString();

                if (consumable is PotionItemData potion)
                {
                    hpRec = potion.hpRecoveryAmount.ToString();
                    staRec = potion.staminaRecoveryAmount.ToString();
                }
                else if (consumable is UpgradeItemData upgrade)
                {
                    upType = upgrade.upgradeType.ToString();
                }
            }
            else if (item is ToolData tool)
            {
                tAction = tool.toolActionType.ToString();
                tPrefab = tool.toolPrefab != null ? tool.toolPrefab.name : "";
                power = tool.power.ToString();
                range = tool.range.ToString();
                tCooldown = tool.cooldown.ToString();
                critC = tool.criticalChance.ToString();
                critM = tool.criticalMultiplier.ToString();
            }

            sb.Append($"{fileName},{itemID},{itemType},{weight},{basePrice},{sellPrice},{maxStack},{iconName},{dropPrefabName},");
            sb.Append($"{useSound},{cooldown},{consumed},");
            sb.Append($"{hpRec},{staRec},");
            sb.Append($"{upType},");
            sb.Append($"{tAction},{tPrefab},{power},{range},{tCooldown},{critC},{critM}");
            sb.AppendLine();
        }

        CSVExporterHelper.SaveCSV("Item_Main", sb.ToString());
    }
}