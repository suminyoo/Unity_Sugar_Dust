using System.Text;
using UnityEditor;
using UnityEngine.Localization;

public class EnemyDataExporter
{
    [MenuItem("Tools/Export/Enemy Data")]
    public static void Export()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("ID,NameKey,MaxHP,MoveSpeed,RunSpeed,AtkDamage,AtkCooldown,AtkRange,DetectRange,PatrolRadius,LootTable");

        string[] guids = AssetDatabase.FindAssets("t:EnemyData");
        foreach (var guid in guids)
        {
            var data = AssetDatabase.LoadAssetAtPath<EnemyData>(AssetDatabase.GUIDToAssetPath(guid));

            var rawEnemyName = CSVExporterHelper.GetPrivateField<LocalizedString>(data, "enemyName");
            string enemyNameKey = CSVExporterHelper.GetLocKey(rawEnemyName);

            sb.AppendLine($"{data.enemyID},{CSVExporterHelper.Escape(data.name)},{data.maxHp},{data.moveSpeed},{data.runSpeed}," +
                          $"{data.attackDamage},{data.attackCooldown},{data.attackRange},{data.detectRange},{data.patrolRadius}," +
                          $"{(data.lootTable != null ? data.lootTable.name : "")}");
        }
        CSVExporterHelper.SaveCSV("EnemyData", sb.ToString());
    }
}