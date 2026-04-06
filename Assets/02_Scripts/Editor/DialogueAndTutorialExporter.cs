using UnityEditor;
using System.Text;
using UnityEngine.Localization;

public class DialogueAndTutorialExporter
{
    [MenuItem("Tools/Export/Dialogue Data")]
    public static void Export()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("FileName,DialogueKey");

        foreach (var guid in AssetDatabase.FindAssets("t:DialogueData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<DialogueData>(AssetDatabase.GUIDToAssetPath(guid));

            var dialogueBlock = CSVExporterHelper.GetPrivateField<LocalizedString>(data, "dialogueBlock");

            string locKey = CSVExporterHelper.GetLocKey(dialogueBlock);

            sb.AppendLine($"{data.name},{locKey}");
        }
        CSVExporterHelper.SaveCSV("Dialogue_Main", sb.ToString());
    }
}