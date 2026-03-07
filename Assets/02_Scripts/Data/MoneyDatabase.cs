using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "MoneyDatabase", menuName = "Shop/Money Database")]
public class MoneyDatabase : ScriptableObject
{
    [System.Serializable]
    public struct MoneyVisualData
    {
        public string label;
        public int amount;
        public Vector2 size;

        [Header("Real Money")]
        public List<Sprite> realMoneySprites;

        [Header("Fake Money")]
        public List<Sprite> fakeMoneySprites;
    }

    public List<MoneyVisualData> moneyList;

    public MoneyVisualData GetData(int amount)
    {
        return moneyList.FirstOrDefault(x => x.amount == amount);
    }

    public Sprite GetRandomSprite(int amount, bool isFake)
    {
        var data = GetData(amount);
        if (data.amount == 0) return null;

        List<Sprite> targetList = isFake ? data.fakeMoneySprites : data.realMoneySprites;

        if (targetList != null && targetList.Count > 0)
        {
            return targetList[Random.Range(0, targetList.Count)];
        }
        return null;
    }

    // ÁøÂ¥ µ· Áß¿¡ ±ú²ýÇÑ°Í¸¸ ¹ÝÈ¯
    public Sprite GetFirstRealSprite(int amount)
    {
        var data = GetData(amount);
        if (data.amount == 0) return null;

        if (data.realMoneySprites != null && data.realMoneySprites.Count > 0)
        {
            return data.realMoneySprites[0];
        }
        return null;
    }
}