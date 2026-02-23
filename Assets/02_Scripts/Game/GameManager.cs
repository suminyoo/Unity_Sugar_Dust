using UnityEngine;
using System;

public enum GAME_TIME
{
    Morning,    // 아침
    Day,        // 낮
    Evening,    // 저녁
    Night       // 밤
}
public class GameManager : MonoBehaviour
{
    public static event Action<GAME_TIME> OnTimeChanged;
    public static GameManager Instance;
    public GAME_TIME currentTime = GAME_TIME.Morning;

    public event Action OnSleep;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
        if (currentTime != GAME_TIME.Night)
        {
            return false;
        }

        ChangeTime(GAME_TIME.Morning);
        OnSleep?.Invoke();
        return true;
    }
}