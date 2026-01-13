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
        SceneController.Instance.AddSceneAndMoveTo(targetSceneName, targetSpawnId, isExiting);
    }
}
