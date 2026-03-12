using UnityEngine;
using System;
using System.Collections;

public enum GAME_TIME
{
    Morning,
    Day,
    Evening,
    Night,
    None
}
public class GameManager : MonoBehaviour, ISaveable
{
    public static event Action<GAME_TIME, bool> OnTimeChanged;
    public static GameManager Instance;

    public int currentDay = 0;
    public GAME_TIME currentTime = GAME_TIME.None;

    public event Action OnSleep;

    private GAME_TIME pendingTime;
    private bool hasPendingTimeChange = false;

    [Header("Sound")]
    public SoundData sleepSound;
    public SoundData morningSound;


    private void OnEnable()
    {
        SceneController.OnScreenFadedOut += ExecutePendingTimeChange;
    }

    private void OnDisable()
    {
        SceneController.OnScreenFadedOut -= ExecutePendingTimeChange;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        ApplyLoadedData();
    }

    public void PauseGameTime()
    {
        Time.timeScale = 0f;
        // 멈출때 음악이 작아진다던지
    }

    public void ResumeGameTime()
    {
        Time.timeScale = 1f;
    }

    public void ApplyLoadedData()
    {
        if (GameSaveManager.Instance != null)
        {
            var data = GameSaveManager.Instance.LoadGameState();
            currentDay = data.day;
            currentTime = data.time;

            ChangeTime(currentTime);

            //Debug.Log($"[GameManager] 세이브 데이터 적용 완료! 현재 시간: {currentTime}");
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.LoadSavedQuests();
        }
    }

    public void ChangeTime(GAME_TIME newTime, bool isInstant = true)
    {
        currentTime = newTime;
        OnTimeChanged?.Invoke(newTime, isInstant);
    }

    public void ChangeTimeAfterFadeOut(GAME_TIME newTime)
    {
        pendingTime = newTime;
        hasPendingTimeChange = true;
    }
    // 씬 컨트롤러가 페이드 아웃 직후에 날리는 이벤트를 받아서 실행
    private void ExecutePendingTimeChange()
    {
        if (hasPendingTimeChange)
        {
            ChangeTime(pendingTime, true);
            hasPendingTimeChange = false;
        }
    }

    #region 탐사 씬 플로우

    // 탐사 시작
    public void StartExploration(int exploreLevel)
    {
        ChangeTimeAfterFadeOut(GAME_TIME.Day);

        GameSaveManager.Instance.SaveSelectedExploreLevel(exploreLevel);
        SceneController.Instance.ChangeScene(SCENE_NAME.EXPLORE, SPAWN_ID.EXPLORE_START);
    }

    // 탐사 종료
    public void EndExploration(bool isSuccess)
    {
        ChangeTimeAfterFadeOut(GAME_TIME.Evening);

        if (isSuccess)
        {
            SceneController.Instance.ChangeScene(SCENE_NAME.TOWN, SPAWN_ID.TOWN_CENTER);
        }
        else
        {
            SceneController.Instance.ChangeSceneAndAddScene(SCENE_NAME.TOWN, SCENE_NAME.HOSPITAL_ROOM, SPAWN_ID.HOSPITAL_BED);
        }
    }

    #endregion

    #region 장사 씬 플로우
    public void StartShop()
    {
        MyShopManager.IsShopMode = true;
        SceneController.Instance.ChangeScene(SCENE_NAME.MY_SHOP, SPAWN_ID.MYSHOP_OPEN);
    }

    public void EndShop()
    {
        ChangeTimeAfterFadeOut(GAME_TIME.Night);
    }
    #endregion

    #region 행동 가능 여부 체크

    public bool CanExplore()
    {
        if (currentTime == GAME_TIME.Evening || currentTime == GAME_TIME.Night)
        {
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.Main("NOTI_TOO_COLD"));
            return false;
        }
        return true;
    }

    public bool CanShop()
    {
        if (currentTime == GAME_TIME.Morning || currentTime == GAME_TIME.Day)
        {
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.Main("NOTI_SHOP_EVENING_ONLY"));
            return false;
        }
        if (currentTime == GAME_TIME.Night)
        {
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.Main("NOTI_SHOP_CLOSED"));
            return false;
        }
        return true;
    }

    #endregion

    public void TrySleep()
    {
        StartCoroutine(SleepCoroutine());
    }

    private IEnumerator SleepCoroutine()
    {
        InputControlManager.Instance.LockInput();
        yield return FadeUIManager.Instance.FadeOut();

        ChangeTime(GAME_TIME.Morning);
        currentDay++;
        OnSleep?.Invoke();

        yield return new WaitForSeconds(1.0f);
        if (sleepSound.clip != null) SoundManager.Instance.PlaySFX2D(sleepSound);

        //TODO: 저장? 아침이 밝아올 때(페이드 인 직후) UI에 N일차 아침 등등 UI추가

        yield return FadeUIManager.Instance.FadeIn();
        InputControlManager.Instance.UnlockInput();

        if (morningSound.clip != null) SoundManager.Instance.PlaySFX2D(morningSound);

    }

    public void SaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGameState(currentDay, currentTime);
            QuestManager.Instance.SaveData();
        }
    }

}