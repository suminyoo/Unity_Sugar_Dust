using UnityEngine;
using System.Collections.Generic;

public class PatrolPath : MonoBehaviour
{
    public List<Transform> waypoints;

    // 에디터에서 경로를 눈으로 보기 위한 기즈모
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            // 점 찍기
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);

            // 선 긋기 
            int nextIndex = (i + 1) % waypoints.Count;
            if (waypoints[nextIndex] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
        }
    }

    public Transform GetPoint(int index)
    {
        if (waypoints == null || waypoints.Count == 0) return null;
        return waypoints[index % waypoints.Count];
    }
}