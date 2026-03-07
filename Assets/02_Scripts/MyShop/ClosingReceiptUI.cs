using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClosingReceiptUI : MonoBehaviour
{
    public GameObject panelRoot;

    public TextMeshProUGUI totalSalesText;
    public TextMeshProUGUI[] fakeBillCountTexts;

    public TextMeshProUGUI totalFakeLossText;

    public TextMeshProUGUI baseRentText;          // 기본 임대료

    public TextMeshProUGUI refusalPenaltyText;     // 손님 거절
    public TextMeshProUGUI mistakePenaltyText;     // 계산 실수

    public TextMeshProUGUI totalRentText;          // 총 임대료
    public TextMeshProUGUI netProfitText;          // 총 수익

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
        string symbol = CustomerPaymentSystem.CURRENCY_SYMBOL;

        // 총 매출
        totalSalesText.text = $"+ {data.totalSales:N0} {symbol}";

        // 위조 화폐 각 개수 표기 (8개 텍스트박스 설정한 화폐단위랑 순서 맞춰야함)
        int[] units = CustomerPaymentSystem.AvailableCurrency;

        for (int i = 0; i < fakeBillCountTexts.Length; i++)
        {
            if (i < units.Length)
            {
                int unit = units[i];
                int count = 0;
                if (data.fakeBillCounts.ContainsKey(unit)) count = data.fakeBillCounts[unit];

                // 표기 형식: "1000 x 1"
                fakeBillCountTexts[i].text = $"{unit:N0} x {count}";
            }
            else
            {
                fakeBillCountTexts[i].text = "-";
            }
        }

        // 총 차감 금액 (위폐손실)
        totalFakeLossText.text = $"- {data.totalFakeLoss:N0} {symbol}";

        // 손님 거절 페널티
        refusalPenaltyText.text = LocalizationHelper.L("RECEIPT_PENALTY_FORMAT", data.refusalCount, data.refusalRatePercent);
        // 계산 실수 페널티
        mistakePenaltyText.text = LocalizationHelper.L("RECEIPT_PENALTY_FORMAT", data.mistakeCount, data.mistakeRatePercent);

        // 총 임대료: 총 임대료 (기본+거절+실수 %) : -금액
        totalRentText.text = $"- {data.totalRentRatePercent:F0}% : - {data.totalRentCost:N0} {symbol}";

        // 총 수익
        netProfitText.text = $"{data.netProfit:N0} {symbol}";
        netProfitText.color = (data.netProfit >= 0) ? Color.blue : Color.red;

        panelRoot.SetActive(true);
    }

    private void OnConfirmReceipt()
    {
        PlayerAssetsManager.Instance.AddMoney(finalAmountToAdd);

        SceneController.Instance.ChangeSceneAndAddScene(
            SCENE_NAME.TOWN,
            SCENE_NAME.MY_SHOP,
            SPAWN_ID.ROOM_SCENE_ENTRY
        );
    }
}