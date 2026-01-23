using UnityEngine;

public class OpenCloseMyShop : MonoBehaviour, IInteractable
{
    public enum MyShopState { TOWN_MODE, SHOP_OPEN, SHOP_CLOSED }
    private MyShopState currentState = MyShopState.TOWN_MODE;

    public void SetState(MyShopState state) => currentState = state;
    

    public string GetInteractPrompt()
    {
        switch (currentState)
        {
            case MyShopState.TOWN_MODE: return "[E] 상점 운영 시작하기";
            case MyShopState.SHOP_OPEN: return "[E] 상점 일찍 마감하기";
            case MyShopState.SHOP_CLOSED: return "[E] 상점 마감하기";
            default: return "";
        }
    }

    public void OnInteract()
    {
        string popupMsg = "";

        if (currentState == MyShopState.TOWN_MODE)
        {
            popupMsg = "영업을 시작하겠습니까?";

            CommonConfirmPopup.Instance.OpenPopup(
                popupMsg,
                () => {
                    MyShopManager.IsShopMode = true; // 씬 로드되면 장사 시작하도록
                    SceneController.Instance.ChangeScene(SCENE_NAME.MY_SHOP, SPAWN_ID.MYSHOP_OPEN);
                }
            );
            return;
        }

        // 조기 마감
        if (currentState == MyShopState.SHOP_OPEN)
        {
            popupMsg = "아직 영업 중입니다.\n<color=red>(진열대에 놓이지 않은 아이템은 회수</color>됩니다)\n마감하겠습니까?";
        }
        // 정상 마감
        else if (currentState == MyShopState.SHOP_CLOSED)
        {
            // 남아있는 손님 체크
            if (!MyShopManager.Instance.CanExitShop())
            {
                popupMsg = "아직 손님이 남아있습니다.\n<color=red>(진열대에 놓이지 않은 아이템은 회수</color>됩니다)\n마감하겠습니까?";
            }
            else
            {
                popupMsg = "오늘 영업을 마치겠습니까?\n<color=red>(진열대에 놓이지 않은 아이템은 회수</color>됩니다)";
            }
        }

        // 메시지가 설정된 경우만 팝업
        if (!string.IsNullOrEmpty(popupMsg))
        {
            CommonConfirmPopup.Instance.OpenPopup(
                popupMsg,
                () => { FinishBusinessAndGoTown(); }
            );
        }
    }

    public void FinishBusinessAndGoTown()
    {

        SceneController.Instance.ChangeSceneAndAddScene(
            SCENE_NAME.TOWN, 
            SCENE_NAME.MY_SHOP, 
            SPAWN_ID.MYSHOP_FRONTDOOR
        );
    }
}