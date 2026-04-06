using UnityEditor;
using System.Text;
using UnityEngine.Localization;

public class QuestDataExporter
{
    [MenuItem("Tools/Export/Quest Data (Split)")]
    public static void Export()
    {
        StringBuilder sbMain = new StringBuilder();
        StringBuilder sbObj = new StringBuilder();

        sbMain.AppendLine("QuestID,NameKey,DescKey,RewardGold,RequiredQuestID");
        sbObj.AppendLine("ParentQuestID,TargetType,TargetID,RequiredAmount,ObjDescKey");

        foreach (var guid in AssetDatabase.FindAssets("t:QuestData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<QuestData>(AssetDatabase.GUIDToAssetPath(guid));
            string qID = data.questID.ToString();

            var questName = CSVExporterHelper.GetPrivateField<LocalizedString>(data, "questName");
            var description = CSVExporterHelper.GetPrivateField<LocalizedString>(data, "description");

            sbMain.AppendLine($"{qID},{CSVExporterHelper.GetLocKey(questName)},{CSVExporterHelper.GetLocKey(description)},{data.rewardGold},{data.requiredQuestID}");

            foreach (var obj in data.objectives)
            {
                string targetID = obj.type switch
                {
                    QuestType.Hunt => obj.enemyID.ToString(),
                    QuestType.Collect => obj.itemID.ToString(),
                    _ => "None"
                };

                string objDescKey = CSVExporterHelper.GetLocKey(obj.objectiveDescription);

                sbObj.AppendLine($"{qID},{obj.type},{targetID},{obj.requiredAmount},{objDescKey}");
            }
        }
        CSVExporterHelper.SaveCSV("Quest_Main", sbMain.ToString());
        CSVExporterHelper.SaveCSV("Quest_Sub_Objectives", sbObj.ToString());
    }
}