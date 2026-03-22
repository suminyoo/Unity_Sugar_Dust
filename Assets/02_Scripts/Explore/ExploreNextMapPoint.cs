using UnityEngine;

public class ExploreNextMapPoint : MonoBehaviour, IInteractable
{
    public ExploreManager exploreManager;
    public SoundData walkSound;

    public TutorialLockType tutorialLockType = TutorialLockType.ExploreNextMap;

    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_EXPLORE_DEEPER");

    void Start()
    {
        exploreManager = FindAnyObjectByType<ExploreManager>();
    }

    public void OnInteract()
    {
        if (!InteractionValidator.CanInteract(tutorialLockType, out string rejectMsg))
        {
            NotificationUIManager.Instance.ShowNotification(rejectMsg);
            return;
        }

        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.Main("CONFIRM_MOVE_NEXT_AREA"),
            () =>
            {
                if (exploreManager != null)
                {
                    if (walkSound.clip != null) SoundManager.Instance.PlaySFX2D(walkSound);
                    exploreManager.AttemptMoveToNextStage();
                }
            }
        );
    }
}