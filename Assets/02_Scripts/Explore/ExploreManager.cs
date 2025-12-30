using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ExploreManager : MonoBehaviour
{
    [Header("Exploration Settings")]
    public float timeLimit = 60f;
    private float currentTime;
    private bool isExplorationEnded = false;
    private bool isExploreStarted = false;
    private Transform playerSpawnPoint;


    [Header("References")]
    private PlayerController player;
    public GridMapSpawner mapSpawner;

    [Header("UI")]
    public GameObject resultUIPanel;
    public Text timerText;
    public Text resultMessageText; // 메시지 표시용

    [Header("Scene")]
    public string townSceneName = "TownScene";

    void Start()
    {
        SpaceShipLandingSpot.OnPlayerReturnToTown += ExploreSuccess; //동적으로 생성되는 오브젝트

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").GetComponent<Transform>();

        if (resultUIPanel != null) resultUIPanel.SetActive(false);
        mapSpawner.OnMapGenerationComplete += OnMapReady;
        
    }

    void OnDestroy()
    {
        SpaceShipLandingSpot.OnPlayerReturnToTown -= ExploreSuccess;
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
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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

        if (resultMessageText != null) resultMessageText.text = "탐사 실패...";

        //TODO: 아이템 소실 처리

        StartCoroutine(ProcessResultAndLeave());
    }

    // 탐사 성공
    public void ExploreSuccess()
    {
        if (isExplorationEnded) return;
        isExplorationEnded = true;

        Debug.Log("탐사 성공");

        if (resultMessageText != null) resultMessageText.text = "탐사 성공! 무사히 귀환합니다.";

        if (player != null) player.Wait();

        StartCoroutine(ProcessResultAndLeave());
    }
    IEnumerator ProcessResultAndLeave()
    {
        yield return new WaitForSeconds(3.0f);

        // 결과창 보는 ui BUTTON
        if (resultUIPanel != null)
        {
            resultUIPanel.SetActive(true);

            // 획득한 아이템 목록 띄우기 로직
        }

        // TODO: 버튼으로 마을이동

        Debug.Log("마을로 귀환합니다...");
        SceneManager.LoadScene(townSceneName);
    }
}