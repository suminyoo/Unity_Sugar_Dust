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

}
