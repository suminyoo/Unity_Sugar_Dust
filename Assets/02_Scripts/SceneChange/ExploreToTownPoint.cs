using System;
using UnityEngine;

public class ExploreToTownPoint : MonoBehaviour, IInteractable
{
    public static event Action<bool> OnPlayerReturnToTown;
    public SoundData walkSound;

    public TutorialLockType tutorialLockType = TutorialLockType.WalkToTown;

    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_RETURN_TO_TOWN");

    public void OnInteract()
    {
        if (!InteractionValidator.CanInteract(tutorialLockType, out string rejectMsg))
        {
            NotificationUIManager.Instance.ShowNotification(rejectMsg);
            return;
        }

        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.Main("CONFIRM_WALK_TO_TOWN"),
            () => {
                if (walkSound.clip != null)
                    SoundManager.Instance.PlaySFX2D(walkSound);

                OnPlayerReturnToTown?.Invoke(false);
            }
        );
    }
}