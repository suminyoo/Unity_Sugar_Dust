using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ExploreManager : MonoBehaviour, ISaveable
{
    [Header("Exploration Data")]
    public int currentLevel;
    public List<ExploreStageData> stageProfiles; // 레벨별 스테이지 데이터 리스트

    [Header("Exploration Settings")]
    public float timeLimit = 300f;
    private float currentTime;

    private bool isExplorationEnded = false;
    private bool isExploreStarted = false;
    private bool isExploreSuccess = false;

    [Header("References")]
    private PlayerController player;
    public GridMapSpawner mapSpawner;
    private Transform playerSpawnPoint;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject resultUIPanel;
    public TextMeshProUGUI resultMessageText; // 메시지 표시용
    public Transform resultItemContainer;
    public GameObject resultItemSlotPrefab;


    void Start()
    {
        ExploreEndSpot.OnPlayerReturnToTown += ExploreSuccess; //동적으로 생성되는 오브젝트
        ExploreToTownPoint.OnPlayerReturnToTown += ExploreSuccess;
        mapSpawner.OnMapGenerationComplete += OnMapReady;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").GetComponent<Transform>();

        if (resultUIPanel != null) resultUIPanel.SetActive(false);

        currentTime = timeLimit;

        // 저장된 레벨 불러오기
        if (GameSaveManager.Instance != null)
            currentLevel = GameSaveManager.Instance.LoadExploreLevel();
        else
            currentLevel = 1; 
        

        LoadStage(currentLevel);
    }

    void LoadStage(int level)
    {
        // 로딩 중에는 시간 멈춤
        isExploreStarted = false;

        ExploreStageData selectedData = GetStageDataForLevel(level);
        mapSpawner.InitAndGenerateMap(selectedData, level, player);
    }

    ExploreStageData GetStageDataForLevel(int level)
    {
        // 우선 20레벨마다 데이터가 바뀜
        // index 0: 1~19 / index 1: 20~39
        int index = (level - 1) / 20;

        if (stageProfiles != null && index < stageProfiles.Count)
        {
            return stageProfiles[index];
        }

        // 데이터가 모자라면 마지막 데이터 사용
        if (stageProfiles.Count > 0) return stageProfiles[stageProfiles.Count - 1];

        return null;
    }


    void OnDestroy()
    {
        ExploreEndSpot.OnPlayerReturnToTown -= ExploreSuccess;
        ExploreToTownPoint.OnPlayerReturnToTown -= ExploreSuccess;

        player.OnPlayerDied -= OnPlayerDeath;
        mapSpawner.OnMapGenerationComplete -= OnMapReady;
    }

    void OnMapReady()
    {
        Debug.Log("맵 준비 완료 신호 수신");

        if (player != null)
        {
            player.transform.position = playerSpawnPoint.transform.position;
            player.gameObject.SetActive(true);
            player.OnPlayerDied += OnPlayerDeath;

        }

        StartCoroutine(ResumeTimer());
    }
    IEnumerator ResumeTimer()
    {
        yield return new WaitForSeconds(1.0f);

        isExploreStarted = true;

        InputControlManager.Instance.UnlockInput();
    }

    public void GoToNextStage()
    {
        currentLevel++;
        Debug.Log($"다음 스테이지로 이동합니다. 현재 레벨: {currentLevel}");

        isExploreStarted = false;

        player.gameObject.SetActive(false);

        LoadStage(currentLevel);
    }

    void OnPlayerDeath()
    {
        if (isExplorationEnded) return;
        ExploreFail(false);
    }

    void Update()
    {
        if (isExplorationEnded) return;
        if (!isExploreStarted) return;

        //시간 감소
        currentTime -= Time.deltaTime;

        if (timerText != null)
        {
            float displayTime = Mathf.Max(currentTime, 0);
            int minutes = Mathf.FloorToInt(displayTime / 60F);
            int seconds = Mathf.FloorToInt(displayTime % 60F);
            timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
        }

        //시간 초과 체크
        if (currentTime <= 0)
        {
            //TODO: 얼어붙는 이펙트 활성화 
            //TODO: 시간 초과시 플레이어에게 지속적인 피해 주기
        }
    }

    void ShowResultItems()
    {
        InventoryHolder inventoryHolder = player.GetComponent<InventoryHolder>();

        if (inventoryHolder == null) return;

        // 초기화
        foreach (Transform child in resultItemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var slot in inventoryHolder.InventorySystem.slots)
        {
            // 빈 슬롯은 건너뜀
            if (slot.itemData == null || slot.amount == 0) continue;

            // 프리팹 생성
            GameObject slotUI = Instantiate(resultItemSlotPrefab, resultItemContainer);
            Debug.Log($"[ResultUI] 아이템 슬롯 생성됨 - 아이템: {slot.itemData.name}, 수량: {slot.amount}");

            var uiScript = slotUI.GetComponent<ResultItemSlotUI>();
            if (uiScript != null)
            {
                uiScript.SetData(slot.itemData, slot.amount);
            }
        }

        resultUIPanel.SetActive(true);

    }
    
    private void LoseItems(bool loseAll)
    {
        if (!loseAll)
        {
            //TODO: 랜덤 아이템 잃기
        }
        else
        {
            //TODO: 아이템 모두 잃기
        }
    }


    // 탐사 성공
    private void ExploreSuccess(bool isSafeReturn)
    {
        if (isExplorationEnded) return;

        isExplorationEnded = true;
        isExploreSuccess = true;

        if (isSafeReturn)
        {
            if (resultMessageText != null) resultMessageText.text = "탐사 성공! 무사히 귀환합니다.";

        }
        else
        {
            if (resultMessageText != null) resultMessageText.text = "탐사 완료! 걸어서 귀환합니다.";
            LoseItems(false);
        }

        InputControlManager.Instance.LockInput();

        ShowResultItems();

    }

    private void ExploreFail(bool shouldKillPlayer)
    {
        if (isExplorationEnded) return;
        isExplorationEnded = true;
        isExploreSuccess = false;

        if (resultMessageText != null) resultMessageText.text = "탐사 실패...";

        LoseItems(true);

        ShowResultItems();

    }

    // 탐사 완료 후 결과창 버튼에 할당
    public void ReturnToTown()
    {
        InputControlManager.Instance.UnlockInput();

        if (isExploreSuccess)
        {
            // 성공: 마을 센터로 일반 이동
            SceneController.Instance.ChangeScene(SCENE_NAME.TOWN, SPAWN_ID.TOWN_CENTER);
        }
        else
        {
            // 실패(플레이어죽음): 마을 로드 후 Additive 씬을 로드하고 플레이어 이동
            SceneController.Instance.ChangeSceneAndAddScene(
                SCENE_NAME.TOWN,
                SCENE_NAME.HOSPITAL_ROOM,
                SPAWN_ID.HOSPITAL_BED
            );
        }
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public float GetTimeLimit()
    {
        return timeLimit;
    }

    public void SaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveExploreLevel(currentLevel);
        }
    }
}