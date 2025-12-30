using UnityEngine;
using UnityEngine.UI;

public class DayNightClockUI : MonoBehaviour
{
    [Header("References")]
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