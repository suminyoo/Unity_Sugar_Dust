using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    private string currentLoadedInterior;

    public CanvasGroup fadeCanvasGroup; //아직 미사용 페이드용 검은화면
    public float fadeDuration = 1.0f;

    // 다음 씬으로 넘겨줄 목적지 ID
    public SPAWN_ID targetSpawnPointID { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Fade(float finalAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true; // 클릭 방지
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, finalAlpha, time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = finalAlpha;

        // 클릭가능
        if (finalAlpha == 0f) fadeCanvasGroup.blocksRaycasts = false;
    }


    #region Change Scene

    public void ChangeScene(SCENE_NAME sceneName, SPAWN_ID spawnPointID)
    {
        StartCoroutine(SceneTransitionCor(sceneName, spawnPointID));
    }

    private IEnumerator SceneTransitionCor(SCENE_NAME sceneName, SPAWN_ID spawnPointID)
    {
        InputControlManager.Instance.LockInput();

        // 이동할 목적지 ID
        targetSpawnPointID = spawnPointID;

        // 플레이어 조작 비활성화 (선택 사항)
        // 플레이어 조작을 막고 싶다면 여기서 처리?

        //Fade Out
        yield return StartCoroutine(Fade(1f));

        // ====================================================
        // 데이터 저장
        // 씬에 있는 플레이어를 찾아서 데이터 저장
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var condition = player.GetComponent<PlayerCondition>();
            var inventory = player.GetComponent<PlayerInventory>();

            Debug.Log($"condition{condition.currentHp} / {condition.currentStamina}");

            if (condition.IsDead)
            {
                Debug.Log("플레이어 사망. 병원으로 이송합니다.");

                // 목적지 병원
                //targetSpawnPointID = SPAWN_ID.TOWN_HOSPITAL;

                // TODO: 인벤토리 드랍 로직?
            }

            // 살아있음 평소 상태 저장
            GameManager.Instance.SavePlayerState(
                condition.currentHp,
                condition.currentStamina,
                inventory.InventorySystem.slots
            );

        }
        // ====================================================
        // 씬에 있는 DisplayStand 스크립트 데이터 저장
        DisplayStand displayStand = FindObjectOfType<DisplayStand>();

        if (displayStand != null && GameManager.Instance != null)
        {
            // 진열대 인벤토리 슬롯 리스트를 매니저에게 전달
            GameManager.Instance.SaveDisplayStand(displayStand.InventorySystem.slots, displayStand.slotPrices);
        }

        // 씬 로드
        yield return SceneManager.LoadSceneAsync(sceneName.ToString());

        PlayerSpawnHandler.Instance.SpawnPlayer(spawnPointID);

        yield return null;

        // Fade In
        yield return StartCoroutine(Fade(0f));

        InputControlManager.Instance.UnlockInput();

    }

    #endregion

    #region Additive Load Scene

    public void AddSceneAndMoveTo(SCENE_NAME sceneName, SPAWN_ID spawnPointID, bool isExiting)
    {
        StartCoroutine(AdditiveLoadCor(sceneName.ToString(), spawnPointID, isExiting));
    }

    private IEnumerator AdditiveLoadCor(string sceneName, SPAWN_ID spawnPointID, bool isExiting)
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
            Debug.Log($"로드 시도 중인 씬 이름: {sceneName.ToString()}");
            yield return SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);

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
        PlayerSpawnHandler.Instance.SpawnPlayer(spawnPointID);
    }

    #endregion

    public void ChangeSceneAndAddScene(SCENE_NAME changeSceneName, SCENE_NAME addSceneName, SPAWN_ID spawnPos)
    {
        StartCoroutine(ChangeAndAddCor(changeSceneName, addSceneName, spawnPos));
    }

    private IEnumerator ChangeAndAddCor(SCENE_NAME baseScene, SCENE_NAME additiveScene, SPAWN_ID targetID)
    {
        yield return StartCoroutine(SceneTransitionCor(baseScene, SPAWN_ID.NONE));
        yield return StartCoroutine(AdditiveLoadCor(additiveScene.ToString(), targetID, false));
    }


}