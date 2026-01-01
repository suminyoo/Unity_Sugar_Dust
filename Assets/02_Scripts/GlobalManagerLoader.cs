using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlobalManagerLoader : MonoBehaviour
{
    public GameObject globalManagers;

    [Header("Boot Settings")]
    public bool isBootScene = false;
    private string nextSceneName = "Town";

    private void Awake()
    {
        // 매니저 생성 (없을 경우에만)
        if (GameManager.Instance == null)
        {
            GameObject managers = Instantiate(globalManagers);
            managers.name = "GlobalManagers";
            DontDestroyOnLoad(managers);
        }

        if (isBootScene)
        {
            StartCoroutine(LoadNextScene());
        }
        else
        {
            // 부트 씬이 아니라면 할 일 끝났으니 로더 삭제
            Destroy(gameObject);
        }
    }

    private IEnumerator LoadNextScene()
    {
        yield return null; 
        SceneManager.LoadScene(nextSceneName);
    }
}