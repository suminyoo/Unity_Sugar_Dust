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

    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_SPACESHIP_RIDE");
}

