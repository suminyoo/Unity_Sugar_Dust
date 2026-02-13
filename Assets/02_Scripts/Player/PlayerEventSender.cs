using UnityEngine;

public class PlayerEventSender : MonoBehaviour
{
    private ActionSystem actionSystem;

    void Start()
    {
        actionSystem = GetComponentInParent<ActionSystem>();
    }

    public void EnableAttackHitBox() => actionSystem.OnAttackStart();
    

    public void DisableAttackHitBox() => actionSystem.OnAttackEnd();
    
}