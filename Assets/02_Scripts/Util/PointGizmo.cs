using UnityEngine;

public class PointGizmo : MonoBehaviour
{

    public Color color = Color.blue;
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
