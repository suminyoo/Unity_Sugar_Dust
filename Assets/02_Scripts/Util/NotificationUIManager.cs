using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationUIManager : MonoBehaviour
{
    public static NotificationUIManager Instance { get; private set; }

    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText;

    [SerializeField] private float defaultDisplayTime = 2.0f; // 기본 알림 표시 시간
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        notificationPanel.SetActive(false);
        
    }

    public void ShowNotification(string message, float displayTime = 0)
    {
        if (notificationPanel != null && notificationText != null)
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            notificationText.text = message;
            notificationPanel.SetActive(true);

            // 일정 시간 후 자동으로 닫기
            float time = displayTime > 0 ? displayTime : defaultDisplayTime;
            hideCoroutine = StartCoroutine(HideAfterDelayCor(time));
        }
    }

    private IEnumerator HideAfterDelayCor(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideNotification();
        hideCoroutine = null;
    }

    public void HideNotification()
    {
        notificationPanel.SetActive(false);
    }
}