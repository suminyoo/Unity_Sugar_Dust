using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        StartCoroutine(SceneTransitionCor(sceneName, spawnPointID));
    }

    private IEnumerator SceneTransitionCor(SCENE_NAME sceneName, SPAWN_ID spawnPointID)
    {
        yield return UIManager.Instance.FadeOut();

        // 이동할 목적지 ID
        targetSpawnPointID = spawnPointID;

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

        yield return null;

        yield return PlayerSpawnHandler.Instance.SpawnPlayer(spawnPointID);

        yield return UIManager.Instance.FadeIn();

    }

    #endregion

    #region Additive Load Scene

    public void AddSceneAndMoveTo(SCENE_NAME sceneName, SPAWN_ID spawnPointID, bool isExiting)
    {
        StartCoroutine(AdditiveLoadCor(sceneName.ToString(), spawnPointID, isExiting));
    }

    private IEnumerator AdditiveLoadCor(string sceneName, SPAWN_ID spawnPointID, bool isExiting)
    {
        yield return UIManager.Instance.FadeOut();

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

        yield return UIManager.Instance.FadeIn();

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