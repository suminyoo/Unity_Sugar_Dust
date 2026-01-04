using UnityEngine;
public enum SPAWN_ID
{
    None,
    Town_Center,    // 마을 광장
    Town_Hospital,  // 병원
    Explore_Start,  // 탐사 시작점

}

public class SpawnPoint : MonoBehaviour
{
    public SPAWN_ID spawnID; // 고유 번호

    // 에디터에서 눈에 잘 띄게 그림 그리기
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
