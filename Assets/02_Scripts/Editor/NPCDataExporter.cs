using UnityEditor;
using System.Text;

public class NPCDataExporter
{
    [MenuItem("Tools/Export/NPC Data (Split)")]
    public static void Export()
    {
        StringBuilder sbMain = new StringBuilder();
        StringBuilder sbQuests = new StringBuilder();

        sbMain.AppendLine("NPCID,MoveSpeed,WaitTime,DetectRange");
        sbQuests.AppendLine("ParentNPCID,QuestID");

        foreach (var guid in AssetDatabase.FindAssets("t:NPCData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<NPCData>(AssetDatabase.GUIDToAssetPath(guid));
            sbMain.AppendLine($"{data.npcID},{data.moveSpeed},{data.waitTimeAtPoint},{data.detectRange}");

            if (data.questsToGive != null)
            {
                foreach (var q in data.questsToGive)
                {
                    if (q != null) sbQuests.AppendLine($"{data.npcID},{q.questID}");
                }
            }
        }
        CSVExporterHelper.SaveCSV("NPC_Main", sbMain.ToString());
        CSVExporterHelper.SaveCSV("NPC_Sub_Quests", sbQuests.ToString());
    }
}