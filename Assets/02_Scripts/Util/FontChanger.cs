using UnityEngine;
using UnityEditor;
using TMPro;

public class FontChanger : EditorWindow
{
    public TMP_FontAsset newFont;

    [MenuItem("Tools/폰트 일괄 변경")]
    public static void ShowWindow()
    {
        GetWindow<FontChanger>("폰트 체인저");
    }

    private void OnGUI()
    {
        GUILayout.Label("현재 씬의 모든 TextMeshPro 폰트를 변경합니다.", EditorStyles.boldLabel);
        GUILayout.Space(10);

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("새 폰트 에셋", newFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);

        if (GUILayout.Button("현재 씬 전체 폰트 바꾸기!"))
        {
            if (newFont != null)
            {
                ChangeAllFonts();
            }
            else
            {
                Debug.LogWarning("새 폰트를 먼저 빈칸에 넣어주세요!");
            }
        }
    }

    private void ChangeAllFonts()
    {
        TMP_Text[] allTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        int count = 0;

        foreach (TMP_Text txt in allTexts)
        {
            if (txt.gameObject.scene.name != null)
            {
                txt.font = newFont;
                EditorUtility.SetDirty(txt);
                count++;
            }
        }

        Debug.Log($"총 {count}개의 텍스트 폰트를 성공적으로 변경했습니다! (씬을 저장해주세요)");
    }
}