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
        string itemNotify = $"<color={warningColor}>(바닥에 떨어진 아이템은 사라집니다)</color>";

        if (currentState == MyShopState.TOWN_MODE)
        {
            if (!GameManager.Instance.CanShop()) return;

            popupMsg = "영업을 시작하겠습니까?";

            popupMsg = "영업을 시작하겠습니까?";
            CommonConfirmPopup.Instance.OpenPopup(
                popupMsg,
                () => { StartBusiness(); }
            );
            return;
        }

        // 영업 마감
        if (currentState == MyShopState.SHOP_OPEN)
        {
            popupMsg = $"아직 영업 중입니다.\n지금 마감하고 정산하겠습니까?\n{itemNotify}";
        }
        else if (currentState == MyShopState.SHOP_CLOSED)
        {
            popupMsg = $"영업이 종료되었습니다.\n오늘의 정산 내역을 확인하겠습니까?\n{itemNotify}";
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
        GameManager.Instance.StartShop();
    }

    public void FinishBusiness()
    {
        GameManager.Instance.EndShop();

        if (currentState == MyShopState.SHOP_OPEN)
        {
            MyShopManager.Instance.ForceEarlyClose();
        }
        MyShopManager.Instance.OpenSettlementUI();
    }

}