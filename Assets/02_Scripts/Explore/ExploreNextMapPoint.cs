using UnityEngine;

public class ExploreNextMapPoint : MonoBehaviour, IInteractable
{
    public ExploreManager exploreManager;
    public AudioClip walkSound;

    public string GetInteractPrompt() => "[E] 더 깊은 곳 탐사하기";


    void Start()
    {
        exploreManager = Object.FindAnyObjectByType<ExploreManager>();
    }

    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            "다음 구역으로 이동하시겠습니까?", 
            () =>
            {
                if (exploreManager != null)
                {
                    if (walkSound != null) SoundManager.Instance.PlaySFX2D(walkSound);
                    exploreManager.AttemptMoveToNextStage();
                }
                else
                {
                    Debug.LogError("ExploreManager를 찾을 수 없음");
                }
            }
        );

    }
}
