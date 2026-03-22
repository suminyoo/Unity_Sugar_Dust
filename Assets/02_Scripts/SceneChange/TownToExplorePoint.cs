using UnityEngine;

public class TownToExplorePoint : MonoBehaviour, IInteractable
{
    [SerializeField] private ExploreSelectionUI selectionUI;

    public TutorialLockType tutorialLockType = TutorialLockType.Spaceship;

    public void OnInteract()
    {
        if (!InteractionValidator.CanInteract(tutorialLockType, out string rejectMsg))
        {
            NotificationUIManager.Instance.ShowNotification(rejectMsg);
            return;
        }

        if (GameManager.Instance.CanExplore())
        {
            selectionUI.OpenPanel();
        }
    }

    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_SPACESHIP_RIDE");
}