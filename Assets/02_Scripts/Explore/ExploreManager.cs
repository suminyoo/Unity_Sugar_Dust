using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public static class ExploreEvents
{
    public static System.Action OnMonsterDefeated;
    public static System.Action OnMineralDestroyed;
}

public class ExploreManager : MonoBehaviour, ISaveable
{
    [Header("References")]
    private PlayerController player;
    public GridMapSpawner mapSpawner;
    private SPAWN_ID playerSpawnPointID = SPAWN_ID.EXPLORE_START;
    public ExploreConfigData exploreConfig;

    [Header("Exploration Settings")]

    private int currentExplorationLevel;
    private float maxTimeLimit;

    private float currentTime;
    private bool isExplorationEnded = false;
    private bool isExploreStarted = false;
    private bool isExploreSuccess = false;


    [Header("UI")]
    public TextMeshProUGUI exploreLevelText;
    public TextMeshProUGUI explorePathText;

    [Header("Result")]
    public GameObject resultUIPanel;
    public GameObject resultItemSlotPrefab;

    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultInfoText;

    public Transform earnedItemContainer;
    public Transform lostItemContainer;

    private List<InventorySlot> lostItemsList;

    public SoundData exploreSuccessSFX;   

    [Header("Exploration Logic")]
    private int targetProgressCount;
    private int currentProgressCount;
    private float currentSuccessProb;
    private bool isRetrying = false;

    private void OnEnable()
    {
        ExploreEvents.OnMonsterDefeated += AddExplorationProgress;
        ExploreEvents.OnMineralDestroyed += AddExplorationProgress;
    }

    void OnDisable()
    {
        ExploreEvents.OnMonsterDefeated -= AddExplorationProgress;
        ExploreEvents.OnMineralDestroyed -= AddExplorationProgress;

        ExploreEndSpot.OnPlayerReturnToTown -= ExploreSuccess;
        ExploreToTownPoint.OnPlayerReturnToTown -= ExploreSuccess;

        player.OnPlayerDied -= OnPlayerDeath;
        mapSpawner.OnMapGenerationComplete -= OnMapReady;
    }

    void Start()
    {
        ExploreEndSpot.OnPlayerReturnToTown += ExploreSuccess; //동적으로 생성되는 오브젝트
        ExploreToTownPoint.OnPlayerReturnToTown += ExploreSuccess;
        mapSpawner.OnMapGenerationComplete += OnMapReady;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        if (resultUIPanel != null) resultUIPanel.SetActive(false);

        // 저장된 레벨 불러오기
        if (GameSaveManager.Instance != null)
            currentExplorationLevel = GameSaveManager.Instance.LoadSelectedExploreLevel();
        else
            currentExplorationLevel = 0;

        maxTimeLimit = exploreConfig.GetStageData(currentExplorationLevel).timeLimit;
        currentTime = maxTimeLimit;

        LoadStage(currentExplorationLevel);
    }

    void Update()
    {
        if (isExplorationEnded) return;
        if (!isExploreStarted) return;

        currentTime -= Time.deltaTime;

        //시간 초과 체크
        if (currentTime <= 0)
        {
            if (GameManager.Instance.currentTime == GAME_TIME.Day)
            {
                GameManager.Instance.ChangeTime(GAME_TIME.Evening, false);
            }
            //TODO: 얼어붙는 이펙트 활성화 
            //TODO: 시간 초과시 플레이어에게 지속적인 피해 주기
        }
    }

    void LoadStage(int level, bool isRetry = false)
    {
        // 로딩 중에는 시간 멈춤
        isExploreStarted = false;
        InputControlManager.Instance.LockInput();
        ExploreStageData selectedData = exploreConfig.GetStageData(level);

        if (!isRetry) currentProgressCount = 0;

        CalculateProgressTargetCount(selectedData);

        mapSpawner.InitAndGenerateMap(selectedData, level, player);
        //UpdateExploreStateUI();
    }

