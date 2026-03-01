using System.Collections.Generic;
using UnityEngine;

public struct SettlementData
{
    public int totalSales;          // 총 매출

    // 위폐 관련
    public Dictionary<int, int> fakeBillCounts; // {금액: 개수}
    public int totalFakeLoss;       // 위폐로 인한 총 차감 금액

    // 횟수 정보
    public int refusalCount;        // 거절 횟수
    public int mistakeCount;        // 실수 횟수

    // 퍼센트 정보
    public float baseRentRatePercent;   // 기본 (10%)
    public float refusalRatePercent;    // 거절 페널티
    public float mistakeRatePercent;    // 실수 페널티
    public float totalRentRatePercent;  // 총 합계

    // 최종 금액
    public int totalRentCost;       // 총 임대료 금액
    public int netProfit;           // 최종 순수익
}

public class SalesManager : MonoBehaviour
{
    public static SalesManager Instance;
    
    [Header("Settings")]
    [Range(0f, 0.5f)] public float baseRentRate = 0.10f;   // 기본 10%
    [Range(0f, 0.5f)] public float ratePerRefusal = 0.03f; // 거절
    [Range(0f, 0.5f)] public float ratePerMistake = 0.05f; // 실수

    // 오늘 하루 기록
    public int totalSales = 0;
    public int totalFakeMoney = 0;
    public int refusedCount = 0;
    public int mistakeCount = 0;

    // 위폐 권종별 카운트
    private Dictionary<int, int> todayFakeCounts = new Dictionary<int, int>();

    private void Awake() { if (Instance == null) Instance = this; }

    public void StartNewShop()
    {
        totalSales = 0;
        totalFakeMoney = 0;
        refusedCount = 0;
        mistakeCount = 0;
        todayFakeCounts.Clear();

        foreach (int unit in CustomerPaymentSystem.AvailableCurrency)
        {
            todayFakeCounts[unit] = 0;
        }
    }

    public void RecordSuccessTransaction(int itemPrice, int requiredChange, int actualChangeGiven, List<int> acceptedFakeBills)
    {
        totalSales += itemPrice;

        if (acceptedFakeBills != null && acceptedFakeBills.Count > 0)
        {
            foreach (int fakeVal in acceptedFakeBills)
            {
                totalFakeMoney += fakeVal;
                if (todayFakeCounts.ContainsKey(fakeVal)) todayFakeCounts[fakeVal]++;
                else todayFakeCounts[fakeVal] = 1;
            }
        }

        if (requiredChange != actualChangeGiven) mistakeCount++;
    }

    public void RecordRefusal(CustomerType type)
    {
        if (type == CustomerType.Beggar || type == CustomerType.Scammer || type == CustomerType.Haggler) return;
        refusedCount++;
    }

    public SettlementData CalculateCloseReceipt()
    {
        // 비율 계산 (실수율 + 거절율 + 기본율)
        float p_refusal = refusedCount * ratePerRefusal;
        float p_mistake = mistakeCount * ratePerMistake;
        float p_base = baseRentRate;

        // 총 비율 (100% 넘지 않게 제한)
        float p_total = Mathf.Min(p_base + p_refusal + p_mistake, 1.0f);

        // 금액 계산
        int rentCost = (int)(totalSales * p_total);     // 총 임대료
        int netProfit = totalSales - totalFakeMoney - rentCost; // 순수익

        return new SettlementData()
        {
            totalSales = totalSales,

            fakeBillCounts = new Dictionary<int, int>(todayFakeCounts),
            totalFakeLoss = totalFakeMoney,

            refusalCount = refusedCount,
            mistakeCount = mistakeCount,

            // 퍼센트로 변환 (0.1 -> 10)
            baseRentRatePercent = p_base * 100f,
            refusalRatePercent = p_refusal * 100f,
            mistakeRatePercent = p_mistake * 100f,
            totalRentRatePercent = p_total * 100f,

            totalRentCost = rentCost,
            netProfit = netProfit
        };
    }
}