using UnityEngine;

public class BedInteraction : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt() => "[E] 잠자기";

    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            "잠자리에 드시겠습니까?",
            () => {
                GameManager.Instance.TrySleep();
            }
        );
    }

}