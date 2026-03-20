using UnityEngine;

public class BedInteraction : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_SLEEP");

    public void OnInteract()
    {
        // 튜토리얼 아직 진행중
        if (!GameSaveManager.Instance.IsTutorialCompleted())
        {
            Quest tuto10 = QuestManager.Instance.GetActiveQuest(QuestID.Tuto_10);
            if (tuto10 == null)
            {
                NotificationUIManager.Instance.ShowNotification("아직 튜토리얼에서 해야 할 일이 남아있어 잠을 잘 수 없습니다.");
                return; 
            }
        }

        // 잠을 잘 수 있는 상태
        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.Main("CONFIRM_SLEEP"),
            () => {
                Quest tuto10 = QuestManager.Instance.GetActiveQuest(QuestID.Tuto_10);
                if (tuto10 != null)
                {
                    QuestManager.Instance.MaxOutQuestObjectives(QuestID.Tuto_10);

                    GameSaveManager.Instance.savedData.isTutorialCompleted = true;
                    Debug.Log("튜토리얼 완료");
                }

                // 수면 및 다음 날 이동 로직
                GameManager.Instance.TrySleep();
            }
        );
    }
}