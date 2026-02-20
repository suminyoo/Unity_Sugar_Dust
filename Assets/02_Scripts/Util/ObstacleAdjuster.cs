using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class ObstacleAdjuster : ScriptableWizard
{
    [Header("조절할 배율 (1.0 = 메쉬 크기 딱 맞게)")]
    public float marginMultiplier = 1.0f;

    [MenuItem("Tools/Adjust Obstacles with Value")]
    public static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<ObstacleAdjuster>("장애물 크기 일괄 조절", "닫기 (Close)", "적용 (Apply)");
    }

    void OnWizardOtherButton()
    {
        AdjustObstacles();
    }

    void OnWizardCreate()
    {
    }

    void AdjustObstacles()
    {
        GameObject[] selections = Selection.gameObjects;

        if (selections.Length == 0)
        {
            Debug.LogWarning("선택된 오브젝트가 없습니다.");
            return;
        }

        int count = 0;
        foreach (GameObject go in selections)
        {
            var obstacle = go.GetComponentInChildren<NavMeshObstacle>();
            var renderer = go.GetComponentInChildren<MeshRenderer>();

            if (obstacle != null && renderer != null)
            {
                obstacle.size = renderer.localBounds.size * marginMultiplier;
                obstacle.center = renderer.localBounds.center;

                EditorUtility.SetDirty(go);
                count++;
            }
        }
        Debug.Log($"{count}개의 오브젝트에 배율 {marginMultiplier} 적용 완료");
    }
}