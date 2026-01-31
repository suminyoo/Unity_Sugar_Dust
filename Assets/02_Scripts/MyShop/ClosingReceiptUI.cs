using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClosingReceiptUI : MonoBehaviour
{
    public GameObject panelRoot;
    public TextMeshProUGUI totalSalesText;
    public TextMeshProUGUI rentText;
    public TextMeshProUGUI penaltyText;
    public TextMeshProUGUI fakeMoneyText;
    public TextMeshProUGUI finalProfitText;

    public Button confirmButton;
    private int finalAmountToAdd = 0;

    private void Start()
    {
        panelRoot.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirmReceipt);
    }

    public void ShowReceipt()
    {
        if (SalesManager.Instance == null) return;

        SettlementData data = SalesManager.Instance.CalculateCloseReceipt();
        finalAmountToAdd = data.netProfit;

        // 확장 메서드 활용 (숫자 포맷팅)
        string symbol = CustomerPaymentSystem.CURRENCY_SYMBOL;

        totalSalesText.text = $"총 매출: +{data.totalSales:N0} {symbol}";

        rentText.text = $"기본 임대료 ({data.baseRate:F0}%): -{data.baseRentCost:N0} {symbol}";

        if (data.penaltyRentCost > 0)
        {
            penaltyText.text = $"평판 페널티 (+{data.penaltyRate:F0}%): -{data.penaltyRentCost:N0} {symbol}";
            penaltyText.color = Color.red;
        }
        else
        {
            penaltyText.text = "평판 페널티 없음 (완벽!)";
            penaltyText.color = Color.green;
        }

        fakeMoneyText.text = $"위조지폐 손실: -{data.fakeMoneyTotal:N0} {symbol}";

        finalProfitText.text = $"최종 순수익: {data.netProfit:N0} {symbol}";
        finalProfitText.color = (data.netProfit >= 0) ? Color.blue : Color.red;

        panelRoot.SetActive(true);
    }
    private void OnConfirmReceipt()
    {
        // 돈 지급
        PlayerAssetsManager.Instance.AddMoney(finalAmountToAdd);
        Debug.Log($"정산 완료! {finalAmountToAdd} 획득.");

        // 마을로 복귀
        SceneController.Instance.ChangeSceneAndAddScene(
            SCENE_NAME.TOWN,
            SCENE_NAME.MY_SHOP,
            SPAWN_ID.ROOM_SCENE_ENTRY
        );
    }

}