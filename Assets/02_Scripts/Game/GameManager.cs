using UnityEngine;
using System;

public enum GAME_TIME
{
    Morning,
    Day,
    Evening,
    Night
}
public class GameManager : MonoBehaviour, ISaveable
{
    public static event Action<GAME_TIME> OnTimeChanged;
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
        if (GameSaveManager.Instance != null)
        {
            var data = GameSaveManager.Instance.LoadGameState();
            currentDay = data.day;
            currentTime = data.time;
            OnTimeChanged?.Invoke(currentTime);
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

    private void ChangeTime(GAME_TIME newTime)
    {
        currentTime = newTime;
        Debug.Log($"시간 변경 알림: {newTime}");
        OnTimeChanged?.Invoke(newTime);
    }

    public bool TrySleep()
    {
        if (currentTime != GAME_TIME.Night) return false;
        
        currentDay++;
        ChangeTime(GAME_TIME.Morning);
        OnSleep?.Invoke();
        return true;
    }

    public void SaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGameState(currentDay, currentTime);
        }
    }

}