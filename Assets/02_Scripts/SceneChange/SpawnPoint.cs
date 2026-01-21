using UnityEngine;

public enum SCENE_NAME
{
    NONE,
    TITLE,
    TOWN,
    EXPLORE,
    PLAYER_HOME,
    MY_SHOP,

    WEAPON_STORE,
    FURNITURE_STORE,
    LIBRARY_ROOM,
    HOSPITAL_ROOM,


}

public enum SPAWN_ID
{
    NONE = 0,
    TOWN_CENTER = 100,    // 마을 광장

    EXPLORE_START = 200,  // 탐사 시작점

    ROOM_SCENE_ENTRY = 300,
    PLAYERHOME_FRONTDOOR = 301,

    HOSPITAL_BED = 310,
    HOSPITAL_FRONTDOOR = 311,

    LIBRARY_FRONTDOOR = 320,

    WEAPONSTORE_FRONTDOOR = 330,

    FURNITURESTORE_FRONTDOOR = 340,



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
