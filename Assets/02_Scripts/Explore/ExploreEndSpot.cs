using UnityEngine;
using System;
using UnityEngine.Localization.Settings;

public class ExploreEndSpot : MonoBehaviour, IInteractable
{
    public static event Action<bool> OnPlayerReturnToTown;

    public string GetInteractPrompt()
    {
        return LocalizationHelper.Main("PROMPT_SPACESHIP_CALL");
    }

    public void OnInteract()
    {
        string localizedMsg = LocalizationHelper.Main("CONFIRM_RETURN_TOWN");
        CommonConfirmPopup.Instance.OpenPopup(
            localizedMsg,
            () => {
                OnPlayerReturnToTown?.Invoke(true);
            }
        );
    }

}
