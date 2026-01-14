using System;
using UnityEngine;

// 게임 전체의 입력 상태를 관리하는 클래스
public class InputControlManager : MonoBehaviour
{
    public static InputControlManager Instance;

    // 입력 잠금 카운터 (여러 시스템이 동시에 잠금을 요청할 수 있으므로 bool 대신 int)
    private int inputLockCount = 0;

    // 상태가 변할 때 알리는 이벤트
    public event Action<bool> OnInputStateChanged; // true: 입력 가능, false: 입력 불가

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

    }

    // 입력을 막아야 할 때 호출 ( 대화 시작, 맵 로딩 시작, 컷씬)
    public void LockInput()
    {
        inputLockCount++;
        if (inputLockCount == 1) // 0에서 1이 될 때 잠금 신호 발송
        {
            OnInputStateChanged?.Invoke(false);
            Debug.Log("Input Locked");
        }
    }

    // 입력을 풀어줄 때 호출 (대화 끝, 맵 로딩 끝, 컷씬)
    public void UnlockInput()
    {
        if (inputLockCount > 0)
        {
            inputLockCount--;
            if (inputLockCount == 0) // 모든 잠금이 해제되었을 때만 해제 신호 발송
            {
                OnInputStateChanged?.Invoke(true);
                Debug.Log("Input Unlocked");
            }
        }
    }
}