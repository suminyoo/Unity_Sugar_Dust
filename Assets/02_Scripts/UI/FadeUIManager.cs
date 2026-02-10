using UnityEngine;
using System.Collections;

public class FadeUIManager : MonoBehaviour
{
    public static FadeUIManager Instance;
    public CanvasGroup fadeCanvasGroup;
    public GameObject loadingIcon;
    private float defaultFadeDuration = 0.5f;

    private int fadeRequestCount = 0;
    private Coroutine currentFadeCoroutine; // 현재 실행 중인 코루틴 추적

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(true);
        }
    }

    public Coroutine FadeOut(float duration = -1)
    {
        fadeRequestCount++;

        // [핵심] 최초 요청 시에만 인풋을 잠급니다.
        if (fadeRequestCount == 1)
        {
            InputControlManager.Instance.LockInput();

            if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
            float time = duration < 0 ? defaultFadeDuration : duration;
            return currentFadeCoroutine = StartCoroutine(FadeCor(1f, time));
        }

        return StartCoroutine(EmptyCor());
    }

    public Coroutine FadeIn(float duration = -1)
    {
        fadeRequestCount--;
        if (fadeRequestCount < 0) fadeRequestCount = 0;

        if (loadingIcon != null) loadingIcon.SetActive(false);

        // [핵심] 마지막 요청일 때만 페이드 인 애니메이션을 실행하고 인풋을 해제합니다.
        if (fadeRequestCount == 0)
        {
            if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
            float time = duration < 0 ? defaultFadeDuration : duration;
            return currentFadeCoroutine = StartCoroutine(FadeCor(0f, time));
        }

        return StartCoroutine(EmptyCor());
    }

    private IEnumerator FadeCor(float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        // [핵심] 페이드 인(0f)이 완전히 끝난 시점에만 인풋과 레이캐스트를 해제합니다.
        if (targetAlpha == 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
            InputControlManager.Instance.UnlockInput(); // 여기서 확실히 해제
        }

        currentFadeCoroutine = null;
    }

    private IEnumerator EmptyCor()
    {
        yield break;
    }
}