using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public CanvasGroup fadeCanvasGroup;
    public float defaultFadeDuration = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 연결 확인 로그
        if (fadeCanvasGroup == null)
        {
            Debug.LogError(" [UIManager] 치명적 오류: Fade Canvas Group이 연결되지 않았습니다! 인스펙터에서 할당해주세요.");
        }
        else
        {
            // 시작할 때 화면이 검은색이어야 한다면 1, 아니면 0으로 초기화
            // 보통 씬 시작 시 밝아져야 하니까 초기값을 1(검정)이나 0(투명)으로 잡습니다.
            // 여기서는 일단 투명(0)으로 둡니다.
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(true); // 혹시 꺼져있을까봐 켬
        }
    }

    public Coroutine FadeOut(float duration = -1)
    {
        float time = duration < 0 ? defaultFadeDuration : duration;
        return StartCoroutine(FadeCor(1f, time)); // 1f = 불투명 (어두워짐)
    }

    public Coroutine FadeIn(float duration = -1)
    {
        float time = duration < 0 ? defaultFadeDuration : duration;
        return StartCoroutine(FadeCor(0f, time)); // 0f = 투명 (밝아짐)
    }

    private IEnumerator FadeCor(float targetAlpha, float duration)
    {
        InputControlManager.Instance.LockInput();

        if (fadeCanvasGroup == null)
        {
            Debug.LogError(" [UIManager] CanvasGroup이 없습니다. 페이드를 스킵합니다.");
            yield break;
        }

        Debug.Log($" [UIManager] 페이드 시작! 목표 Alpha: {targetAlpha} / 시간: {duration}초");

        fadeCanvasGroup.blocksRaycasts = true;
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            fadeCanvasGroup.alpha = newAlpha;
            // Debug.Log($"Running Fade... Alpha: {newAlpha}"); // 너무 많이 뜨면 주석 처리

            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
            Debug.Log("페이드 인 완료");
        }
        else
        {
            Debug.Log("페이드 아웃 완료");
        }

        InputControlManager.Instance.UnlockInput();

    }
}