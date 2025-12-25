using UnityEngine;

public class AnimationEventSender : MonoBehaviour
{
    private ActionSystem actionSystem;

    void Start()
    {
        actionSystem = GetComponentInParent<ActionSystem>();
    }

    public void OnActionEvent()
    {
        if (actionSystem != null)
        {
            actionSystem.ExecuteAction();
        }
    }
}