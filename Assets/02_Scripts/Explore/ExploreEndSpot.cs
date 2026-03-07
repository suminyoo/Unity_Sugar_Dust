using UnityEngine;
using System;
using UnityEngine.Localization.Settings;

public class ExploreEndSpot : MonoBehaviour, IInteractable
{
    public static event Action<bool> OnPlayerReturnToTown;

    public string GetInteractPrompt()
    {
        return LocalizationHelper.L("PROMPT_SPACESHIP_CALL");
    }

    public void OnInteract()
    {
        string localizedMsg = LocalizationHelper.L("CONFIRM_RETURN_TOWN");
        CommonConfirmPopup.Instance.OpenPopup(
            localizedMsg,
            () => {
                OnPlayerReturnToTown?.Invoke(true);
            }
        );
    }

}
