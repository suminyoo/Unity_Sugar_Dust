using UnityEngine;
using System.Collections;

public class SkyDome : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private MeshRenderer skyRenderer;
    [SerializeField] private float transitionDuration = 3f;

    [Header("Offset Values")]
    [SerializeField] private float morningOffset = 0.75f;
    [SerializeField] private float dayOffset = 0f;
    [SerializeField] private float eveningOffset = 0.25f;
    [SerializeField] private float nightOffset = 0.6f;

    [Header("References")]
    [SerializeField] private MeshRenderer sunRenderer;  
    [SerializeField] private MeshRenderer moonRenderer; 
    [SerializeField] private MeshRenderer starRenderer;
    private Coroutine currentTransition;

    private void OnEnable()
    {
        GameManager.OnTimeChanged += HandleTimeChange;
    }

    private void OnDisable()
    {
        GameManager.OnTimeChanged -= HandleTimeChange;
    }

    private void HandleTimeChange(GAME_TIME timeState)
    {
        float targetOffset = 0f;
        float targetSunAlpha = 0f;
        float targetMoonAlpha = 0f;

        // Enum
        switch (timeState)
        {
            case GAME_TIME.Day:
                targetOffset = dayOffset;
                targetSunAlpha = 1f;
                targetMoonAlpha = 0f;
                break;
            case GAME_TIME.Evening:
                targetOffset = eveningOffset;
                targetSunAlpha = 0f;
                targetMoonAlpha = 1f;
                break;
            case GAME_TIME.Night:
                targetOffset = nightOffset;
                targetSunAlpha = 0f;
                targetMoonAlpha = 1f;
                break;
            case GAME_TIME.Morning:
                targetOffset = morningOffset;
                targetSunAlpha = 1f;
                targetMoonAlpha = 0f;
                break;
        }

        // 코루틴
        if (currentTransition != null) StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(SmoothChangeSky(targetOffset, targetSunAlpha, targetMoonAlpha));
    }

    // 부드럽게 변함
    private IEnumerator SmoothChangeSky(float targetSkyX, float targetSunAlpha, float targetMoonAlpha)
    {
        {
            Material skyMat = skyRenderer.material;
            float startSkyX = skyMat.mainTextureOffset.x;

            // 해, 달, 별
            float startSunAlpha = sunRenderer ? sunRenderer.material.color.a : 0;
            float startMoonAlpha = moonRenderer ? moonRenderer.material.color.a : 0;

            // 역행 방지
            if (startSkyX > targetSkyX && Mathf.Abs(startSkyX - targetSkyX) > 0.5f)
                targetSkyX += 1.0f;

            float time = 0;
            while (time < transitionDuration)
            {
                time += Time.deltaTime;
                float t = time / transitionDuration;

                // 배경 이동
                float newX = Mathf.Lerp(startSkyX, targetSkyX, t);
                skyMat.mainTextureOffset = new Vector2(newX % 1.0f, 0);

                // 해 투명도
                if (sunRenderer != null) ChangeAlpha(sunRenderer, startSunAlpha, targetSunAlpha, t);

                // 달/별 투명
                if (moonRenderer != null) ChangeAlpha(moonRenderer, startMoonAlpha, targetMoonAlpha, t);
                if (starRenderer != null) ChangeAlpha(starRenderer, startMoonAlpha, targetMoonAlpha, t);

                yield return null;
            }

            // 최종값 확정
            skyMat.mainTextureOffset = new Vector2(targetSkyX % 1.0f, 0);
            if (sunRenderer) SetAlpha(sunRenderer, targetSunAlpha);
            if (moonRenderer) SetAlpha(moonRenderer, targetMoonAlpha);
            if (starRenderer) SetAlpha(starRenderer, targetMoonAlpha);
        }
    }

    private void ChangeAlpha(Renderer r, float start, float end, float t)
    {
        Color c = r.material.color;
        c.a = Mathf.Lerp(start, end, t);
        r.material.color = c;
    }

    private void SetAlpha(Renderer r, float alpha)
    {
        Color c = r.material.color;
        c.a = alpha;
        r.material.color = c;
    }
}