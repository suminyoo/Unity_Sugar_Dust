using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public CanvasGroup fadeCanvasGroup;
    public float defaultFadeDuration = 1.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 연결 확인 로그
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("Fade Canvas Group이 연결되지 않았습니다");
        }
        else
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(true); // 혹시 꺼져있을까봐 켬
        }
    }

    public Coroutine FadeOut(float duration = -1)
    {
        float time = duration < 0 ? defaultFadeDuration : duration;
        return StartCoroutine(FadeCor(1f, time)); // 불투명
    }

    public Coroutine FadeIn(float duration = -1)
    {
        float time = duration < 0 ? defaultFadeDuration : duration;
        return StartCoroutine(FadeCor(0f, time)); //투명
    }

    private IEnumerator FadeCor(float targetAlpha, float duration)
    {
        InputControlManager.Instance.LockInput();

        if (fadeCanvasGroup == null)
        {
            Debug.LogError(" CanvasGroup이 없습니다");
            yield break;
        }

        Debug.Log($" 페이드 목표 Alpha: {targetAlpha} / 시간: {duration}초");

        fadeCanvasGroup.blocksRaycasts = true;
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            fadeCanvasGroup.alpha = newAlpha;

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