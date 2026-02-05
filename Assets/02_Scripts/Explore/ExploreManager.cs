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
    private SPAWN_ID playerSpawnPointID = SPAWN_ID.EXPLORE_START;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI exploreLevelText;
    public TextMeshProUGUI explorePathFamiliarityText;

    [Header("Result")]
    public GameObject resultUIPanel;
    public GameObject resultItemSlotPrefab;

    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultInfoText;

    public Transform earnedItemContainer;
    public Transform lostItemContainer;

    private List<InventorySlot> lostItemsList;


    void Start()
    {
        ExploreEndSpot.OnPlayerReturnToTown += ExploreSuccess; //동적으로 생성되는 오브젝트
        ExploreToTownPoint.OnPlayerReturnToTown += ExploreSuccess;
        mapSpawner.OnMapGenerationComplete += OnMapReady;
        if(timerText == null) timerText = GameObject.FindGameObjectWithTag("ClockText").GetComponent<TextMeshProUGUI>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

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
        InputControlManager.Instance.LockInput();

        exploreLevelText.text = $"탐사 구역 {currentLevel:00}";

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
            PlayerSpawnHandler.Instance.SpawnPlayer(playerSpawnPointID);
            //player.transform.position = playerSpawnPoint.transform.position;
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
        //Debug.Log($"다음 스테이지로 이동합니다. 현재 레벨: {currentLevel}");

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

        UpdateTimeUI();


        //시간 초과 체크
        if (currentTime <= 0)
        {
            //TODO: 얼어붙는 이펙트 활성화 
            //TODO: 시간 초과시 플레이어에게 지속적인 피해 주기
        }
    }

    public void UpdateTimeUI()
    {
        //시간 감소
        currentTime -= Time.deltaTime;

        if (timerText != null)
        {
            float displayTime = Mathf.Max(currentTime, 0);
            int minutes = Mathf.FloorToInt(displayTime / 60F);
            int seconds = Mathf.FloorToInt(displayTime % 60F);
            timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
        }
    }

    public void UpdateExploreStateUI()
    {
        //Todo: 탐사 진척도에 따른 UI 업데이트
        //explorePathFamiliarityText.text;
    }


    void ShowResultItems()
    {
        InventoryHolder holder = player.GetComponent<InventoryHolder>();
        if (holder == null) return;

        // 얻은 아이템 UI 생성
        // 초기화
        foreach (Transform child in earnedItemContainer) 
            Destroy(child.gameObject);

        foreach (var slot in holder.GetOccupiedSlots())
        {
            GameObject slotUI = Instantiate(resultItemSlotPrefab, earnedItemContainer);
            slotUI.GetComponent<ResultItemSlotUI>()?.SetData(slot.ItemData, slot.Amount);
        }

        // 잃은 아이템 UI 생성
        if (lostItemContainer != null && lostItemsList != null)
        {
            foreach (Transform child in lostItemContainer) 
                Destroy(child.gameObject);

            // 리스트를 돌며 생성
            foreach (var slot in lostItemsList)
            {
                GameObject slotUI = Instantiate(resultItemSlotPrefab, lostItemContainer);
                slotUI.GetComponent<ResultItemSlotUI>()?.SetData(slot.ItemData, slot.Amount);
            }
        }

        resultUIPanel.SetActive(true);
    }

    private void LoseItems(bool loseAll)
    {
        // GetComponent를 통해 PlayerInventory로 가져옵니다.
        PlayerInventory playerInv = player.GetComponent<PlayerInventory>();
        if (playerInv == null) return;

        if (loseAll)
        {
            lostItemsList = new List<InventorySlot>();
            foreach (var slot in playerInv.GetOccupiedSlots())
                lostItemsList.Add(new InventorySlot(slot.ItemData, slot.Amount)); //새로운 리스트로 생성
            
            playerInv.ClearAllInventory();
        }
        else
        {
            lostItemsList = playerInv.LoseRandomItems(1, 4);
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
            resultTitleText.text = "탐사 성공";
            resultInfoText.text = "우주선을 타고 마을로 무사히 귀환합니다.";
        }
        else
        {
            resultTitleText.text = "탐사 완료";
            resultInfoText.text = "걸어서 마을로 귀환합니다.\n돌아가는 길에 몇몇 아이템을 잃어버렸습니다."; 
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

        resultTitleText.text = "탐사 실패";
        resultInfoText.text = "구조대에 의해\n병원으로 이송됩니다.\n아이템을 모두 잃어버렸습니다.";

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