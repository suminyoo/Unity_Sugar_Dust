using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueClickUI : MonoBehaviour, IPointerClickHandler
{
    private DialogueManager manager;

    private void Awake()
    {
        manager = GetComponentInParent<DialogueManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null && manager.isDialogueActive)
        {
            manager.OnDialoguePanelClicked();
        }
    }
}