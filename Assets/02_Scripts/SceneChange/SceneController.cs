using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;


public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    private string currentLoadedInterior;

    [SerializeField] private Color backgroundColor = new Color(110, 110, 160);

    // 다음 씬으로 넘겨줄 목적지 ID
    public SPAWN_ID targetSpawnPointID { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    #region Change Scene

    public void ChangeScene(SCENE_NAME sceneName, SPAWN_ID spawnPointID)
    {
        StartCoroutine(SceneTransitionCor(sceneName, spawnPointID, true));
    }

    private IEnumerator SceneTransitionCor(SCENE_NAME sceneName, SPAWN_ID spawnPointID, bool useFade = true)
    {
        if (useFade) InputControlManager.Instance.LockInput();
        if (useFade) yield return FadeUIManager.Instance.FadeOut();

        // 이동할 목적지 ID
        targetSpawnPointID = spawnPointID;

        // ================= 데이터 저장 =======================
        // 현재 씬의 ISaveable 인터페이스를 가진 컴포넌트를 찾아서 저장
        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();

        foreach (var saveable in saveables)
        {
            saveable.SaveData();
        }

        Debug.Log($"[SceneController] 총 {saveables.Count()}개의 오브젝트 데이터 저장 완료");
        // ===================================================

        // 씬 로드
        yield return SceneManager.LoadSceneAsync(sceneName.ToString());

        currentLoadedInterior = null;

        yield return null;

        yield return PlayerSpawnHandler.Instance.SpawnPlayer(spawnPointID);

        if (useFade) yield return FadeUIManager.Instance.FadeIn();
        if (useFade) InputControlManager.Instance.UnlockInput();


    }

    #endregion

    #region Additive Load Scene

    public void AddSceneAndMoveTo(SCENE_NAME sceneName, SPAWN_ID spawnPointID, bool isExiting)
    {
        StartCoroutine(AdditiveLoadCor(sceneName.ToString(), spawnPointID, isExiting, true));
    }

    private IEnumerator AdditiveLoadCor(string sceneName, SPAWN_ID spawnPointID, bool isExiting, bool useFade = true)
    {
        if (useFade) InputControlManager.Instance.LockInput();
        if (useFade) yield return FadeUIManager.Instance.FadeOut();

        //  나갈때: 기존 실내 언로드
        if (isExiting)
        {
            Camera.main.clearFlags = CameraClearFlags.Skybox;

            if (!string.IsNullOrEmpty(currentLoadedInterior))
            {
                yield return SceneManager.UnloadSceneAsync(currentLoadedInterior);
                currentLoadedInterior = null;
            }
        }
        else
        {
            // 들어갈떄: 새로운 실내 로드
            Camera.main.backgroundColor = backgroundColor;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;

            if (!string.IsNullOrEmpty(currentLoadedInterior))
            {
                yield return SceneManager.UnloadSceneAsync(currentLoadedInterior);
            }


            Debug.Log($"로드 시도 중인 씬 이름: {sceneName.ToString()}");
            yield return SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);

            currentLoadedInterior = sceneName;

            // 로드 완료 후 해당씬 활성화 -> lighting
            //Scene newScene = SceneManager.GetSceneByName(sceneName);
            //if (newScene.IsValid())
            //{
            //    SceneManager.SetActiveScene(newScene);
            //}
        }

        // 플레이어 이동
        yield return null;

        yield return PlayerSpawnHandler.Instance.SpawnPlayer(spawnPointID);

        if (useFade) yield return FadeUIManager.Instance.FadeIn();
        if (useFade) InputControlManager.Instance.UnlockInput();

    }

    #endregion

    public void ChangeSceneAndAddScene(SCENE_NAME changeSceneName, SCENE_NAME addSceneName, SPAWN_ID spawnPos)
    {
        StartCoroutine(ChangeAndAddCor(changeSceneName, addSceneName, spawnPos));
    }

    private IEnumerator ChangeAndAddCor(SCENE_NAME baseScene, SCENE_NAME additiveScene, SPAWN_ID targetID)
    {
        InputControlManager.Instance.LockInput();
        yield return FadeUIManager.Instance.FadeOut();

        yield return StartCoroutine(SceneTransitionCor(baseScene, SPAWN_ID.NONE, false));
        yield return StartCoroutine(AdditiveLoadCor(additiveScene.ToString(), targetID, false, false));

        yield return FadeUIManager.Instance.FadeIn();
        InputControlManager.Instance.UnlockInput();
    }


}