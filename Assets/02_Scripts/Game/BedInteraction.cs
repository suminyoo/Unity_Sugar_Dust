using UnityEngine;

public class BedInteraction : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt() => "[E] 잠자기";

    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            "잠자리에 드시겠습니까?",
            () => {
                SleepAndNextDay();
            }
        );
    }

    private void SleepAndNextDay()
    {
        bool isSleepSuccess = GameManager.Instance.TrySleep();

        if (!isSleepSuccess)
        {
            NotificationUIManager.Instance.ShowNotification("아직 잘 시간이 아니다.");
            return;
        }

        // 성공했을 때의 연출 (짹짹 소리, 화면 페이드 인/아웃 등
        Debug.Log(" 아침이 되었습니다.");
    }
}