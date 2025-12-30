using UnityEngine;

public class NextMapPoint : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public GridMapSpawner mapSpawner;
    public CommonConfirmPopup popupUI;

    public string GetInteractPrompt() => "[E] 더 깊은 곳 탐사하기";

    public void OnInteract()
    {
        popupUI.OpenPopup(
            "다음 구역으로 이동하시겠습니까?", 
            () => {
                if(mapSpawner != null) 
                    mapSpawner.RestartMap();
            }
        );

    }
}
