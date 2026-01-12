using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public string placeName;
    public SCENE_NAME targetSceneName;
    public GameObject targetSpawnPosition;
    public bool isExiting = false;

    public string GetInteractPrompt() => $"[E] {placeName} {(isExiting ? "나가기" : "들어가기")}";

    public void OnInteract()
    {
        Vector3 spawnPos = targetSpawnPosition.transform.position;
        SceneLoadPortalManager.Instance.LoadAndMoveTo(targetSceneName, spawnPos, isExiting);
    }
}