    void CalculateProgressTargetCount(ExploreStageData data)
    {
        targetProgressCount = 0;
        int localLevelIndex = exploreConfig.GetLocalLevel(currentExplorationLevel);
        // 진척도를 위한 광물 개수
        foreach (var info in data.mineralObjects)
            targetProgressCount += Mathf.RoundToInt(info.spawnRateCurve.Evaluate(localLevelIndex));
        // 진척도를 위한 적 개수
        foreach (var info in data.enemyObjects)
            targetProgressCount += Mathf.RoundToInt(info.spawnRateCurve.Evaluate(localLevelIndex));

        int maxLevel = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();
        if (currentExplorationLevel < maxLevel)
        {
            currentProgressCount = targetProgressCount;
        }
    }

    public void AddExplorationProgress()
    {
        currentProgressCount++;
        UpdateExploreProgressUI();
    }

    void OnMapReady()
    {
        Debug.Log("맵 준비 완료 신호 수신");

        int localLevel = exploreConfig.GetLocalLevel(currentExplorationLevel);
        string stageName = exploreConfig.GetStageData(currentExplorationLevel).stageName;
        exploreLevelText.text = $"{stageName} {localLevel:00}";

        UpdateExploreProgressUI();
        if (player != null)
        {
            PlayerSpawnHandler.Instance.SpawnPlayer(playerSpawnPointID);
            //player.transform.position = playerSpawnPoint.transform.position;
            player.gameObject.SetActive(true);
            player.OnPlayerDied += OnPlayerDeath;
        }

        if (isRetrying)
        {
            NotificationUIManager.Instance.ShowNotification("복잡한 길 때문에 되돌아왔다…");
            isRetrying = false;
        }

        StartCoroutine(ResumeTimer());
    }

    public void AttemptMoveToNextStage()
    {
        if (Random.Range(0f, 100f) <= currentSuccessProb)
        {
            isRetrying = false;
            GoToNextStage();

            int lastMax = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();
            if (currentExplorationLevel > lastMax)
            {
                GameSaveManager.Instance.SaveExploreMaxUnlockedLevel(currentExplorationLevel);
            }
        }
        else
        {
            isRetrying = true;
            LoadStage(currentExplorationLevel);

        }
    }
    public void GoToNextStage()
    {
        currentExplorationLevel++;
        //Debug.Log($"다음 스테이지로 이동합니다. 현재 레벨: {currentLevel}");

        isExploreStarted = false;
        LoadStage(currentExplorationLevel);
    }

    #region UI

    public void UpdateExploreProgressUI()
    {
        float ratio = 0f;
        if (targetProgressCount > 0)
            ratio = (float)currentProgressCount / (float)targetProgressCount;
        else
            ratio = 1f;

        int maxLevel = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();
        bool isCleared = currentExplorationLevel < maxLevel;

        if (isCleared || ratio >= 0.7f)
        {
            currentSuccessProb = 100f;
            explorePathText.text = "이제 길을 확실히 알 것 같다.";
        }
        else if (ratio >= 0.4f)
        {
            currentSuccessProb = 50f;
            explorePathText.text = "어느 정도 길이 익숙해졌다.";
        }
        else
        {
            currentSuccessProb = 10f;
            explorePathText.text = "아직 길이 헷갈린다…";
        }
    }
    #endregion

    #region Result 

    void OnPlayerDeath()
    {
        if (isExplorationEnded) return;
        ExploreFail(false);
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
            if (exploreSuccessSFX.clip != null) SoundManager.Instance.PlaySFX2D(exploreSuccessSFX);

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
        GameManager.Instance.EndExploration(isExploreSuccess);
        
    }

    #endregion

    #region Timer
    IEnumerator ResumeTimer()
    {
        yield return new WaitForSeconds(1.0f);

        isExploreStarted = true;

        InputControlManager.Instance.UnlockInput();
    }

    public float GetCurrentTime() => currentTime;
    public float GetTimeLimit() => maxTimeLimit;

    #endregion 

    public void SaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveExploreMaxUnlockedLevel(currentExplorationLevel);
        }
    }
}