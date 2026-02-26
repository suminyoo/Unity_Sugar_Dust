using UnityEngine;
using System;
using System.Collections;

public enum GAME_TIME
{
    Morning,
    Day,
    Evening,
    Night
}
public class GameManager : MonoBehaviour, ISaveable
{
    public static event Action<GAME_TIME, bool> OnTimeChanged;
    public static GameManager Instance;

    public int currentDay = 1;
    public GAME_TIME currentTime = GAME_TIME.Morning;

    public event Action OnSleep;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        ApplyLoadedData();
    }

    public void ApplyLoadedData()
    {
        if (GameSaveManager.Instance != null)
        {
            var data = GameSaveManager.Instance.LoadGameState();
            currentDay = data.day;
            currentTime = data.time;

            ChangeTime(currentTime, true);

            Debug.Log($"[GameManager] 세이브 데이터 적용 완료! 현재 시간: {currentTime}");
        }
    }

    // 임시 디버깅용 (시간 조작)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeTime(GAME_TIME.Morning);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeTime(GAME_TIME.Day);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeTime(GAME_TIME.Evening);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeTime(GAME_TIME.Night);
    }

    public void ChangeTime(GAME_TIME newTime, bool isInstant = true)
    {
        currentTime = newTime;
        Debug.Log($"시간 변경 알림: {newTime}");
        OnTimeChanged?.Invoke(newTime, isInstant);
    }

    #region 탐사 씬 플로우

    // 탐사 시작
    public void StartExploration(int exploreLevel)
    {
        ChangeTime(GAME_TIME.Day);
        GameSaveManager.Instance.SaveSelectedExploreLevel(exploreLevel);
        SceneController.Instance.ChangeScene(SCENE_NAME.EXPLORE, SPAWN_ID.EXPLORE_START);
    }

    // 탐사 종료
    public void EndExploration(bool isSuccess)
    {
        ChangeTime(GAME_TIME.Evening);

        if (isSuccess)
        {
            SceneController.Instance.ChangeScene(SCENE_NAME.TOWN, SPAWN_ID.TOWN_CENTER);
        }
        else
        {
            SceneController.Instance.ChangeSceneAndAddScene(
                SCENE_NAME.TOWN,
                SCENE_NAME.HOSPITAL_ROOM,
                SPAWN_ID.HOSPITAL_BED
            );
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
        ChangeTime(GAME_TIME.Night);
    }
    #endregion

    #region 행동 가능 여부 체크

    public bool CanExplore()
    {
        if (currentTime == GAME_TIME.Evening || currentTime == GAME_TIME.Night)
        {
            NotificationUIManager.Instance.ShowNotification("지금 나가기에는 너무 춥고 위험합니다.");
            return false;
        }
        return true;
    }

    public bool CanShop()
    {
        if (currentTime == GAME_TIME.Morning || currentTime == GAME_TIME.Day)
        {
            NotificationUIManager.Instance.ShowNotification("상점 영업은 저녁부터 가능합니다.");
            return false;
        }
        if (currentTime == GAME_TIME.Night)
        {
            NotificationUIManager.Instance.ShowNotification("오늘은 이미 장사를 마감했습니다.");
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

        currentDay++;
        ChangeTime(GAME_TIME.Morning);
        OnSleep?.Invoke();

        yield return new WaitForSeconds(1.0f);

        yield return FadeUIManager.Instance.FadeIn();
        InputControlManager.Instance.UnlockInput();
    }

    public void SaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGameState(currentDay, currentTime);
        }
    }

}