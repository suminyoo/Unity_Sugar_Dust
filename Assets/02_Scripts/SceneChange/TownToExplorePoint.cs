using UnityEngine;

public class TownToExplorePoint : MonoBehaviour, IInteractable
{
    //public static event Action OnPlayerGoExplore;

    public void OnInteract()
    {
        if (!GameManager.Instance.CanExplore()) return;


        int maxLevel = GameSaveManager.Instance.LoadExploreMaxUnlockedLevel();

        // 테스트용:s 지금은 일단 최고 레벨로 가게 설정
        CommonConfirmPopup.Instance.OpenPopup(
            $"{maxLevel:00} 구역 탐사를 시작하시겠습니까?",
            () => {
                GameManager.Instance.StartExploration(1);
            }
        ); ;
    }

    public string GetInteractPrompt() => "[E] 우주선 타기";
}

