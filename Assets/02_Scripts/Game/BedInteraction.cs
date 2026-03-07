using UnityEngine;

public class BedInteraction : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt() => LocalizationHelper.L("PROMPT_SLEEP");

    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.L("CONFIRM_SLEEP"),
            () => {
                GameManager.Instance.TrySleep();
            }
        );
    }

}