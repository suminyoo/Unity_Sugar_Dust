using UnityEditor;

public class DataExportMaster
{
    [MenuItem("Tools/Export/Export All Data (Relational)")]
    public static void ExportAll()
    {
        ItemDataExporter.Export();
        EnemyDataExporter.Export();
        ExploreDataExporter.Export();

        QuestDataExporter.Export();
        ShopAndEnemyExporter.Export();
        StageAndTutorialExporter.Export();
        NPCDataExporter.Export();

        PlayerAndConfigExporter.Export();
        DialogueAndTutorialExporter.Export();

        EditorUtility.DisplayDialog("Export Complete", "리스트 데이터가 포함된 SO들이 분리된 테이블 형태로 추출되었습니다.", "OK");
    }
}