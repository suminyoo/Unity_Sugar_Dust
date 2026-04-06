using UnityEditor;
using System.Text;
using System.Linq;

public class PlayerAndConfigExporter
{
    [MenuItem("Tools/Export/Player & Config Data")]
    public static void Export()
    {
        // Player Data (Level-based)
        StringBuilder sbPlayer = new StringBuilder();
        sbPlayer.AppendLine("Level,HP,Stamina,MaxWeight,InvSize,EquipSize,DisplaySize,ContainerSize");

        string[] pGuids = AssetDatabase.FindAssets("t:PlayerData");
        if (pGuids.Length > 0)
        {
            var data = AssetDatabase.LoadAssetAtPath<PlayerData>(AssetDatabase.GUIDToAssetPath(pGuids[0]));
            int maxLevel = data.hpLevels.Length; // HP 레벨 기준으로 추출
            for (int i = 0; i < maxLevel; i++)
            {
                sbPlayer.AppendLine($"{i},{data.GetMaxHpValue(i)},{data.GetMaxStaminaValue(i)},{data.GetMaxWeightValue(i)}," +
                                   $"{data.GetInventorySize(i)},{data.GetEquipmentSize(i)},{data.GetDisplayStandSize(i)},{data.GetContainerBoxSize(i)}");
            }
        }
        CSVExporterHelper.SaveCSV("PlayerData_Levels", sbPlayer.ToString());

        // Explore Config
        StringBuilder sbConfig = new StringBuilder();
        sbConfig.AppendLine("LevelsPerStage,LevelsPerEnv,StageProfiles");
        foreach (var guid in AssetDatabase.FindAssets("t:ExploreConfigData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<ExploreConfigData>(AssetDatabase.GUIDToAssetPath(guid));
            string profiles = string.Join(";", data.stageProfiles.Select(p => p != null ? p.name : "None"));
            sbConfig.AppendLine($"{data.levelsPerStageData},{data.levelsPerEnvironment},{CSVExporterHelper.Escape(profiles)}");
        }
        CSVExporterHelper.SaveCSV("ExploreConfig", sbConfig.ToString());
    }
}