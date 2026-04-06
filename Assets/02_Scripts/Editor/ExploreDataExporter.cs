using UnityEditor;
using System.Text;

public class ExploreDataExporter
{
    [MenuItem("Tools/Export/Mineral & Object Data")]
    public static void Export()
    {
        // Mineral Data
        StringBuilder sbMineral = new StringBuilder();
        sbMineral.AppendLine("FileName,MaxHealth,LootTable");
        foreach (var guid in AssetDatabase.FindAssets("t:MineralData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<MineralData>(AssetDatabase.GUIDToAssetPath(guid));
            sbMineral.AppendLine($"{data.name},{data.maxHealth},{(data.lootTable != null ? data.lootTable.name : "")}");
        }
        CSVExporterHelper.SaveCSV("MineralData", sbMineral.ToString());

        // Explore Object Data
        StringBuilder sbObj = new StringBuilder();
        sbObj.AppendLine("FileName,Prefab,SizeX,SizeY,RotationType");
        foreach (var guid in AssetDatabase.FindAssets("t:ExploreObjectData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<ExploreObjectData>(AssetDatabase.GUIDToAssetPath(guid));
            sbObj.AppendLine($"{data.name},{(data.prefab != null ? data.prefab.name : "")},{data.size.x},{data.size.y},{data.rotationType}");
        }
        CSVExporterHelper.SaveCSV("ExploreObjectData", sbObj.ToString());
    }
}