using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("UI Reference")]
    public CanvasGroup fadeCanvasGroup; //아직 미사용 페이드용 검은화면
    public float fadeDuration = 1.0f;

    // 다음 씬으로 넘겨줄 목적지 ID
    public int targetSpawnPointID { get; private set; }

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

    public void LoadScene(string sceneName, int spawnPointID)
    {
        StartCoroutine(TransitionRoutine(sceneName, spawnPointID));
    }

    private IEnumerator TransitionRoutine(string sceneName, int spawnPointID)
    {
        // 이동할 목적지 ID
        targetSpawnPointID = spawnPointID;

        // 2. 플레이어 조작 비활성화 (선택 사항)
        // 만약 플레이어 조작을 막고 싶다면 여기서 처리

        //Fade Out
        yield return StartCoroutine(Fade(1f));

        // 데이터 저장
        // 씬에 있는 플레이어를 찾아서 데이터 저장
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var condition = player.GetComponent<PlayerCondition>();
            var inventory = player.GetComponent<PlayerInventory>();

            if (GameManager.Instance != null && condition != null && inventory != null)
            {
                // 인벤토리 슬롯과 스탯저장
                GameManager.Instance.SavePlayerState(
                    condition.currentHp,
                    condition.currentStamina,
                    inventory.InventorySystem.slots
                );
            }
        }

        // 씬 로드
        yield return SceneManager.LoadSceneAsync(sceneName);

        yield return null;

        // Fade In
        yield return StartCoroutine(Fade(0f));
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
}