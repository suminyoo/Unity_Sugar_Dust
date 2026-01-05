using UnityEngine;

// 시계 ui 관리 스크립트 (현재는 ExploreManager의 시간에 따라 시계바늘 회전)
public class DayNightClockUI : MonoBehaviour
{
    public ExploreManager exploreManager;
    public RectTransform clockDial;

    void Update()
    {
        if (exploreManager == null || clockDial == null) return;

        float currentTime = exploreManager.GetCurrentTime();
        float maxTime = exploreManager.GetTimeLimit();

        if (maxTime <= 0) return;

        float progress = Mathf.Clamp01(1.0f - (currentTime / maxTime));

        float targetAngle = progress * 180f;

        clockDial.localRotation = Quaternion.Euler(0, 0, -targetAngle);
    }
}