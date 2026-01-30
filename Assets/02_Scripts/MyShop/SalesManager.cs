using UnityEngine;

public class SalesManager : MonoBehaviour
{
    public static SalesManager Instance;

    public int totalSalesGold = 0;       // 총 매출 (물건 값)
    public int totalPenaltyAmount = 0;   // 실수로 인한 차감액 (페널티)
    public int mistakeCount = 0;         // 실수 횟수

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 하루 시작할 때 초기화
    public void StartNewDay()
    {
        totalSalesGold = 0;
        totalPenaltyAmount = 0;
        mistakeCount = 0;
    }

    // 거래 기록 (CounterUI에서 호출)
    public void RecordTransaction(int itemPrice, int expectedChange, int actualChangeGiven)
    {
        // 1. 기본 매출 등록
        totalSalesGold += itemPrice;

        // 2. 거스름돈 실수 계산

        int difference = Mathf.Abs(expectedChange - actualChangeGiven);

        if (difference > 0)
        {
            // 틀린 금액만큼 정산 때 차감?
            totalPenaltyAmount += difference;
            mistakeCount++;
            Debug.Log($"[정산 기록] 계산 실수! {difference}원 차이 발생.");
        }
        else
        {
            Debug.Log("[정산 기록] 완벽한 계산.");
        }
    }

    // 영업 종료 시 최종 금액 계산
    public int CalculateFinalProfit()
    {
        return totalSalesGold - totalPenaltyAmount;
    }
}