using UnityEngine;

public class TownToExplorePoint : MonoBehaviour, IInteractable
{
    [SerializeField] private ExploreSelectionUI selectionUI;

    public void OnInteract()
    {
        if (GameManager.Instance.CanExplore())
        {
            selectionUI.OpenPanel();
        }
    }

    public string GetInteractPrompt() => LocalizationHelper.L("PROMPT_SPACESHIP_RIDE");
}

