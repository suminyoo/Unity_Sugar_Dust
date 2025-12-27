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

    [Header("Scene Navigation")]
    public string townSceneName = "TownScene";

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        player.gameObject.SetActive(false);

        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").GetComponent<Transform>();

        if (resultUIPanel != null) resultUIPanel.SetActive(false);
        mapSpawner.OnMapGenerationComplete += OnMapReady;
        OnMapReady();
        
    }

    void OnDestroy()
    {
        if (player != null) player.OnPlayerDied -= OnPlayerDeath;
        if (mapSpawner != null) mapSpawner.OnMapGenerationComplete -= OnMapReady;
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

        Debug.Log("플레이어 사망 확인 -> 탐사 실패 처리");
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
            ExploreFail(true); // 시간 초과 처리
        }
    }

    // 시간 초과
    void ExploreFail(bool shouldKillPlayer)
    {
        if (isExplorationEnded) return;
        isExplorationEnded = true;

        if (resultMessageText != null) resultMessageText.text = "탐사 실패...";

        // 시간 초과로 인한 실패라면 플레이어를 죽임
        if (shouldKillPlayer && player != null)
        {
            player.Die(); // isExplorationEnded 체크 필요 (무한루프방지)
        }

        StartCoroutine(ProcessResultAndLeave());
    }

    // 탐사 성공
    public void ExploreSuccess()
    {
        if (isExplorationEnded) return;
        isExplorationEnded = true;

        Debug.Log("탈출 성공! 아이템 보존");

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