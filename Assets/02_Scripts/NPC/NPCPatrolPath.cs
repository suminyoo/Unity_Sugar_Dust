using System.Collections.Generic;
using UnityEngine;

public class NPCPatrolPath : MonoBehaviour
{
    [SerializeField] private List<PatrolPoint> waypoints = new List<PatrolPoint>();

    private void Awake()
    {
        UpdateWaypoints();
    }

    public void UpdateWaypoints()
    {
        waypoints.Clear();
        foreach (Transform child in transform)
        {
            PatrolPoint p = child.GetComponent<PatrolPoint>();
            if (p != null) waypoints.Add(p);
        }
    }

    public PatrolPoint GetWaypoint(int index)
    {
        if (waypoints.Count == 0) return null;
        return waypoints[index % waypoints.Count];
    }
    private void OnDrawGizmos()
    {
        UpdateWaypoints();

        if (waypoints.Count < 2) return;

        Gizmos.color = Color.white;
        for (int i = 0; i < waypoints.Count; i++)
        {
            int nextIndex = (i + 1) % waypoints.Count;
            if (waypoints[i] != null && waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].transform.position, waypoints[nextIndex].transform.position);
            }
        }
    }

}