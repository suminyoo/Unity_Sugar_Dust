using UnityEngine;

public class OpenCloseMyShop : MonoBehaviour, IInteractable
{
    public enum MyShopState { TOWN_MODE, SHOP_OPEN, SHOP_CLOSED }
    private MyShopState currentState = MyShopState.TOWN_MODE;

    public void SetState(MyShopState state) => currentState = state;

    public string warningColor = "#cc5e69";

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
        // 영업 시작
        string popupMsg = "";
        string itemNotify = $"<color={warningColor}>진열대에 놓이지 않은 아이템은 인벤토리로 회수</color>됩니다";

        if (currentState == MyShopState.TOWN_MODE)
        {
            popupMsg = "영업을 시작하겠습니까?";

            popupMsg = "영업을 시작하겠습니까?";
            CommonConfirmPopup.Instance.OpenPopup(
                popupMsg,
                () => { StartBusiness(); } // ★ 깔끔해진 호출
            );
            return;
        }

        // 영업 마감
        if (currentState == MyShopState.SHOP_OPEN)
        {
            popupMsg = $"아직 영업 중입니다.\n{itemNotify}\n지금 마감하고 정산하겠습니까?";
        }
        else if (currentState == MyShopState.SHOP_CLOSED)
        {
            popupMsg = $"영업이 종료되었습니다.\n{itemNotify}\n오늘의 정산 내역을 확인하겠습니까?";
        }
        if (!string.IsNullOrEmpty(popupMsg))
        {
            CommonConfirmPopup.Instance.OpenPopup(
                popupMsg,
                () => { FinishBusiness(); }
            );
        }
    }

    public void StartBusiness()
    {
        MyShopManager.IsShopMode = true;
        SceneController.Instance.ChangeScene(SCENE_NAME.MY_SHOP, SPAWN_ID.MYSHOP_OPEN);
    }

    public void FinishBusiness()
    {
        if (currentState == MyShopState.SHOP_OPEN)
        {
            MyShopManager.Instance.ForceEarlyClose();
        }
        MyShopManager.Instance.OpenSettlementUI();
    }

}