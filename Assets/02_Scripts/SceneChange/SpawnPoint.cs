using UnityEngine;

public enum SCENE_NAME
{
    NONE,
    TITLE,
    TOWN,
    EXPLORE,
    PLAYER_HOME,

    HOSPITAL_ROOM,
    WEAPON_STORE,
    FURNITURE_STORE,
    LIBRARY_ROOM,


}

public enum SPAWN_ID
{
    NONE = 0,
    TOWN_CENTER = 100,    // 마을 광장

    EXPLORE_START = 200,  // 탐사 시작점

    ROOM_SCENE_ENTRY = 300,

    HOSPITAL_BED = 310,
    HOSPITAL_FRONTDOOR = 311,

}

public class SpawnPoint : MonoBehaviour
{
    public SPAWN_ID spawnID; // 고유 번호


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
