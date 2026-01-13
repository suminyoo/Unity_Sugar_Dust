using UnityEngine;

public class ExploreStartInTown : MonoBehaviour, IInteractable
{
    //public static event Action OnPlayerGoExplore;

    public void OnInteract()
    {
        // 팝업을 열면서 메시지와 할 일(람다식) 전달
        CommonConfirmPopup.Instance.OpenPopup(
            "탐사를 시작하시겠습니까?",
            () => {
                Debug.Log("탐사로 이동 중...");
                SceneController.Instance.ChangeScene(SCENE_NAME.EXPLORE, SPAWN_ID.EXPLORE_START);
                //OnPlayerGoExplore?.Invoke();
            }
        );
    }

    public string GetInteractPrompt() => "[E] 우주선 타기";
}

