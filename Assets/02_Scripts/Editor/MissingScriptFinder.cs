using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GlobalMissingScriptFinder : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts in Project")]
    public static void FindInProject()
    {
        // 모든 프리팹 에셋의 GUID를 가져옴
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (string guid in allPrefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                // 프리팹 내부의 모든 자식 컴포넌트 검사
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                foreach (Component c in components)
                {
                    if (c == null) // Missing 스크립트 발견
                    {
                        Debug.LogError($"[Missing Script] 발견됨! 위치: {path}", prefab);
                        count++;
                        break; // 한 프리팹에 여러 개 있어도 일단 하나만 출력하고 다음 프리팹으로
                    }
                }
            }
        }
        Debug.Log($"검색 완료: 총 {count}개의 프리팹에서 문제를 발견했습니다.");
    }
}