using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public string placeName;
    public SCENE_NAME targetSceneName;
    public SPAWN_ID targetSpawnId;
    public bool isExiting = false;

    public string GetInteractPrompt() => $"[E] {placeName} {(isExiting ? "나가기" : "들어가기")}";

    public void OnInteract()
    {
        if (isExiting && MyShopManager.Instance != null && MyShopManager.Instance.IsShopOpen)
        {
            NotificationUIManager.Instance.ShowNotification("영업 중에는 나갈 수 없습니다.");
            return;
        }

        SceneController.Instance.AddSceneAndMoveTo(targetSceneName, targetSpawnId, isExiting);
    }
}

