using System;
using UnityEngine;

public class ExploreToTownPoint : MonoBehaviour, IInteractable
{
    public static event Action<bool> OnPlayerReturnToTown;
    public SoundData walkSound;

    public string GetInteractPrompt() => LocalizationHelper.L("PROMPT_RETURN_TO_TOWN");
    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.L("CONFIRM_WALK_TO_TOWN"),
            () => {
                if (walkSound.clip != null) 
                    SoundManager.Instance.PlaySFX2D(walkSound);

                OnPlayerReturnToTown?.Invoke(false);
            }
        );

    }
}
