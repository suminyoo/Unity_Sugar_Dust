using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization;
using System.Reflection;

public static class CSVExporterHelper
{
    public static void SaveCSV(string fileName, string content)
    {
        string folderPath = Application.dataPath + "/ExportedData";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, fileName + ".csv");
        File.WriteAllText(filePath, content, Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"[CSV Export] ¿Ï·á: {filePath}");
    }

    public static string Escape(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        if (text.Contains(",") || text.Contains("\"") || text.Contains("\n"))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
        return text;
    }

    public static string GetLocKey(LocalizedString locString)
    {
        if (locString == null || locString.IsEmpty) return "None";

        string tableName = locString.TableReference.TableCollectionName;
        string keyName = "";

        if (locString.TableEntryReference.ReferenceType == UnityEngine.Localization.Tables.TableEntryReference.Type.Name)
        {
            keyName = locString.TableEntryReference.Key;
        }
        else
        {
            var collection = UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(locString.TableReference);
            if (collection != null && collection.SharedData != null)
            {
                var entry = collection.SharedData.GetEntryFromReference(locString.TableEntryReference);
                if (entry != null)
                {
                    keyName = entry.Key;
                }
            }

            if (string.IsNullOrEmpty(keyName))
            {
                keyName = locString.TableEntryReference.KeyId.ToString();
            }
        }

        return $"{tableName}/{keyName}";
    }

    public static T GetPrivateField<T>(object target, string fieldName)
    {
        if (target == null) return default;
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (field != null)
        {
            return (T)field.GetValue(target);
        }
        return default;
    }
}