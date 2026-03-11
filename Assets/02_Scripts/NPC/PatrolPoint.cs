using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    public float waitTime = 2f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 1.0f);
    }
}