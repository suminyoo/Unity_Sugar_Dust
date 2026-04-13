using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    public bool isRandomWait = false;
    public float waitTime = 2f;

    private void Awake()
    {
        ApplyRandomTime();
    }

    private void OnEnable()
    {
        ApplyRandomTime();
    }

    private void ApplyRandomTime()
    {
        if (isRandomWait)
        {
            waitTime = (float)Random.Range(3, 16);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 1.0f);
    }
}