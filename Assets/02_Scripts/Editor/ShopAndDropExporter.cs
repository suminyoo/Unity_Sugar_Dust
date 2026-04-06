using UnityEditor;
using System.Text;
using UnityEngine.Localization;

public class ShopAndEnemyExporter
{
    [MenuItem("Tools/Export/Shop & Enemy")]
    public static void Export()
    {
        // Shop
        StringBuilder sbShop = new StringBuilder();
        sbShop.AppendLine("FileName,ShopNameKey");
        foreach (var guid in AssetDatabase.FindAssets("t:ShopData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<ShopData>(AssetDatabase.GUIDToAssetPath(guid));

            var shopName = CSVExporterHelper.GetPrivateField<LocalizedString>(data, "shopName");

            sbShop.AppendLine($"{data.name},{CSVExporterHelper.GetLocKey(shopName)}");
        }

        // Enemy
        StringBuilder sbEnemy = new StringBuilder();
        sbEnemy.AppendLine("EnemyID,EnemyNameKey,HP,Atk");
        foreach (var guid in AssetDatabase.FindAssets("t:EnemyData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<EnemyData>(AssetDatabase.GUIDToAssetPath(guid));

            var enemyName = CSVExporterHelper.GetPrivateField<LocalizedString>(data, "enemyName");

            sbEnemy.AppendLine($"{data.enemyID},{CSVExporterHelper.GetLocKey(enemyName)},{data.maxHp},{data.attackDamage}");
        }

        CSVExporterHelper.SaveCSV("Shop_Main", sbShop.ToString());
        CSVExporterHelper.SaveCSV("Enemy_Main", sbEnemy.ToString());
    }
}