using UnityEngine;

public class ExploreStartInTown : MonoBehaviour, IInteractable
{
    //public static event Action OnPlayerGoExplore;
    public string exploreSceneName = "Explore";

    public void OnInteract()
    {
        // 팝업을 열면서 메시지와 할 일(람다식) 전달
        CommonConfirmPopup.Instance.OpenPopup(
            "탐사를 시작하시겠습니까?",
            () => {
                Debug.Log("탐사로 이동 중...");
                SceneController.Instance.LoadScene(exploreSceneName, SPAWN_ID.Explore_Start);
                //OnPlayerGoExplore?.Invoke();
            }
        );
    }

    public string GetInteractPrompt() => "[E] 우주선 타기";
}

