using UnityEngine;
using TMPro;
using System;

public class CommonConfirmPopup : MonoBehaviour
{
    public static CommonConfirmPopup Instance;

    [Header("UI Components")]
    public TextMeshProUGUI messageText;

    private Action onConfirm; // 네 눌렀을 때 실행할 함수 저장
    private Action onCancel;  // 아니오 눌렀을 때 실행할 함수 저장

    private void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ClosePopup();
    }

    void Update()
    {
        // ESC 키를 누르면 아니오와 똑같이 처리
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickNo();
        }
    }


    // 팝업 여는 함수 (외부호출)
    // message: 띄울 내용 /  confirmAction: 네 눌렀을 때 할 일
    public void OpenPopup(string message, Action confirmAction, Action cancelAction = null)
    {
        gameObject.SetActive(true); // 패널 켜기

        messageText.text = message; // 텍스트 변경
        onConfirm = confirmAction;  // 할 일 저장
        onCancel = cancelAction;
    }

    public void OnClickYes()
    {
        // 저장해둔 행동 실행 (씬 이동, 잠자기 등)
        onConfirm?.Invoke();

        ClosePopup();
    }

    public void OnClickNo()
    {
        // 취소 행동이 있다면 실행
        onCancel?.Invoke();

        ClosePopup();
    }

    private void ClosePopup()
    {
        gameObject.SetActive(false); // 패널 끄기

    }
}