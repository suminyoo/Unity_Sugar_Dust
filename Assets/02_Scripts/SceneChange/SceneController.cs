using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System;


public class SceneController : MonoBehaviour
{
    public static event Action OnScreenFadedOut;

    public static SceneController Instance;

    private bool isChangingScene = false;
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

    // [isLoadGame 설명]
    // false (일반 이동): 현재 씬의 변경사항(위치, 퀘스트 등)을 메모리에 기록하고 이동함.
    // true  (데이터 로드): 타이틀에서 새로 시작하거나 저장 파일을 불러오는 경우임. 
    //                    이때 저장 로직이 돌면 기존 데이터를 빈 값으로 덮어쓸 위험이 있어 건너뜀.

    public void ChangeScene(SCENE_NAME sceneName, SPAWN_ID spawnPointID, bool isLoadGame = false)
    {
        if (isChangingScene) return;

        StartCoroutine(SceneTransitionCor(sceneName, spawnPointID, isLoadGame));
    }

    private IEnumerator SceneTransitionCor(SCENE_NAME sceneName, SPAWN_ID spawnPointID, bool isLoadGame, bool useFade = true)
    {
        isChangingScene = true;

        if (useFade) InputControlManager.Instance.LockInput();
        if (useFade) yield return FadeUIManager.Instance.FadeOut();

        OnScreenFadedOut?.Invoke();

        // 이동할 목적지 ID
        targetSpawnPointID = spawnPointID;

        // ================= 데이터 저장 =======================
        // 현재 씬의 ISaveable 인터페이스를 가진 컴포넌트를 찾아서 저장
        if (!isLoadGame)
        {
            var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
            foreach (var saveable in saveables)
            {
                saveable.SaveData();
            }
            //Debug.Log($"[SceneController] 총 {saveables.Count()}개의 오브젝트 데이터 저장 완료");

        }

        // ===================================================

        // 씬 로드
        yield return SceneManager.LoadSceneAsync(sceneName.ToString());

        currentLoadedInterior = null;

        yield return null;

        yield return PlayerSpawnHandler.Instance.SpawnPlayer(spawnPointID);

        if (useFade) yield return FadeUIManager.Instance.FadeIn();
        if (useFade) InputControlManager.Instance.UnlockInput();

        isChangingScene = false;


    }

    #endregion

    #region Additive Load Scene

    public void AddSceneAndMoveTo(SCENE_NAME sceneName, SPAWN_ID spawnPointID, bool isExiting, bool isLoadGame = false)
    {
        if (isChangingScene) return;

        StartCoroutine(AdditiveLoadCor(sceneName.ToString(), spawnPointID, isExiting, isLoadGame));
    }

    private IEnumerator AdditiveLoadCor(string sceneName, SPAWN_ID spawnPointID, bool isExiting, bool isLoadGame, bool useFade = true)
    {
        isChangingScene = true;

        if (useFade) InputControlManager.Instance.LockInput();
        if (useFade) yield return FadeUIManager.Instance.FadeOut();

        // ================= 데이터 저장 =======================
        // 현재 씬의 ISaveable 인터페이스를 가진 컴포넌트를 찾아서 저장

        if (!isLoadGame)
        {
            var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
            foreach (var saveable in saveables)
            {
                saveable.SaveData();
            }
            //Debug.Log($"[SceneController] 총 {saveables.Count()}개의 오브젝트 데이터 저장 완료");
        }
        // ===================================================


        //  나갈때: 기존 실내 언로드
        if (isExiting)
        {
            FindObjectOfType<EnvironmentController>()?.SetIndoorMode(false);

            Camera.main.clearFlags = CameraClearFlags.Skybox;

            if (!string.IsNullOrEmpty(currentLoadedInterior))
            {
                yield return SceneManager.UnloadSceneAsync(currentLoadedInterior);
                currentLoadedInterior = null;
            }
        }
        else // 들어갈떄: 새로운 실내 로드

        {
            FindObjectOfType<EnvironmentController>()?.SetIndoorMode(true);

            Camera.main.backgroundColor = backgroundColor;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;

            if (!string.IsNullOrEmpty(currentLoadedInterior))
            {
                yield return SceneManager.UnloadSceneAsync(currentLoadedInterior);
            }

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

        isChangingScene = false;

    }

    #endregion

    public void ChangeSceneAndAddScene(SCENE_NAME changeSceneName, SCENE_NAME addSceneName, SPAWN_ID spawnPos, bool isLoadGame = false)
    {

        StartCoroutine(ChangeAndAddCor(changeSceneName, addSceneName, spawnPos, isLoadGame));
    }

    private IEnumerator ChangeAndAddCor(SCENE_NAME baseScene, SCENE_NAME additiveScene, SPAWN_ID targetID, bool isLoadGame)
    {
        InputControlManager.Instance.LockInput();
        yield return FadeUIManager.Instance.FadeOut();

        yield return StartCoroutine(SceneTransitionCor(baseScene, SPAWN_ID.NONE, isLoadGame, false));
        yield return StartCoroutine(AdditiveLoadCor(additiveScene.ToString(), targetID, false, true, false));

        yield return FadeUIManager.Instance.FadeIn();
        InputControlManager.Instance.UnlockInput();
    }


}