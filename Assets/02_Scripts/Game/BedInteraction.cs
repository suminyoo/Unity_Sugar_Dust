using UnityEngine;

public class BedInteraction : MonoBehaviour, IInteractable
{
    [Header("Tutorial Lock")]
    public TutorialLockType tutorialLockType = TutorialLockType.Bed;

    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_SLEEP");

    public void OnInteract()
    {
        if (!InteractionValidator.CanInteract(tutorialLockType, out string rejectMsg))
        {
            NotificationUIManager.Instance.ShowNotification(rejectMsg);
            return;
        }

        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.Main("CONFIRM_SLEEP"),
            () => {
                Quest tuto10 = QuestManager.Instance.GetActiveQuest(QuestID.Tuto_10);
                if (tuto10 != null)
                {
                    QuestManager.Instance.MaxOutQuestObjectives(QuestID.Tuto_10);
                    GameSaveManager.Instance.CompleteTutorial();
                    //Debug.Log("튜토리얼 완료");
                }

                GameManager.Instance.TrySleep();
            }
        );
    }
}