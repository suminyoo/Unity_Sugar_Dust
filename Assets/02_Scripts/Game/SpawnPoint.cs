using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public int spawnID; // 고유 번호

    // 에디터에서 눈에 잘 띄게 그림 그리기
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 2;
        Gizmos.DrawRay(transform.position, direction);
    }
}
