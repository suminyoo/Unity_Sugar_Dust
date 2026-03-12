using UnityEngine;

public class BedInteraction : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_SLEEP");

    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.Main("CONFIRM_SLEEP"),
            () => {
                GameManager.Instance.TrySleep();
            }
        );
    }

}