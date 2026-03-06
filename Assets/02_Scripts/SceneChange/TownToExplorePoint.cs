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

    public string GetInteractPrompt() => "우주선 타기";
}

