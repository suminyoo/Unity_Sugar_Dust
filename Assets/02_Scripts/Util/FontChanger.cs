using UnityEngine;
using UnityEditor;
using TMPro;

public class FontChanger : EditorWindow
{
    public TMP_FontAsset newFont; // 새로 적용할 폰트

    // 유니티 상단 메뉴에 [Tools -> 폰트 일괄 변경] 메뉴를 만듭니다.
    [MenuItem("Tools/폰트 일괄 변경")]
    public static void ShowWindow()
    {
        GetWindow<FontChanger>("폰트 체인저");
    }

    private void OnGUI()
    {
        GUILayout.Label("현재 씬의 모든 TextMeshPro 폰트를 변경합니다.", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 폰트를 넣을 수 있는 칸 생성
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("새 폰트 에셋", newFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);

        // 버튼 생성
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
            // 프로젝트 폴더 내의 프리팹 원본이 아닌, 현재 씬에 존재하는 오브젝트만 변경
            if (txt.gameObject.scene.name != null)
            {
                txt.font = newFont;
                EditorUtility.SetDirty(txt); // 변경 사항을 유니티에 알림(저장 가능하게)
                count++;
            }
        }

        Debug.Log($"총 {count}개의 텍스트 폰트를 성공적으로 변경했습니다! (씬을 저장해주세요)");
    }
}