using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    [Header("Grid Settings")]
    public float gridSize = 1.0f; // 격자 한 칸의 크기
    public int gridCount = 20;    // 한 방향으로 그려질 격자 개수
    public Color gridColor = Color.cyan;

    // OnDrawGizmos는 에디터 뷰에서 항상 보입니다.
    // 선택했을 때만 보고 싶다면 OnDrawGizmosSelected를 사용하세요.
    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;

        // 원점(0,0,0)을 기준으로 그리드를 그립니다.
        float halfLength = (gridSize * gridCount) / 2f;

        for (int i = 0; i <= gridCount; i++)
        {
            float pos = -halfLength + (i * gridSize);

            // 가로선 (X축 방향으로 뻗은 선들)
            Gizmos.DrawLine(new Vector3(-halfLength, 0, pos), new Vector3(halfLength, -1f, pos));

            // 세로선 (Z축 방향으로 뻗은 선들)
            Gizmos.DrawLine(new Vector3(pos, 0, -halfLength), new Vector3(pos, -1f, halfLength));
        }
    }
}