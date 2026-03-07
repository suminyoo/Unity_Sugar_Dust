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
    private PlayerController playerController;
    private PlayerCondition playerCondition;

    public GridMapSpawner mapSpawner;
    public ExploreConfigData exploreConfig;

    private SPAWN_ID playerSpawnPointID = SPAWN_ID.EXPLORE_START;

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
    public GameObject frozenBG;

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
    private bool isFrozenEnvironment = false;
    private float frozenDamageInterval = 2f;
    private float frozenDamageRate = 5f;
    private float lastFrozenDamageTime;

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

        playerController.OnPlayerDied -= OnPlayerDeath;
        mapSpawner.OnMapGenerationComplete -= OnMapReady;
    }

    void Start()
    {
        ExploreEndSpot.OnPlayerReturnToTown += ExploreSuccess; //동적으로 생성되는 오브젝트
        ExploreToTownPoint.OnPlayerReturnToTown += ExploreSuccess;
        mapSpawner.OnMapGenerationComplete += OnMapReady;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        playerCondition = player.GetComponent<PlayerCondition>();

        if (resultUIPanel != null) resultUIPanel.SetActive(false);
        if (frozenBG != null) frozenBG.SetActive(false);
        isFrozenEnvironment = false;

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
        if (isExplorationEnded || !isExploreStarted) return;

        if (!isFrozenEnvironment)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                StartFrozenEnvironment();
            }
        }
        else
        {
            HandleFrozenDamage();
        }
    }

    void StartFrozenEnvironment()
    {
        if (GameManager.Instance.currentTime == GAME_TIME.Day)
        {
            GameManager.Instance.ChangeTime(GAME_TIME.Evening, false);
        }

        isFrozenEnvironment = true;
        frozenBG.SetActive(true);

        
        NotificationUIManager.Instance.ShowNotification(LocalizationHelper.L("NOTI_TEMP_DROP"));
    }
    void HandleFrozenDamage()
    {
        lastFrozenDamageTime += Time.deltaTime;
        if (lastFrozenDamageTime >= frozenDamageInterval)
        {
            if (playerCondition != null)
            {
                playerCondition.TakeDamage(frozenDamageRate);
            }
            lastFrozenDamageTime = 0f;
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

        mapSpawner.InitAndGenerateMap(selectedData, level, playerController);
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
        int localLevel = exploreConfig.GetLocalLevel(currentExplorationLevel);
        string stageName = exploreConfig.GetStageData(currentExplorationLevel).GetStageNameText();
        exploreLevelText.text = $"{stageName} {localLevel:00}";

        UpdateExploreProgressUI();
        if (playerController != null)
        {
            PlayerSpawnHandler.Instance.SpawnPlayer(playerSpawnPointID);
            //player.transform.position = playerSpawnPoint.transform.position;
            playerController.gameObject.SetActive(true);
            playerController.OnPlayerDied += OnPlayerDeath;
        }

        if (isRetrying)
        {
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.L("NOTI_EXPLORE_RETRY"));
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
            explorePathText.text = LocalizationHelper.L("EXPLORE_PATH_CLEAR");
        }
        else if (ratio >= 0.4f)
        {
            currentSuccessProb = 50f;
            explorePathText.text = LocalizationHelper.L("EXPLORE_PATH_FAMILIAR");
        }
        else
        {
            currentSuccessProb = 10f;
            explorePathText.text = LocalizationHelper.L("EXPLORE_PATH_CONFUSED");
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
        InventoryHolder holder = playerController.GetComponent<InventoryHolder>();
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
        PlayerInventory playerInv = playerController.GetComponent<PlayerInventory>();
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

            resultTitleText.text = LocalizationHelper.L("RESULT_EXPLORE_SUCCESS");
            resultInfoText.text = LocalizationHelper.L("RESULT_DESC_SUCCESS_SAFE");
        }
        else
        {
            resultTitleText.text = LocalizationHelper.L("RESULT_EXPLORE_COMPLETE");
            resultInfoText.text = LocalizationHelper.L("RESULT_DESC_SUCCESS_WALK");
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

        resultTitleText.text = LocalizationHelper.L("RESULT_EXPLORE_FAIL");
        resultInfoText.text = LocalizationHelper.L("RESULT_DESC_FAIL");

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