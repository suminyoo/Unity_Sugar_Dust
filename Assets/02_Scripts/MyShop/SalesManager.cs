using UnityEngine;

public struct SettlementData
{
    public int totalSales;
    public int fakeMoneyTotal;
    public int refusalCount;
    public int mistakeCount;

    // 돈 관련 결과
    public int baseRentCost;    // 기본 10% 임대료 금액
    public int penaltyRentCost; // 실수로 추가된 페널티 금액
    public int totalRent;       // 최종 임대료 (기본 + 페널티)
    public int netProfit;       // 순수익

    // UI 표시용 비율
    public float baseRate;      // 10%
    public float penaltyRate;   // 추가된 %
    public float totalRate;     // 최종 %
}

public class SalesManager : MonoBehaviour
{
    public static SalesManager Instance;

    [Header("임대료와 패널티 세팅")]
    [Range(0f, 1f)] public float baseRentRate = 0.10f; // 기본 수수료 10%
    [Range(0f, 1f)] public float ratePerRefusal = 0.05f; // 거절 1회당 5%
    [Range(0f, 1f)] public float ratePerMistake = 0.05f; // 실수 1회당 5%

    [Header("Today Record")]
    public int totalSalesGold = 0;
    public int totalFakeMoney = 0;
    public int refusedCount = 0;
    public int mistakeCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void StartNewShop()
    {
        totalSalesGold = 0;
        totalFakeMoney = 0;
        refusedCount = 0;
        mistakeCount = 0;
    }

    // 거래 기록 로직
    public void RecordSuccessTransaction(int itemPrice, int requiredChange, int actualChangeGiven, int acceptedFakeAmount)
    {
        totalSalesGold += itemPrice;
        if (acceptedFakeAmount > 0) totalFakeMoney += acceptedFakeAmount;
        if (requiredChange != actualChangeGiven) mistakeCount++;
    }

    public void RecordRefusal(CustomerType type)
    {
        // 평판 영향 없는 손님은 제외
        if (type == CustomerType.Beggar || type == CustomerType.Scammer || type == CustomerType.Haggler) return;
        refusedCount++;
    }

    public SettlementData CalculateCloseReceipt()
    {
        //추가 페널티 비율
        float addedRate = (refusedCount * ratePerRefusal) + (mistakeCount * ratePerMistake);

        // 최종 비율 (기본 10% + 추가 %)
        // 우선 최대 100%를 넘지 않게 제한 (1.0이나 1.5?)
        float finalRate = Mathf.Min(baseRentRate + addedRate, 1.0f);
        
        // 계산
        int baseCost = (int)(totalSalesGold * baseRentRate);
        int penaltyCost = (int)(totalSalesGold * addedRate);
        int totalRent = (int)(totalSalesGold * finalRate);

        // 순수익 = 매출 - 위폐 - 총임대료
        int netProfit = totalSalesGold - totalFakeMoney - totalRent;

        return new SettlementData()
        {
            totalSales = totalSalesGold,
            fakeMoneyTotal = totalFakeMoney,
            refusalCount = refusedCount,
            mistakeCount = mistakeCount,

            baseRentCost = baseCost,
            penaltyRentCost = penaltyCost,
            totalRent = totalRent,
            netProfit = netProfit,

            baseRate = baseRentRate * 100f,
            penaltyRate = addedRate * 100f,
            totalRate = finalRate * 100f
        };
    }
}