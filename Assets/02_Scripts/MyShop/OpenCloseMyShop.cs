using UnityEngine;

public class OpenCloseMyShop : MonoBehaviour, IInteractable 
{
    [SerializeField] private SCENE_NAME shopSceneName = SCENE_NAME.MY_SHOP;
    [SerializeField] private SPAWN_ID businessStartSpawnID = SPAWN_ID.MYSHOP_OPEN; // 장사 시작 시 플레이어 위치


    public void OnInteract()
    {
        Debug.Log("장사를 시작합니다. 마을 씬을 언로드");
        SceneController.Instance.ChangeScene(shopSceneName, businessStartSpawnID);
    }

    public string GetInteractPrompt() => "[E] 상점 운영 시작하기";


    public void FinishBusinessAndGoTown(SPAWN_ID townDoorSpawnID)
    {
        Debug.Log("장사를 마치고 마을로 돌아갑니다.");
        // 마을 씬을 Single로 로드
        SceneController.Instance.ChangeScene(SCENE_NAME.TOWN, townDoorSpawnID);
    }


}