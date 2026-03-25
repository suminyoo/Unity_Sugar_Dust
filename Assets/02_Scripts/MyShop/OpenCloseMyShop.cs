using TMPro;
using UnityEngine;

public class OpenCloseMyShop : MonoBehaviour, IInteractable
{
    public enum MyShopState { TOWN_MODE, SHOP_OPEN, SHOP_CLOSED }
    private MyShopState currentState = MyShopState.TOWN_MODE;

    public string warningColor = "#cc5e69";
    public TextMeshPro panelText;


    public void SetState(MyShopState state)
    {
        currentState = state;

        if (currentState == MyShopState.SHOP_OPEN)
        {
            panelText.text = "OPEN";
        }
        else
        {
            panelText.text = "CLOSED";
        }

    }

    public string GetInteractPrompt()
    {
        switch (currentState)
        {
            case MyShopState.TOWN_MODE:
                return LocalizationHelper.Main("PROMPT_SHOP_OPEN");
            case MyShopState.SHOP_OPEN:
                return LocalizationHelper.Main("PROMPT_SHOP_EARLY_CLOSE");
            case MyShopState.SHOP_CLOSED:
                return LocalizationHelper.Main("PROMPT_SHOP_CLOSE");
            default: return "";
        }
    }

    public void OnInteract()
    {
        // 영업 시작
        string popupMsg = "";
        string itemNotify = LocalizationHelper.Main("MYSHOP_WARNING", warningColor);
        if (currentState == MyShopState.TOWN_MODE)
        {
            if (!GameManager.Instance.CanShop()) return;

            popupMsg = LocalizationHelper.Main("CONFIRM_SHOP_OPEN");

            CommonConfirmPopup.Instance.OpenPopup(
                popupMsg,
                () => { StartBusiness(); }
            );
            return;
        }

        // 영업 마감
        if (currentState == MyShopState.SHOP_OPEN)
        {
            popupMsg = LocalizationHelper.Main("CONFIRM_EARLY_CLOSE", itemNotify);
        }
        else if (currentState == MyShopState.SHOP_CLOSED)
        {
            popupMsg = LocalizationHelper.Main("CONFIRM_SHOP_SETTLEMENT", itemNotify);
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