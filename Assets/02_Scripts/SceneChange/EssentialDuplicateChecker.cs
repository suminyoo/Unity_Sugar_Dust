using UnityEngine;

public class EssentialDuplicateChecker : MonoBehaviour
{
    private void Awake()
    {
        var essentials = GameObject.FindGameObjectsWithTag("Essential");

        if (essentials.Length > 1)
        {
            // 나중에 로드된 에센셜은 파괴
            Destroy(gameObject);
            Debug.Log("중복된 에센셜 그룹을 제거함");
        }
        else
        {
            Debug.Log("에센셜 그룹이 유일하므로 유지함");
        }
    }
}