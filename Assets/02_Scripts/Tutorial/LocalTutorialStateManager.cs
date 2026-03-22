using UnityEngine;

public class LocalTutorialStateManager : MonoBehaviour
{
    public GameObject tutorialGroup;
    public GameObject normalGroup;

    private void Start()
    {
        if (GameSaveManager.Instance == null) return;

        if (GameSaveManager.Instance.IsTutorialCompleted())
        {
            if (tutorialGroup != null) tutorialGroup.SetActive(false);
            if (normalGroup != null) normalGroup.SetActive(true);
        }
        else
        {
            if (tutorialGroup != null) tutorialGroup.SetActive(true);
            if (normalGroup != null) normalGroup.SetActive(false);
        }
    }
}