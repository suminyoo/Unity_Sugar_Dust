using UnityEngine;

public class ShopInteractionZone : MonoBehaviour, IInteractable
{
    public DisplayStand targetDisplayStand;

    public string GetInteractPrompt()
    {
        if (targetDisplayStand != null)
            return targetDisplayStand.GetInteractPrompt();
        return "";
    }

    public void OnInteract()
    {
        if (targetDisplayStand != null)
        {
            targetDisplayStand.OnInteract();
        }
    }
}