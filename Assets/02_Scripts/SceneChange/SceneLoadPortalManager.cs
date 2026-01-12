using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoadPortalManager : MonoBehaviour
{
    public static SceneLoadPortalManager Instance { get; private set; }

    private string currentLoadedInterior;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadAndMoveTo(SCENE_NAME sceneName, Vector3 spawnPos, bool isExiting)
    {
        StartCoroutine(LoadRoutine(sceneName.ToString(), spawnPos, isExiting));
    }

    private IEnumerator LoadRoutine(string sceneName, Vector3 spawnPos, bool isExiting)
    {
        //  나갈때: 기존 실내 언로드
        if (isExiting)
        {
            if (!string.IsNullOrEmpty(currentLoadedInterior))
            {
                yield return SceneManager.UnloadSceneAsync(currentLoadedInterior);
                currentLoadedInterior = null;
            }
        }
        else
        {
            // 들어갈떄: 새로운 실내 로드
            if (!string.IsNullOrEmpty(currentLoadedInterior))
            {
                yield return SceneManager.UnloadSceneAsync(currentLoadedInterior);
            }

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            currentLoadedInterior = sceneName;

            // 로드 완료 후 해당씬 활성화 (lighting
            // TODO: 라이팅 설정 확인
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid())
            {
                SceneManager.SetActiveScene(newScene);
            }
        }

        // 플레이어 이동
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = spawnPos;
        }
    }
}