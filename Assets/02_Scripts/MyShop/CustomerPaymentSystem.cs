using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class CustomerPaymentSystem
{
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

    // 금액 결정 딱 떨어지면 그대로, 아니면 올림
    private static List<int> GetOverPayment(int targetAmount)
    {
        int[] roundUnits = { 1000, 5000, 10000, 50000 };
        List<int> potentialTotals = new List<int>();

        foreach (int unit in roundUnits)
        {
            int nextAmount;
            if (targetAmount % unit == 0) nextAmount = targetAmount;
            else nextAmount = ((targetAmount / unit) + 1) * unit;

            potentialTotals.Add(nextAmount);
        }

        potentialTotals = potentialTotals.Distinct().ToList();

        // 후보 중 하나 랜덤 선택
        int finalTotal = potentialTotals[Random.Range(0, potentialTotals.Count)];

        // 선택된 총액 그리디
        return GetExactPayment(finalTotal);
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