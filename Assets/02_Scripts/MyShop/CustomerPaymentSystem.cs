using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class CustomerPaymentSystem
{
    public const string CURRENCY_SYMBOL = "P";

    public static readonly int[] AvailableCurrency = { 50000, 10000, 5000, 1000, 500, 100, 50, 10 };

    public static List<int> GeneratePayment(int price, CustomerType type)
    {
        if (price <= 0) return new List<int>();

        switch (type)
        {
            case CustomerType.Normal_Exact:
                return GetExactPayment(price);

            case CustomerType.Normal_BigBill:
            case CustomerType.Tipper:
            case CustomerType.Disturber:
            case CustomerType.Impatient:
                return GetOverPayment(price);

            case CustomerType.CoinOnly:
                return GetCoinOnlyPayment(price);

            case CustomerType.Scammer:
                return (Random.value > 0.5f) ? GetExactPayment(price) : GetOverPayment(price);

            case CustomerType.Haggler:
                return GetHagglerPayment(price);

            case CustomerType.Beggar:
                return new List<int>();

            default:
                return GetExactPayment(price);
        }
    }

    // 그리디
    private static List<int> GetExactPayment(int targetAmount)
    {
        List<int> result = new List<int>();
        int remain = targetAmount;

        foreach (int unit in AvailableCurrency)
        {
            while (remain >= unit)
            {
                remain -= unit;
                result.Add(unit);
            }
        }
        return result;
    }

    /// 가격보다 큰 단위 중 가장 합리적인(가까운) 단위를 선택하거나,
    // 최고액권보다 비싼 경우 최고액권을 여러 장 사용하여 지불 총액을 결정
    private static List<int> GetOverPayment(int price)
    {
        int[] paymentUnits = { 1000, 5000, 10000, 50000 };

        // 가격보다 큰 화폐 단위 필터링
        var largerUnits = paymentUnits.Where(unit => unit > price).ToList();

        long finalAmountLong = 0;

        if (largerUnits.Count > 0)
        {
            // Case 1: 물건값보다 큰 단일 화폐가 존재하는 경우 
            // 가장 가까운 단위 1~2개 중에서만 선택
            int candidateCount = Mathf.Min(largerUnits.Count, 2);
            var candidates = largerUnits.Take(candidateCount).ToList();

            int selectedUnit = candidates[Random.Range(0, candidates.Count)];

            // 해당 단위 1장으로 지불
            finalAmountLong = selectedUnit;
        }
        else
        {
            // Case 2: 물건값이 최고액권(5만원)보다 비싼 경우
            // 최고액권을 기준으로 올림(Ceiling) 처리
            int maxUnit = paymentUnits.Last();

            if (price % maxUnit == 0)
            {
                finalAmountLong = price;
            }
            else
            {
                finalAmountLong = ((long)(price / maxUnit) + 1) * maxUnit;
            }
        }

        return GetExactPayment((int)finalAmountLong);
    }

    private static List<int> GetHagglerPayment(int originalPrice)
    {
        // 할인율
        int[] discounts = { 10, 20, 30, 40, 50 };
        int discountPercent = discounts[Random.Range(0, discounts.Length)];

        float factor = (100 - discountPercent) / 100f;
        int discountedPrice = (int)(originalPrice * factor);

        discountedPrice = (discountedPrice / 10) * 10;

        if (discountedPrice <= 0) discountedPrice = 10;

        return GetExactPayment(discountedPrice);
    }

    private static List<int> GetCoinOnlyPayment(int targetAmount)
    {
        List<int> result = new List<int>();
        int remain = targetAmount;

        // 동전만
        int[] coins = { 500, 100, 50, 10 };

        foreach (int coin in coins)
        {
            while (remain >= coin)
            {
                remain -= coin;
                result.Add(coin);
            }
        }
        return result;
    }
}