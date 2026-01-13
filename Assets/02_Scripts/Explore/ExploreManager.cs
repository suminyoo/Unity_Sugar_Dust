using UnityEngine;
using TMPro;

public class ExploreManager : MonoBehaviour
{
    [Header("Exploration Settings")]
    public float timeLimit = 60f;
    private float currentTime;
    private bool isExplorationEnded = false;
    private bool isExploreStarted = false;
    private Transform playerSpawnPoint;
    private bool isExploreSuccess = false;

    [Header("References")]
    private PlayerController player;
    public GridMapSpawner mapSpawner;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject resultUIPanel;
    public TextMeshProUGUI resultMessageText; // 메시지 표시용
    public Transform resultItemContainer;
    public GameObject resultItemSlotPrefab;


    void Start()
    {
        ExploreEndSpot.OnPlayerReturnToTown += ExploreSuccess; //동적으로 생성되는 오브젝트

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").GetComponent<Transform>();

        if (resultUIPanel != null) resultUIPanel.SetActive(false);
        mapSpawner.OnMapGenerationComplete += OnMapReady;
        
    }

    void OnDestroy()
    {
        ExploreEndSpot.OnPlayerReturnToTown -= ExploreSuccess;
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

        currentTime = timeLimit;
        isExploreStarted = true;
    }

    void OnPlayerDeath()
    {
        if (isExplorationEnded) return;
        ExploreFail(false);
    }

    void Update()
    {
        if (isExplorationEnded) return;

        currentTime -= Time.deltaTime;

        if (timerText != null)
        {
            float displayTime = Mathf.Max(currentTime, 0);
            int minutes = Mathf.FloorToInt(displayTime / 60F);
            int seconds = Mathf.FloorToInt(displayTime % 60F);
            timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
        }

        if (currentTime <= 0)
        {
            //TODO: 얼어붙는 이펙트 활성화 
            //TODO: 시간 초과시 플레이어에게 지속적인 피해 주기
        }
    }

    void ExploreFail(bool shouldKillPlayer)
    {
        if (isExplorationEnded) return;
        isExplorationEnded = true;
        isExploreSuccess = false; 

        if (resultMessageText != null) resultMessageText.text = "탐사 실패...";

        //TODO: 아이템 소실 처리

        ShowResultItems();

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

    // 탐사 성공
    public void ExploreSuccess()
    {
        if (isExplorationEnded) return;
        isExplorationEnded = true;
        isExploreSuccess = true;

        Debug.Log("탐사 성공");

        if (resultMessageText != null) resultMessageText.text = "탐사 성공! 무사히 귀환합니다.";

        InputControlManager.Instance.LockInput();

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
}