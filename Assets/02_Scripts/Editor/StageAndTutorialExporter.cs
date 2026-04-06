using UnityEditor;
using System.Text;

public class StageAndTutorialExporter
{
    [MenuItem("Tools/Export/Stage & Tutorial (Split)")]
    public static void Export()
    {
        // Stage Data
        StringBuilder sbStage = new StringBuilder();
        sbStage.AppendLine("FileName,StageNameKey,TimeLimit");
        foreach (var guid in AssetDatabase.FindAssets("t:ExploreStageData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<ExploreStageData>(AssetDatabase.GUIDToAssetPath(guid));
            string stageNameKey = CSVExporterHelper.GetLocKey(data.stageName);
            sbStage.AppendLine($"{data.name},{CSVExporterHelper.GetLocKey(data.stageName)},{data.timeLimit}");
        }

        // Tutorial Data
        StringBuilder sbTutPages = new StringBuilder();
        sbTutPages.AppendLine("ParentTutorial,PageIndex,TitleKey,DescKey,ImageName");
        foreach (var guid in AssetDatabase.FindAssets("t:TutorialDataSO"))
        {
            var data = AssetDatabase.LoadAssetAtPath<TutorialDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            for (int i = 0; i < data.pages.Length; i++)
            {
                var p = data.pages[i];

                string titleKey = CSVExporterHelper.GetLocKey(p.title);
                string descKey = CSVExporterHelper.GetLocKey(p.description);

                sbTutPages.AppendLine($"{data.name},{i},{CSVExporterHelper.GetLocKey(p.title)},{CSVExporterHelper.GetLocKey(p.description)},{(p.image != null ? p.image.name : "None")}");
            }
        }

        CSVExporterHelper.SaveCSV("Stage_Main", sbStage.ToString());
        CSVExporterHelper.SaveCSV("Tutorial_Sub_Pages", sbTutPages.ToString());
    }
}