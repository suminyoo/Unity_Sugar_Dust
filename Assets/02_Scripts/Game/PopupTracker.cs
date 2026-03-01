using UnityEngine;

public class PopupTracker : MonoBehaviour
{
    private void OnEnable()
    {
        PauseManager.openPopupCount++;
    }

    private void OnDisable()
    {
        if (PauseManager.openPopupCount > 0)
        {
            PauseManager.openPopupCount--;
        }
    }
}