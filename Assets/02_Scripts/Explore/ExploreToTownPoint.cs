using System;
using UnityEngine;

public class ExploreToTownPoint : MonoBehaviour, IInteractable
{
    public static event Action<bool> OnPlayerReturnToTown;

    public string GetInteractPrompt() => "[E] 마을로 돌아가기";

    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            "정말로 걸어서 마을로 돌아가시겠습니까?",
            () => {
                Debug.Log("마을로 이동 중...");
                OnPlayerReturnToTown?.Invoke(false);
            }
        );

    }
}
