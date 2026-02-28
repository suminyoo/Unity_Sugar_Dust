using UnityEngine;
using TMPro;

public class ClockUI : MonoBehaviour
{
    [Header("Clock Images")]
    public GameObject morningImage;
    public GameObject dayToEveningImage;
    public GameObject eveningToNightImage;

    [Header("Texts")]
    public TextMeshProUGUI dayTimeText;
    public TextMeshProUGUI countdownText;

    public ExploreManager exploreManager;
    public MyShopManager shopManager;

    private RectTransform activeDial;

    private void OnEnable()
    {
        GameManager.OnTimeChanged += HandleTimeChanged;

        if (GameManager.Instance != null)
        {
            HandleTimeChanged(GameManager.Instance.currentTime, true);
        }
    }

    private void OnDisable()
    {
        GameManager.OnTimeChanged -= HandleTimeChanged;
    }

    private void HandleTimeChanged(GAME_TIME newTime, bool isInstant)
    {
        string timeName = "";
        switch (newTime)
        {
            case GAME_TIME.Morning: timeName = "¾ÆÄ§"; break;
            case GAME_TIME.Day: timeName = "³·"; break;
            case GAME_TIME.Evening: timeName = "Àú³á"; break;
            case GAME_TIME.Night: timeName = "¹ã"; break;
        }

        if (GameManager.Instance != null && dayTimeText != null)
        {
            dayTimeText.text = $"{GameManager.Instance.currentDay}ÀÏÂ÷ {timeName}";
        }

        morningImage.SetActive(false);
        dayToEveningImage.SetActive(false);
        eveningToNightImage.SetActive(false);

        switch (newTime)
        {
            case GAME_TIME.Morning:
                morningImage.SetActive(true);
                countdownText.gameObject.SetActive(false);
                activeDial = null;
                break;

            case GAME_TIME.Day:
                dayToEveningImage.SetActive(true);
                countdownText.gameObject.SetActive(true);
                activeDial = dayToEveningImage.GetComponent<RectTransform>();
                activeDial.localRotation = Quaternion.Euler(0, 0, 0);
                break;

            case GAME_TIME.Evening:
                eveningToNightImage.SetActive(true);
                countdownText.gameObject.SetActive(false);
                activeDial = eveningToNightImage.GetComponent<RectTransform>();
                activeDial.localRotation = Quaternion.Euler(0, 0, 0);
                break;

            case GAME_TIME.Night:
                eveningToNightImage.SetActive(true);
                countdownText.gameObject.SetActive(false);
                activeDial = eveningToNightImage.GetComponent<RectTransform>();
                activeDial.localRotation = Quaternion.Euler(0, 0, -180f);
                break;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.currentTime == GAME_TIME.Day)
        {
            UpdateExploreTimer();
        }
        else if (GameManager.Instance.currentTime == GAME_TIME.Evening)
        {
            UpdateShopTimer();
        }
    }

    private void UpdateExploreTimer()
    {
        if (exploreManager == null) return;
        float currentTime = exploreManager.GetCurrentTime();
        float maxTime = exploreManager.GetTimeLimit();
        UpdateTimerAndDial(currentTime, maxTime);
    }

    private void UpdateShopTimer()
    {
        if (MyShopManager.Instance == null)
        {
            if (countdownText.gameObject.activeSelf) countdownText.gameObject.SetActive(false);
            return;
        }

        if (MyShopManager.Instance == null || !MyShopManager.Instance.IsShopOpen)
        {
            if (countdownText.gameObject.activeSelf)
            {
                countdownText.gameObject.SetActive(false);
            }
            return;
        }

        if (!countdownText.gameObject.activeSelf)
        {
            countdownText.gameObject.SetActive(true);
        }

        float currentTime = MyShopManager.Instance.GetCurrentTime();
        float maxTime = MyShopManager.Instance.GetTimeLimit();

        UpdateTimerAndDial(currentTime, maxTime);
    }

    private void UpdateTimerAndDial(float currentTime, float maxTime)
    {
        if (maxTime <= 0) return;

        if (activeDial != null)
        {
            float progress = Mathf.Clamp01(1.0f - (currentTime / maxTime));
            float targetAngle = progress * 180f;
            activeDial.localRotation = Quaternion.Euler(0, 0, -targetAngle);
        }

        int minutes = Mathf.FloorToInt(currentTime / 60F);
        int seconds = Mathf.FloorToInt(currentTime % 60F);
        countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}